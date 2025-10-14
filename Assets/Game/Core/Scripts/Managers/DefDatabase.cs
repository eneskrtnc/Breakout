using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using SpaceTrader.Core.Data;

namespace SpaceTrader.Core
{
    public class DefDatabase : MonoBehaviour
    {
        public static DefDatabase Instance { get; private set; }

        public List<CatalogBase> catalogs = new();

        // Tip -> (id -> BaseDef)
        private readonly Dictionary<System.Type, Dictionary<string, BaseDef>> _byType =
            new Dictionary<System.Type, Dictionary<string, BaseDef>>();

        private bool _isIndexed;

        private void Awake()
        {
            if (Instance && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // ÖNEMLİ: Indeksi Start yerine Awake'te kur.
            BuildIndex();
        }

        private void Start()
        {
            // İsteğe bağlı: Start'ta da güvence olsun
            if (!_isIndexed) BuildIndex();
        }

        public void BuildIndex()
        {
            _byType.Clear();

            foreach (var cat in catalogs.Where(c => c != null))
            {
                var t = cat.DefType;
                if (!_byType.TryGetValue(t, out var map))
                {
                    map = new Dictionary<string, BaseDef>();
                    _byType[t] = map;
                }

                foreach (var def in cat.Enumerate())
                {
                    if (def == null || string.IsNullOrEmpty(def.Id)) continue;
                    if (map.ContainsKey(def.Id))
                    {
                        Debug.LogError($"[DefDatabase] Duplicate id '{def.Id}' in {t.Name}");
                        continue;
                    }
                    map[def.Id] = def;
                }
            }

            _isIndexed = true;
            Debug.Log($"[DefDatabase] Indexed types: {string.Join(", ", _byType.Keys.Select(x => x.Name))}");
        }

        public bool IsReady => _isIndexed;

        public TDef Get<TDef>(string id) where TDef : BaseDef
        {
            if (string.IsNullOrEmpty(id)) return null;
            var t = typeof(TDef);
            if (_byType.TryGetValue(t, out var map) && map.TryGetValue(id, out var def))
                return def as TDef;
            return null;
        }

        public BaseDef GetByKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            var dot = key.IndexOf('.');
            if (dot <= 0 || dot >= key.Length - 1) return null;

            var prefix = key.Substring(0, dot);
            var id = key.Substring(dot + 1);

            System.Type t = prefix switch
            {
                "ship" => typeof(ShipDef),
                // "enemy" => typeof(EnemyDef),
                _ => null
            };

            if (t == null) return null;

            if (_byType.TryGetValue(t, out var map) && map.TryGetValue(id, out var def))
                return def;
            return null;
        }
    }
}
