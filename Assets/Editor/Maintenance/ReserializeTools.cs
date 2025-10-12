// Assets/Editor/ReserializeTools.cs
using UnityEditor;
using System.Linq;
namespace SpaceTrader.Editor.Maintenance
{
    public static class ReserializeTools
    {
        [MenuItem("Tools/Serialization/Force Reserialize Selected %#r")]
        public static void ForceReserializeSelected()
        {
            var paths = Selection.assetGUIDs.Length > 0
                ? Selection.assetGUIDs.Select(AssetDatabase.GUIDToAssetPath)
                : AssetDatabase.FindAssets("t:Object").Select(AssetDatabase.GUIDToAssetPath).Distinct();

            AssetDatabase.ForceReserializeAssets(paths);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log("Reserialized:\n" + string.Join("\n", paths));
        }
    }
}
