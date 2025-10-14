using UnityEngine;

namespace SpaceTrader.Core
{
    public class PrefabFactory : MonoBehaviour
    {
        public PrefabLibrary library;

        public GameObject Spawn(string key, Vector3 pos, Quaternion rot)
        {
            if (!library || !library.TryGet(key, out var prefab))
            {
                Debug.LogError($"[PrefabFactory] Key bulunamadı: {key}");
                return null;
            }
            return Instantiate(prefab, pos, rot);
        }
    }
}
