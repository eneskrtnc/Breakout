using System.Collections.Generic;
using UnityEngine;

namespace SpaceTrader.Game.Data
{
    [CreateAssetMenu(menuName = "Game/Core/GameCatalog", fileName = "GameCatalog")]
    public class GameCatalog : ScriptableObject
    {
        public List<GameDef> defs = new();
        Dictionary<string, GameDef> _map;

        void OnEnable()
        {
            _map = new Dictionary<string, GameDef>(defs.Count);
            foreach (var d in defs)
            {
                if (!d || string.IsNullOrWhiteSpace(d.id))
                    continue;
                _map[d.id] = d;
            }
        }

        public bool TryGet(string id, out GameDef def)
        {
            if (_map == null)
            {
                def = null;
                return false;
            }
            return _map.TryGetValue(id, out def);
        }

        public IEnumerable<GameDef> All => defs; // <— property
    }
}
