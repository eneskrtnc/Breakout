using UnityEngine;

namespace SpaceTrader.Core.Data
{
    /// Tüm tanım (def) tipleri için ortak sözleşme.
    public interface IKeyedDef
    {
        // Benzersiz kimlik (örn: "starter_ship")
        string Id { get; }

        // PrefabLibrary key ön eki (örn: "ship", "enemy", "proj", "item", "vfx")
        string KeyPrefix { get; }

        // PrefabLibrary'de kullanılacak nihai anahtar: $"{KeyPrefix}.{Id}"
        string Key { get; }

        // Sunum alanları
        string DisplayName { get; }
        string Description { get; }
        string[] Tags { get; }

        // 2D oyun için önemli: UI ikon ve sahnede kullanılacak prefab
        Sprite Icon { get; }
        GameObject Prefab { get; }
    }
}
