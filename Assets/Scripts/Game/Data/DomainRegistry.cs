using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceTrader.Game.Data
{
    /// <summary>
    /// Bir veri domain'ini (ships, weapons, stations ...) tanımlar.
    /// </summary>
    public class DomainConfig
    {
        public string DomainName; // Örn: "Ships"
        public string Label; // Addressables label: "ships"
        public Type DefType; // Örn: typeof(ShipDef)
        public string IdFieldName = "id"; // def.id alan adı (varsayılan "id")

        // Opsiyonel alan adları (icon/prefab alanları yoksa boş bırak)
        public string IconFieldName = "iconRef"; // AssetReference (Sprite)
        public string PrefabFieldName = "prefabRef"; // AssetReference (GameObject)

        // Opsiyonel fallback Addressables adresleri (yoksa boş bırak)
        public string FallbackIconAddress; // Örn: "ships/icons/_fallback"
        public string FallbackPrefabAddress; // Örn: "ships/prefabs/_fallback"
    }

    /// <summary>
    /// Çalışma zamanı domain durumu: yüklenen kayıtlar ve fallback cache'leri.
    /// </summary>
    public class DomainRuntime
    {
        public DomainConfig Config;
        public readonly Dictionary<string, ScriptableObject> ById = new Dictionary<
            string,
            ScriptableObject
        >(StringComparer.Ordinal);
        public Sprite FallbackIcon;
        public GameObject FallbackPrefab;
    }
}
