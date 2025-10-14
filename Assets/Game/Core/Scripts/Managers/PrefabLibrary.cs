using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceTrader.Core
{
    [CreateAssetMenu(menuName = "Game/Prefab Library")]
    public class PrefabLibrary : ScriptableObject
    {
        [System.Serializable]
        public struct Entry
        {
            public string key;        // örn: "debug.cube" veya "enemy.slime.basic"
            public GameObject prefab; // hedef prefab
        }

        public List<Entry> entries = new();

        Dictionary<string, GameObject> _map;

        private void OnEnable() => _map = entries.ToDictionary(e => e.key, e => e.prefab);

        public bool TryGet(string key, out GameObject prefab) => _map.TryGetValue(key, out prefab);
        public GameObject Get(string key) => _map[key];
    }
}
