using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceTrader.Core.Data
{
    [CreateAssetMenu(menuName = "Game/Catalogs/Generic Catalog", fileName = "Catalog")]
    public class Catalog<TDef> : CatalogBase
        where TDef : BaseDef
    {
        public List<TDef> items = new();

        public override System.Type DefType => typeof(TDef);

        public override IEnumerable<BaseDef> Enumerate() => items.Where(i => i != null);
    }
}
