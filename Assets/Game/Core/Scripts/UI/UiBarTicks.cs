using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceTrader.Game.UI
{
    /// <summary>
    /// Dinamik % bölüntü çizgileri. Havuzlanır, destroy yok. Editör & Play güvenli.
    /// Horizontal: dikey çizgiler; Vertical: yatay çizgiler çizer.
    /// </summary>
    [ExecuteAlways]
    public class UiBarTicks : MonoBehaviour
    {
        public enum Orientation { Horizontal, Vertical }

        [Header("Orientation")]
        public Orientation orientation = Orientation.Horizontal; // Energy dikey için Vertical

        [Header("Targets")]
        public RectTransform targetRect;      // ölçü referansı (Frame veya Mask)
        public RectTransform ticksContainer;  // çizgilerin parent'ı
        public Image tickTemplate;            // 1x1 beyaz Image (GameObject disabled)

        [Header("Layout")]
        [Range(2, 20)] public int divisions = 10;   // 10 => %10
        // Yatay için sol/sağ; dikey için üst/alt padding kullanılır:
        public float leftPadding = 0f;
        public float rightPadding = 0f;
        public float topPadding = 0f;
        public float bottomPadding = 0f;

        [Header("Style")]
        [Range(0f, 1f)] public float alpha = 0.45f;
        public Color color = new Color32(33, 41, 54, 255); // #212936
        public bool drawEnds = false; // %0 ve %100 dahil mi?

        Canvas _canvas;
        bool _dirty;
        readonly List<Image> _pool = new List<Image>();

        void Awake() { _canvas = GetComponentInParent<Canvas>(); }
        void OnEnable() { MarkDirty(); }
        void OnDisable() { for (int i = 0; i < _pool.Count; i++) if (_pool[i]) _pool[i].gameObject.SetActive(false); }
        void OnRectTransformDimensionsChange() { if (isActiveAndEnabled) MarkDirty(); }

#if UNITY_EDITOR
        void OnValidate()
        {
            divisions = Mathf.Clamp(divisions, 2, 20);
            alpha = Mathf.Clamp01(alpha);
            MarkDirty();
        }
#endif

        void Update() { if (_dirty) RedrawNow(); }
        void MarkDirty() => _dirty = true;

        void RedrawNow()
        {
            _dirty = false;
            if (!_canvas || !targetRect || !ticksContainer || !tickTemplate) return;

            float px = 1f / Mathf.Max(1f, _canvas.scaleFactor);

            if (orientation == Orientation.Horizontal)
            {
                float w = targetRect.rect.width - (leftPadding + rightPadding);
                float h = targetRect.rect.height - (topPadding + bottomPadding);
                if (w <= 0f || h <= 0f) { DisableAll(); return; }

                int start = drawEnds ? 0 : 1;
                int end = drawEnds ? divisions : divisions - 1;
                int needed = Mathf.Max(0, end - start + 1);
                EnsurePool(needed);

                for (int i = 0; i < needed; i++)
                {
                    var img = _pool[i];
                    img.gameObject.SetActive(true);
                    img.color = new Color(color.r, color.g, color.b, alpha);

                    var rt = img.rectTransform;
                    rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(px, Mathf.Max(px, h)); // dikey çizgi

                    float x = Mathf.Round(leftPadding + (w * (start + i) / divisions));
                    rt.anchoredPosition = new Vector2(x, 0f);
                }

                for (int i = needed; i < _pool.Count; i++) _pool[i].gameObject.SetActive(false);
            }
            else // Vertical
            {
                float w = targetRect.rect.width - (leftPadding + rightPadding);
                float h = targetRect.rect.height - (topPadding + bottomPadding);
                if (w <= 0f || h <= 0f) { DisableAll(); return; }

                int start = drawEnds ? 0 : 1;
                int end = drawEnds ? divisions : divisions - 1;
                int needed = Mathf.Max(0, end - start + 1);
                EnsurePool(needed);

                for (int i = 0; i < needed; i++)
                {
                    var img = _pool[i];
                    img.gameObject.SetActive(true);
                    img.color = new Color(color.r, color.g, color.b, alpha);

                    var rt = img.rectTransform;
                    rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(Mathf.Max(px, w), px); // yatay çizgi

                    float y = Mathf.Round(bottomPadding + (h * (start + i) / divisions));
                    rt.anchoredPosition = new Vector2(0f, y);
                }

                for (int i = needed; i < _pool.Count; i++) _pool[i].gameObject.SetActive(false);
            }
        }

        void EnsurePool(int count)
        {
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
            while (_pool.Count < count)
            {
                var img = Object.Instantiate(tickTemplate, ticksContainer);
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
