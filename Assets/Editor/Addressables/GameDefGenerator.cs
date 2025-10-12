#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using SpaceTrader.Game.Data;

namespace SpaceTrader.Editor.Addressables
{
    public static class GameDefGenerator
    {
        [MenuItem("Tools/GameDefs/Generate For Prefabs Under Assets/Prefabs")]
        public static void Generate()
        {
            var outDir = "Assets/Data/AutoDefs";
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder(outDir))
                AssetDatabase.CreateFolder("Assets/Data", "AutoDefs");

            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" }).Distinct();
            int created = 0;

            foreach (var guid in guids)
            {
                var prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                var id = Path.GetFileNameWithoutExtension(prefabPath);

                // Varsa mevcut def’leri atla (id eşleşmesine göre)
                var existing = AssetDatabase
                    .FindAssets($"t:GameDef {id}")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(AssetDatabase.LoadAssetAtPath<GameDef>)
                    .FirstOrDefault(d => d && d.id == id);
                if (existing)
                    continue;

                var def = ScriptableObject.CreateInstance<GameDef>();
                def.id = id;
                def.category = GuessCategory(prefabPath); // basit kestirim

                // Prefab referansı
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                def.prefab = new UnityEngine.AddressableAssets.AssetReferenceGameObject(guid);

                // Icon: Art/**/icons/<id> aramaya çalış
                var iconGuid = AssetDatabase
                    .FindAssets($"{id} t:Sprite", new[] { "Assets/Art" })
                    .Select(g => new { g, p = AssetDatabase.GUIDToAssetPath(g) })
                    .FirstOrDefault(x => x.p.ToLower().Contains("/icons/"))
                    ?.g;
                if (!string.IsNullOrEmpty(iconGuid))
                    def.icon = new UnityEngine.AddressableAssets.AssetReferenceSprite(iconGuid);

                var assetPath = $"{outDir}/{id}.asset";
                AssetDatabase.CreateAsset(def, assetPath);
                created++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log(
                $"[GameDefGenerator] Created {created} defs under {outDir}. Add them to a GameCatalog."
            );
        }

        static string GuessCategory(string path)
        {
            var p = path.ToLower();
            if (p.Contains("/ships"))
                return "ship";
            if (p.Contains("/enemies"))
                return "enemy";
            if (p.Contains("/weapons"))
                return "weapon";
            if (p.Contains("/fx"))
                return "fx";
            if (p.Contains("/props"))
                return "prop";
            return "misc";
        }
    }
}
#endif
