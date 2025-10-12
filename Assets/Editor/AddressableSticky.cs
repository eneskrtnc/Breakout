// Assets/Editor/AddressablesSticky.cs
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

[InitializeOnLoad]
public static class AddressablesSticky
{
    static AddressablesSticky()
    {
        PinDefaultSettings();
        // Editör açılır açılmaz Ships varlıklarını grup içinde tuttuğumuzdan emin ol
        EnsureShipsGroup(silent: true);
    }

    [MenuItem("Tools/Addressables/Pin Default Settings")]
    public static void PinDefaultSettings()
    {
        var path = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";
        var settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(path);
        if (settings != null)
        {
            AddressableAssetSettingsDefaultObject.Settings = settings;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log("[Addressables] Pinned default settings → " + path);
        }
        else Debug.LogError("[Addressables] Settings asset not found at: " + path);
    }

    [MenuItem("Tools/Addressables/Ensure Ships Group")]
    public static void EnsureShipsGroupMenu() => EnsureShipsGroup();

    public static void EnsureShipsGroup(bool silent = false)
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) { if (!silent) Debug.LogError("No Addressables settings."); return; }

        var group = settings.FindGroup("Ships") ??
                    settings.CreateGroup("Ships", false, false, false, null,
                        typeof(BundledAssetGroupSchema), typeof(ContentUpdateGroupSchema));

        var roots = new[] { "Assets/Art/ships", "Assets/Art/Ships", "Assets/Ships" }
            .Where(AssetDatabase.IsValidFolder).ToArray();

        var guids = roots.SelectMany(r => AssetDatabase.FindAssets("t:Object", new[] { r }))
                         .Distinct();

        int added = 0, moved = 0;
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.IsValidFolder(path)) continue;

            var entry = settings.FindAssetEntry(guid);
            if (entry == null)
            {
                entry = settings.CreateOrMoveEntry(guid, group, false, false);
                entry.address = Path.GetFileNameWithoutExtension(path);
                entry.SetLabel("ships", true, true);
                added++;
            }
            else if (entry.parentGroup != group)
            {
                settings.CreateOrMoveEntry(guid, group, false, false);
                moved++;
            }
        }

        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, null, true);
        AssetDatabase.SaveAssets();
        if (!silent) Debug.Log($"[Addressables] Group '{group.Name}' → added: {added}, moved: {moved}");
    }

    [MenuItem("Tools/Addressables/Purge Missing Entries")]
    public static void PurgeMissingEntries()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        if (settings == null) return;
        int removed = 0;

        foreach (var g in settings.groups.Where(g => g != null))
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
        Debug.Log($"[Addressables] Removed {removed} missing entries.");
    }
}
