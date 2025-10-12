using System.Collections.Generic;
using System.Threading.Tasks;
using SpaceTrader.Game.Data;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace SpaceTrader.Game.Runtime.Services
{
    public class AssetService : MonoBehaviour
    {
        public static AssetService Instance { get; private set; }

        [SerializeField]
        readonly GameDatabase database; // Inspector'dan ata (Project asseti)

        readonly Dictionary<GameObject, AsyncOperationHandle<GameObject>> _instances = new();
        readonly Dictionary<string, AsyncOperationHandle<Sprite>> _iconCache = new();

        void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task PreloadAsync(string id, bool iconToo = false)
        {
            if (!database || !database.TryGet(id, out var def))
            {
                Debug.LogWarning($"[AssetService] def not found: {id}");
                return;
            }
            // Prefab'ı ön yükle (InstantiateAsync yerine LoadAsset)
            await def.prefab.LoadAssetAsync<GameObject>().Task;
            if (iconToo && def.icon.RuntimeKeyIsValid())
            {
                var h = def.icon.LoadAssetAsync();
                _iconCache[id] = h;
                await h.Task;
            }
        }

        public async Task<GameObject> SpawnAsync(
            string id,
            Vector3 pos,
            Quaternion rot,
            Transform parent = null
        )
        {
            if (!database || !database.TryGet(id, out var def))
            {
                Debug.LogError($"[AssetService] Unknown id: {id}");
                return null;
            }
            var h = Addressables.InstantiateAsync(def.prefab, pos, rot, parent);
            var go = await h.Task;
            if (!go)
                return null;
            _instances[go] = h;
            return go;
        }

        public void Despawn(GameObject go)
        {
            if (!go)
                return;
            if (_instances.TryGetValue(go, out var h))
            {
                Addressables.ReleaseInstance(go);
                _instances.Remove(go);
            }
            else
            {
                // Addressables dışı ise:
                Destroy(go);
            }
        }

        public async Task<Sprite> LoadIconAsync(string id)
        {
            if (!database || !database.TryGet(id, out var def))
                return null;
            if (!def.icon.RuntimeKeyIsValid())
                return null;

            if (_iconCache.TryGetValue(id, out var h) && h.IsValid())
                return await h.Task;

            var nh = def.icon.LoadAssetAsync();
            _iconCache[id] = nh;
            return await nh.Task;
        }

        public void ReleaseIcon(string id)
        {
            if (_iconCache.TryGetValue(id, out var h) && h.IsValid())
            {
                Addressables.Release(h);
                _iconCache.Remove(id);
            }
        }
    }
}
