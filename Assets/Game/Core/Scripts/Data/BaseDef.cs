using UnityEngine;

namespace SpaceTrader.Core.Data
{
    /// Ortak alanları tek yerde toplayan ScriptableObject tabanı.
    public abstract class BaseDef : ScriptableObject, IKeyedDef
    {
        [Header("Identity")]
        [SerializeField]
        private readonly string id; // "starter_ship"

        [SerializeField]
        private readonly string displayName = ""; // Oyuncuya görünen ad

        [TextArea]
        [SerializeField]
        private readonly string desc = "";

        [SerializeField]
        private readonly string[] tags = System.Array.Empty<string>();

        [Header("Presentation (2D)")]
        [SerializeField]
        private readonly Sprite icon; // UI ikon

        [SerializeField]
        private readonly GameObject prefab; // SpriteRenderer içeren prefab (2D)

        // ---- IKeyedDef sözleşmesi ----
        public string Id => id;
        public string DisplayName => displayName;
        public string Description => desc;
        public string[] Tags => tags;
        public Sprite Icon => icon;
        public GameObject Prefab => prefab;

        // Her alt sınıf kendi prefix'ini belirtir (örn: "ship", "enemy").
        public abstract string KeyPrefix { get; }

        // PrefabLibrary ile birebir aynı olacak anahtar üretimi.
        public string Key => string.IsNullOrEmpty(Id) ? "" : $"{KeyPrefix}.{Id}";

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            // id doğrulaması
            if (!string.IsNullOrEmpty(id))
            {
                var ok = System.Text.RegularExpressions.Regex.IsMatch(id, "^[a-z0-9_]+$");
                if (!ok)
                    Debug.LogWarning(
                        $"[BaseDef] id '{id}' regex ile uyumlu değil (^[a-z0-9_]+$): {name}",
                        this
                    );
            }
            else
            {
                Debug.LogWarning($"[BaseDef] id boş: {name}", this);
            }

            if (!prefab)
                Debug.LogWarning($"[BaseDef] prefab atanmadı: {name}", this);
            if (!icon)
                Debug.LogWarning($"[BaseDef] icon atanmadı: {name}", this);
        }
#endif
    }
}
