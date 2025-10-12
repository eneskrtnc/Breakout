// Assets/Scripts/Editor/Addressables/AddressablesDeepClean.cs
#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace SpaceTrader.Editor.Addressables
{
    public static class AddressablesDeepClean
    {
        [MenuItem("Tools/Addressables/Deep Clean → Purge Broken Entries (RAW)")]
        public static void DeepCleanBrokenEntries()
        {
            var s = AddressableAssetSettingsDefaultObject.Settings;
            if (!s)
            {
                Debug.LogError("[AddrClean] No Addressables settings.");
                return;
            }

            int removedNull = 0,
                fixedAddr = 0,
                removedMissing = 0;

            foreach (var g in s.groups.Where(g => g != null))
            {
                var so = new SerializedObject(g);
                var entries = so.FindProperty("m_Entries");
                if (entries == null)
                    continue;

                // Tersten sil (index kaymasın)
                for (int i = entries.arraySize - 1; i >= 0; i--)
                {
                    var e = entries.GetArrayElementAtIndex(i);
                    if (e == null)
                    {
                        entries.DeleteArrayElementAtIndex(i);
                        removedNull++;
                        continue;
                    }

                    var guidProp = e.FindPropertyRelative("m_GUID");
                    var addressProp = e.FindPropertyRelative("m_Address");

                    var guid = guidProp?.stringValue;
                    var addr = addressProp?.stringValue;

                    // 1) Null/empty guid → kesin sil
                    if (string.IsNullOrEmpty(guid))
                    {
                        entries.DeleteArrayElementAtIndex(i);
                        removedNull++;
                        continue;
                    }

                    // 2) GUID path'i yoksa (asset silinmiş) → sil
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    if (string.IsNullOrEmpty(path))
                    {
                        entries.DeleteArrayElementAtIndex(i);
                        removedMissing++;
                        continue;
                    }

                    // 3) Adres boşsa dosya adından doldur
                    if (string.IsNullOrEmpty(addr))
                    {
                        var newAddr = Path.GetFileNameWithoutExtension(path);
                        addressProp.stringValue = newAddr;
                        fixedAddr++;
                    }
                }
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(g);
            }

            s.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, null, true);
            AssetDatabase.SaveAssets();

            Debug.Log(
                $"[AddrClean] Removed null: {removedNull}, removed missing: {removedMissing}, fixed addresses: {fixedAddr}"
            );
        }

        [MenuItem("Tools/Addressables/Deep Clean → Reset All Groups (KEEP Settings)")]
        public static void ResetAllGroupsKeepSettings()
        {
            var s = AddressableAssetSettingsDefaultObject.Settings;
            if (!s)
            {
                Debug.LogError("[AddrClean] No Addressables settings.");
                return;
            }

            // Tüm grupları kaldır
            foreach (var g in s.groups.ToList())
            {
                if (g != null)
                    s.RemoveGroup(g);
            }

            // Varsayılan grup ve şemaları geri kur
            var def = s.CreateGroup(
                "Default Local Group",
                false,
                false,
                false,
                null,
                typeof(BundledAssetGroupSchema),
                typeof(ContentUpdateGroupSchema)
            );
            s.DefaultGroup = def;

            var schema =
                def.GetSchema<BundledAssetGroupSchema>()
                ?? def.AddSchema<BundledAssetGroupSchema>();
            EnsureProfileVar(
                s,
                "LocalBuildPath",
                "{UnityEngine.AddressableAssets.Addressables.BuildPath}/[BuildTarget]"
            );
            EnsureProfileVar(
                s,
                "LocalLoadPath",
                "{UnityEngine.AddressableAssets.Addressables.RuntimePath}/[BuildTarget]"
            );
            schema.BuildPath.SetVariableByName(s, "LocalBuildPath");
            schema.LoadPath.SetVariableByName(s, "LocalLoadPath");

            EditorUtility.SetDirty(s);
            AssetDatabase.SaveAssets();
            Debug.Log("[AddrClean] All groups reset. Now run Rebuild From Conventions.");
        }

        [MenuItem("Tools/Addressables/Deep Clean → Print Active Settings Info")]
        public static void PrintInfo()
        {
            var s = AddressableAssetSettingsDefaultObject.Settings;
            var path = s ? AssetDatabase.GetAssetPath(s) : "(null)";
            var groupCount = s ? s.groups?.Count ?? 0 : 0;
            Debug.Log($"[AddrClean] Settings: {path}  Groups: {groupCount}");
        }

        static void EnsureProfileVar(AddressableAssetSettings s, string name, string value)
        {
            var ps = s.profileSettings;
            var cur = ps.GetValueByName(s.activeProfileId, name);
            if (cur == null)
                ps.CreateValue(name, value);
            else if (string.IsNullOrEmpty(cur))
                ps.SetValue(s.activeProfileId, name, value);
        }
    }
}
#endif
