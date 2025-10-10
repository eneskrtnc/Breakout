// Assets/Scripts/Data/DefSet.cs
using System;
using System.Collections.Generic;
using System.Linq;   // <-- Bunu ekle

namespace SpaceTrader.Data
{
    public sealed class DefSet<T> where T : BaseDef
    {
        public T[] All { get; private set; } = Array.Empty<T>();
        private Dictionary<string, T> _byId = new(StringComparer.Ordinal);

        public void Replace(IEnumerable<T> defs)
        {
            if (defs == null)
            {
                All = Array.Empty<T>();
                _byId = new Dictionary<string, T>(StringComparer.Ordinal);
                return;
            }

            // Dizi ise doğrudan kullan, değilse ToArray
            if (defs is T[] arr) All = arr;
            else All = defs.ToArray();

            var map = new Dictionary<string, T>(StringComparer.Ordinal);
            foreach (var d in All)
            {
                if (d == null || string.IsNullOrWhiteSpace(d.id)) continue;
                if (!map.ContainsKey(d.id)) map.Add(d.id, d); // çakışmada ilk kazanır
            }
            _byId = map;
        }

        public T Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return _byId.TryGetValue(id, out var v) ? v : null;
        }
    }
}
