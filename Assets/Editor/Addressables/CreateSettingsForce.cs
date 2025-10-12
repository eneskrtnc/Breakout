// Assets/Scripts/Editor/Addressables/CreateSettingsForce.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace SpaceTrader.Editor.Addressables
{
    public static class CreateSettingsForce
    {
        const string SettingsPath = "Assets/AddressableAssetsData/AddressableAssetSettings.asset";

        [MenuItem("Tools/Addressables/Create Settings (Force)")]
        public static void CreateOrRepair()
        {
            // 1) Settings asset'i oluştur / yükle
            var settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(SettingsPath);
            if (!settings)
            {
                if (!AssetDatabase.IsValidFolder("Assets/AddressableAssetsData"))
                    AssetDatabase.CreateFolder("Assets", "AddressableAssetsData");

                settings = AddressableAssetSettings.Create(
                    "Assets/AddressableAssetsData",
                    "AddressableAssetSettings",
                    true,
                    true
                );
                Debug.Log("[Addr] Created AddressableAssetSettings at " + SettingsPath);
            }

            // 2) Aktif settings olarak pinle
            AddressableAssetSettingsDefaultObject.Settings = settings;

            // 3) Profil değişkenleri
            var ps = settings.profileSettings;
            EnsureVar(
                ps,
                settings.activeProfileId,
                "LocalBuildPath",
                "{UnityEngine.AddressableAssets.Addressables.BuildPath}/[BuildTarget]"
            );
            EnsureVar(
                ps,
                settings.activeProfileId,
                "LocalLoadPath",
                "{UnityEngine.AddressableAssets.Addressables.RuntimePath}/[BuildTarget]"
            );

            // 4) Default Local Group ve schema
            var group = settings.DefaultGroup ?? settings.FindGroup("Default Local Group");
            if (group == null)
                group = settings.CreateGroup(
                    "Default Local Group",
                    false,
                    false,
                    false,
                    null,
                    typeof(BundledAssetGroupSchema),
                    typeof(ContentUpdateGroupSchema)
                );
            settings.DefaultGroup = group;

            var schema =
                group.GetSchema<BundledAssetGroupSchema>()
                ?? group.AddSchema<BundledAssetGroupSchema>();
            schema.BundleMode = BundledAssetGroupSchema.BundlePackingMode.PackTogether;
            schema.BuildPath.SetVariableByName(settings, "LocalBuildPath");
            schema.LoadPath.SetVariableByName(settings, "LocalLoadPath");

            EditorUtility.SetDirty(schema);
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();

            Debug.Log("[Addr] Settings pinned & default group ensured.");
        }

        static void EnsureVar(
            AddressableAssetProfileSettings ps,
            string pid,
            string name,
            string value
        )
        {
            var cur = ps.GetValueByName(pid, name);
            if (cur == null)
                ps.CreateValue(name, value);
            else if (string.IsNullOrEmpty(cur))
                ps.SetValue(pid, name, value);
        }
    }
}
#endif
