// Assets/Scripts/Editor/Addressables/AddressablesSticky.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;

namespace SpaceTrader.Editor.Addressables
{
    /// <summary>
    /// Editor açıldığında Addressables settings asset'ini otomatik pinler.
    /// Menüden elle de tetikleyebilirsin: Tools → Addressables → Pin Default Settings
    /// </summary>
    [InitializeOnLoad]
    public static class AddressablesSticky
    {
        static AddressablesSticky()
        {
            // Editor tamamen hazır olduğunda pinle (erken null riskini önler)
            EditorApplication.delayCall += PinDefaultSettings;
        }

        [MenuItem("Tools/Addressables/Pin Default Settings")]
        public static void PinDefaultSettings()
        {
            var settings = FindOrCreateSettingsAsset();
            if (!settings)
            {
                Debug.LogError("[Addressables] Settings asset bulunamadı/oluşturulamadı.");
                return;
            }

            AddressableAssetSettingsDefaultObject.Settings = settings;
            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();

            var path = AssetDatabase.GetAssetPath(settings);
            Debug.Log("[Addressables] Pinned default settings → " + path);
        }

        private static AddressableAssetSettings FindOrCreateSettingsAsset()
        {
            const string defaultPath =
                "Assets/AddressableAssetsData/AddressableAssetSettings.asset";

            // 1) Varsayılan yerde var mı?
            var settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(defaultPath);
            if (settings)
                return settings;

            // 2) Projede herhangi bir yerde var mı?
            var guids = AssetDatabase.FindAssets("t:AddressableAssetSettings");
            if (guids.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(path);
                if (settings)
                    return settings;
            }

            // 3) Yoksa oluştur
            if (!AssetDatabase.IsValidFolder("Assets/AddressableAssetsData"))
                AssetDatabase.CreateFolder("Assets", "AddressableAssetsData");

            var dir = "Assets/AddressableAssetsData";
            settings = AddressableAssetSettings.Create(dir, "AddressableAssetSettings", true, true);
            AssetDatabase.SaveAssets();
            return settings;
        }
    }
}
#endif
