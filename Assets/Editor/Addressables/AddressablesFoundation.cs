// Assets/Editor/AddressablesFoundation.cs
#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace SpaceTrader.Editor.Addressables
{
    public static class AddressablesFoundation
    {
        // ---------- BOOTSTRAP ----------
        [MenuItem("Tools/Addressables/Bootstrap Project")]
        public static void BootstrapProject()
        {
            var s = EnsureSettings();
            EnsureProfileVars(s);
            EnsureCoreGroups(s, new[] { "UI", "Ships", "FX", "Env", "Audio", "Scenes", "Data" });
            ForceLocalPathsAllGroups(s);
            AssetDatabase.SaveAssets();
            Debug.Log("[Addr] Bootstrap done.");
        }

        // ---------- CONVENTION REBUILD ----------
        [MenuItem("Tools/Addressables/Rebuild From Conventions")]
        public static void RebuildFromConventions()
        {
            var s = AddressableAssetSettingsDefaultObject.Settings;
            if (s == null) { Debug.LogError("No Addressables settings."); return; }

            // KURALLAR — kökler, arama filtresi, address prefix, grup, label
            var rules = new List<Rule>
        {
            // UI
            new Rule(new[]{ "Assets/Art/ui/icons", "Assets/Art/UI/Icons" }, "t:Sprite", "ui/icons", "UI", "ui"),
            // Ships
            new Rule(new[]{ "Assets/Art/ships/icons", "Assets/Art/Ships/Icons" }, "t:Sprite", "ships/icons", "Ships", "ships"),
            new Rule(new[]{ "Assets/Prefabs/Ships", "Assets/Ships/Prefabs" }, "t:Prefab", "ships/prefabs", "Ships", "ships"),
            new Rule(new[]{ "Assets/Data/Ships" }, "t:ScriptableObject", "ships/defs", "Ships", "ships"),
            // FX
            new Rule(new[]{ "Assets/Art/fx/sprites", "Assets/Art/VFX" }, "t:Sprite", "fx/sprites", "FX", "fx"),
            // ENV
            new Rule(new[]{ "Assets/Art/env/tilesets", "Assets/Art/Tiles" }, "t:Sprite", "env/tilesets", "Env", "env"),
            new Rule(new[]{ "Assets/Art/env/props" }, "t:Sprite", "env/props", "Env", "env"),
            // Audio
            new Rule(new[]{ "Assets/Audio/SFX" }, "t:AudioClip", "audio/sfx", "Audio", "audio"),
            new Rule(new[]{ "Assets/Audio/Music" }, "t:AudioClip", "audio/music", "Audio", "audio"),
            // Scenes
            new Rule(new[]{ "Assets/Scenes" }, "t:Scene", "scenes", "Scenes", "scenes"),
            // Data (genel)
            new Rule(new[]{ "Assets/Data" }, "t:ScriptableObject", "data", "Data", "data"),
        };

            int added = 0, moved = 0, touched = 0;

            foreach (var r in rules)
            {
                foreach (var root in r.Roots.Where(AssetDatabase.IsValidFolder))
                {
                    var guids = AssetDatabase.FindAssets(r.Filter, new[] { root }).Distinct();
                    foreach (var guid in guids)
                    {
                        var path = AssetDatabase.GUIDToAssetPath(guid);
                        if (AssetDatabase.IsValidFolder(path)) continue;

                        var id = Path.GetFileNameWithoutExtension(path);
                        var address = string.IsNullOrEmpty(r.AddressPrefix) ? id : (r.AddressPrefix + "/" + id);

                        var group = EnsureGroup(s, r.Group);
                        var entry = s.FindAssetEntry(guid);

                        if (entry == null)
                        {
                            entry = s.CreateOrMoveEntry(guid, group, false, false);
                            entry.address = address;
                            if (!string.IsNullOrEmpty(r.Label)) entry.SetLabel(r.Label, true, true);
                            added++;
                        }
                        else
                        {
                            bool ch = false;
                            if (entry.parentGroup != group) { s.CreateOrMoveEntry(guid, group, false, false); moved++; ch = true; }
                            if (entry.address != address) { entry.address = address; ch = true; }
                            if (ch) touched++;
                        }
                    }
                }
            }

            s.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
            AssetDatabase.SaveAssets();
            Debug.Log(string.Format("[Addr] Rebuild From Conventions → added:{0}, moved:{1}, updated:{2}", added, moved, touched));
        }

        // ---------- FIX: Empty/Duplicate Keys ----------
        [MenuItem("Tools/Addressables/Fix Empty or Duplicate Addresses")]
        public static void FixAddresses()
        {
            var s = AddressableAssetSettingsDefaultObject.Settings;
            if (s == null) { Debug.LogError("No Addressables settings."); return; }

            var used = new HashSet<string>();
            int fixedCount = 0;

            foreach (var g in s.groups.Where(g => g != null))
            {
                var entries = g.entries.ToList();
                foreach (var e in entries)
                {
                    if (e == null) continue;
                    var addr = e.address;
                    if (string.IsNullOrEmpty(addr))
                    {
                        var path = AssetDatabase.GUIDToAssetPath(e.guid);
                        addr = Path.GetFileNameWithoutExtension(path);
                        if (string.IsNullOrEmpty(addr)) addr = e.guid;
                        e.address = addr;
                        fixedCount++;
                    }

                    var baseAddr = e.address;
                    int i = 1;
                    while (!used.Add(e.address))
                        e.address = baseAddr + "_" + (i++);
                }
            }

            s.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, null, true);
            AssetDatabase.SaveAssets();
            Debug.Log("[Addr] Fixed " + fixedCount + " empty/duplicate addresses.");
        }

        // ---------- PURGE MISSING ----------
        [MenuItem("Tools/Addressables/Purge Missing Entries")]
        public static void PurgeMissing()
        {
            var s = AddressableAssetSettingsDefaultObject.Settings;
            if (s == null) return;
            int removed = 0;
            foreach (var g in s.groups.Where(g => g != null))
            {
                var entries = g.entries.ToList();
                foreach (var e in entries)
                {
                    if (string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(e.guid)))
                    {
                        g.RemoveAssetEntry(e);
                        removed++;
                    }
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Addr] Removed " + removed + " missing entries.");
        }

        // ---------- FORCE LOCAL PATHS ----------
        [MenuItem("Tools/Addressables/Force Local Paths (All Groups)")]
        public static void ForceLocalPathsMenu()
        {
            var s = AddressableAssetSettingsDefaultObject.Settings;
            if (s == null) { Debug.LogError("No Addressables settings."); return; }
            ForceLocalPathsAllGroups(s);
            AssetDatabase.SaveAssets();
            Debug.Log("[Addr] All groups now use LocalBuildPath / LocalLoadPath.");
        }

        // ---------- HEALTH CHECK ----------
        [MenuItem("Tools/Addressables/Health Check")]
        public static void HealthCheck()
        {
            var s = AddressableAssetSettingsDefaultObject.Settings;
            if (s == null) { Debug.LogError("No Addressables settings."); return; }

            var pid = s.activeProfileId;
            var lb = s.profileSettings.GetValueByName(pid, "LocalBuildPath");
            var ll = s.profileSettings.GetValueByName(pid, "LocalLoadPath");
            if (string.IsNullOrEmpty(lb) || string.IsNullOrEmpty(ll))
                Debug.LogWarning("[Addr] Profile vars missing. Expected LocalBuildPath & LocalLoadPath.");

            foreach (var g in s.groups.Where(g => g != null))
            {
                var schema = g.GetSchema<BundledAssetGroupSchema>();
                if (schema == null) { Debug.LogWarning("[Addr] Group without Bundled schema: " + g.Name); continue; }
                var bp = schema.BuildPath.GetValue(s);
                var lp = schema.LoadPath.GetValue(s);
                if (!bp.Contains("{UnityEngine.AddressableAssets.Addressables.BuildPath}"))
                    Debug.LogWarning("[Addr] " + g.Name + " BuildPath not dynamic: " + bp);
                if (!lp.Contains("{UnityEngine.AddressableAssets.Addressables.RuntimePath}"))
                    Debug.LogWarning("[Addr] " + g.Name + " LoadPath not dynamic: " + lp);

                foreach (var e in g.entries)
                {
                    if (string.IsNullOrEmpty(e.address))
                        Debug.LogWarning("[Addr] Empty address in group " + g.Name + " → guid: " + e.guid);
                }
            }
            Debug.Log("[Addr] HealthCheck complete.");
        }

        // ==================== helpers ====================
        class Rule
        {
            public string[] Roots; public string Filter; public string AddressPrefix; public string Group; public string Label;
            public Rule(string[] roots, string filter, string prefix, string group, string label)
            { Roots = roots; Filter = filter; AddressPrefix = prefix; Group = group; Label = label; }
        }

        static AddressableAssetSettings EnsureSettings()
        {
            var path = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
            var settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(path);
            if (settings == null)
            {
                if (!AssetDatabase.IsValidFolder("Assets/AddressableAssetsData"))
                    AssetDatabase.CreateFolder("Assets", "AddressableAssetsData");
                var dir = "Assets/AddressableAssetsData";
                settings = AddressableAssetSettings.Create(dir, "AddressableAssetSettings", true, true);
            }
            AddressableAssetSettingsDefaultObject.Settings = settings;
            return settings;
        }

        static void EnsureProfileVars(AddressableAssetSettings s)
        {
            var p = s.profileSettings;
            EnsureVar(p, s.activeProfileId, "LocalBuildPath", "{UnityEngine.AddressableAssets.Addressables.BuildPath}/[BuildTarget]");
            EnsureVar(p, s.activeProfileId, "LocalLoadPath", "{UnityEngine.AddressableAssets.Addressables.RuntimePath}/[BuildTarget]");
        }

        static void EnsureVar(AddressableAssetProfileSettings p, string profileId, string name, string value)
        {
            var existing = p.GetValueByName(profileId, name);
            if (existing == null) p.CreateValue(name, value);
            else if (string.IsNullOrEmpty(existing)) p.SetValue(profileId, name, value);
        }

        static void EnsureCoreGroups(AddressableAssetSettings s, IEnumerable<string> names)
        {
            foreach (var n in names) EnsureGroup(s, n);
        }

        static AddressableAssetGroup EnsureGroup(AddressableAssetSettings s, string name)
        {
            var g = s.FindGroup(name);
            if (g == null)
                g = s.CreateGroup(name, false, false, false, null,
                    typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));

            var schema = g.GetSchema<BundledAssetGroupSchema>() ?? g.AddSchema<BundledAssetGroupSchema>();
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            schema.BuildPath.SetVariableByName(s, "LocalBuildPath");
            schema.LoadPath.SetVariableByName(s, "LocalLoadPath");
            return g;
        }

        static void ForceLocalPathsAllGroups(AddressableAssetSettings s)
        {
            foreach (var g in s.groups.Where(g => g != null))
            {
                var schema = g.GetSchema<BundledAssetGroupSchema>();
                if (schema == null) continue;
                schema.BuildPath.SetVariableByName(s, "LocalBuildPath");
                schema.LoadPath.SetVariableByName(s, "LocalLoadPath");
                EditorUtility.SetDirty(schema);
            }
        }
    }
}
#endif
