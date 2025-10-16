using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceTrader.Game.UI
{
    /// <summary>
    /// Dinamik % bölüntü çizgileri. Çocuk çizgileri havuzlanır (destroy edilmez),
    /// yeniden çizimde sadece aktif/pasif yapılır. Editör ve Play'de güvenli çalışır.
    /// </summary>
    [ExecuteAlways]
    public class UiBarTicks : MonoBehaviour
    {
        [Header("Targets")]
        public RectTransform targetRect;      // genişlik/height referansı (Frame veya Mask)
        public RectTransform ticksContainer;  // çizgilerin parent'ı
        public Image tickTemplate;            // 1x1 beyaz Image (GameObject disabled)

        [Header("Layout")]
        [Range(2, 20)] public int divisions = 10;   // 10 => %10
        public float leftPadding = 0f;              // px
        public float rightPadding = 0f;             // px
        public float verticalPadding = 0f;          // üst/alt boşluk

        [Header("Style")]
        [Range(0f, 1f)] public float alpha = 0.45f;
        public Color color = new Color32(33, 41, 54, 255); // #212936
        public bool drawEnds = false; // %0 ve %100'de de çizgi çizilsin mi?

        Canvas _canvas;
        bool _dirty;

        // Havuz (spawnlanan çizgiler)
        readonly List<Image> _pool = new List<Image>();

        void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
        }

        void OnEnable()
        {
            MarkDirty();
        }

        void OnDisable()
        {
            // Play'de sahneden ayrılırken çizgileri gizle (destroy yok)
            for (int i = 0; i < _pool.Count; i++)
                if (_pool[i]) _pool[i].gameObject.SetActive(false);
        }

        void OnRectTransformDimensionsChange()
        {
            if (!isActiveAndEnabled) return;
            MarkDirty();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            divisions = Mathf.Clamp(divisions, 2, 20);
            alpha = Mathf.Clamp01(alpha);
            // Editörde anında UI mutasyonu yasak olabildiği için sadece işaretle
            MarkDirty();
        }
#endif

        void Update()
        {
            // Hem Editör, hem Play'de güvenli zamanda uygula
            if (_dirty) RedrawNow();
        }

        void MarkDirty() => _dirty = true;

        void RedrawNow()
        {
            _dirty = false;
            if (!targetRect || !ticksContainer || !tickTemplate || !_canvas) return;

            float w = targetRect.rect.width - leftPadding - rightPadding;
            float h = targetRect.rect.height - (verticalPadding * 2f);
            if (w <= 0f || h <= 0f) { DisableAll(); return; }

            // 1 ekran pikseli kalınlık
            float px = 1f / Mathf.Max(1f, _canvas.scaleFactor);

            int start = drawEnds ? 0 : 1;
            int end = drawEnds ? divisions : divisions - 1;
            int needed = Mathf.Max(0, end - start + 1);

            EnsurePool(needed);

            // Aktifleştir & konumlandır
            for (int i = 0; i < needed; i++)
            {
                var img = _pool[i];
                if (!img) continue;

                img.gameObject.SetActive(true);
                img.color = new Color(color.r, color.g, color.b, alpha);

                var rt = img.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);

                // 1px genişlik, bar iç yüksekliği kadar
                rt.sizeDelta = new Vector2(px, Mathf.Max(px, h));

                // % konum → piksel hizasına yuvarla
                float x = Mathf.Round(leftPadding + (w * (start + i) / divisions));
                rt.anchoredPosition = new Vector2(x, 0f);
            }

            // Fazla olanları gizle
            for (int i = needed; i < _pool.Count; i++)
                if (_pool[i]) _pool[i].gameObject.SetActive(false);
        }

        void EnsurePool(int count)
        {
            // Mevcut çocuklardan (template haricinde) havuzu oluştur
            // (Bir kere oluşturmak yeterli; sonra sadece eksikleri instantiateleriz)
            if (_pool.Count == 0)
            {
                for (int i = 0; i < ticksContainer.childCount; i++)
                {
                    var t = ticksContainer.GetChild(i);
                    if (t == tickTemplate.transform) continue;
                    var img = t.GetComponent<Image>();
                    if (img) _pool.Add(img);
                }
            }

            // Eksik varsa template'ten üret
            while (_pool.Count < count)
            {
                var img = Instantiate(tickTemplate, ticksContainer);
                img.gameObject.SetActive(false);
                _pool.Add(img);
            }
        }

        void DisableAll()
        {
            for (int i = 0; i < _pool.Count; i++)
                if (_pool[i]) _pool[i].gameObject.SetActive(false);
        }
    }
}
