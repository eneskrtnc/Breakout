using System.Collections.Generic;
using UnityEngine;

namespace SpaceTrader.Core.Data
{
    /// Tüm kataloglar için soyut taban (type-erased görünüm).
    public abstract class CatalogBase : ScriptableObject
    {
        // Bu katalog hangi def tipini tutuyor? (örn: typeof(ShipDef))
        public abstract System.Type DefType { get; }

        // İçerdiği BaseDef'ler (box'lanmış). DefDatabase bunları okuyacak.
        public abstract IEnumerable<BaseDef> Enumerate();
    }
}
