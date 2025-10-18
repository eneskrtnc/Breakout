using SpaceTrader.Core.Data;
using SpaceTrader.Game.UI;
using UnityEngine;

namespace SpaceTrader.Core
{
    public class SpawnService : MonoBehaviour
    {
        public PrefabFactory factory;
        public DefDatabase defs;

        private void Awake()
        {
            if (!factory)
                factory = FindFirstObjectByType<PrefabFactory>();
            if (!defs)
                defs = FindFirstObjectByType<DefDatabase>();
        }

        public GameObject Spawn<TDef>(string id, Vector3 pos, Quaternion rot)
            where TDef : BaseDef
        {
            // Güvence: indeks hazır değilse hazırla
            if (defs && !defs.IsReady)
                defs.BuildIndex();

            var def = defs ? defs.Get<TDef>(id) : null;

            // Bir kere daha şans ver (özellikle sahne/asset kablolaması yeni yapıldıysa)
            if (def == null && defs)
            {
                defs.BuildIndex();
                def = defs.Get<TDef>(id);
            }

            if (def == null)
            {
                Debug.LogError($"[SpawnService] Def yok: type={typeof(TDef).Name}, id={id}");
                return null;
            }

            if (factory)
                return factory.Spawn(def.Key, pos, rot);

            if (!def.Prefab)
            {
                Debug.LogError($"[SpawnService] Prefab yok: {def.name}");
                return null;
            }
            return Instantiate(def.Prefab, pos, rot);
        }

        public GameObject SpawnByKey(string key, Vector3 pos, Quaternion rot)
        {
            if (defs && !defs.IsReady)
                defs.BuildIndex();

            if (factory)
                return factory.Spawn(key, pos, rot);

            var def = defs ? defs.GetByKey(key) : null;
            if (def?.Prefab == null)
            {
                Debug.LogError($"[SpawnService] Key çözülemedi veya prefab yok: {key}");
                return null;
            }
            return Instantiate(def.Prefab, pos, rot);
        }
    }
}
