using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceTrader.Game.UI
{
    /// <summary>
    /// 5x7 bitmap sayaç. Sağ üst HUD için:
    /// - alignRight=true: sağa yaslanır, sola doğru büyür (doğru sırada)
    /// - Coin solda; sayı büyüdükçe coin sola kayar
    /// - Glif boyu sabit: 6x8 px (SetNativeSize kullanılmaz)
    /// </summary>
    [ExecuteAlways]
    public class UiCounterBitmap : MonoBehaviour
    {
        [Header("Refs")]
        public RectTransform digitsRoot; // "Digits" container
        public Image digitTemplate; // disabled Image (5x7 sprite, herhangi biri)

        [Tooltip("0..9 zorunlu; (+,-,x,/,:) opsiyonel")]
        public Sprite[] glyphs;

        [Header("Layout")]
        public bool alignRight = true; // sağa yasla, sola büyüt
        public int pixelSpacing = 1; // karakterler arası PX

        [Header("Icon (optional)")]
        public RectTransform iconRt; // coin
        public int iconSpacing = 4;
        public bool iconOnLeft = true; // coin solda

        [Header("Optional: parent layout size")]
        public LayoutElement layoutElement;

        [Header("Value")]
        public int value = 0;

        // Sabit glif ölçüleri (sheet hücresi 6x8)
        const int GLYPH_W = 18;
        const int GLYPH_H = 24;

        Canvas _canvas;
        readonly List<Image> _pool = new();

        void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            EnsureTemplateDisabled();
            AdoptExistingChildren();
        }

        void OnEnable()
        {
            EnsureTemplateDisabled();
            AdoptExistingChildren();
            HideAll();
            Redraw();
        }

        void OnDisable() => HideAll();

#if UNITY_EDITOR
        void OnValidate()
        {
            EnsureTemplateDisabled();
            if (!Application.isPlaying)
            {
                AdoptExistingChildren();
                HideAll();
                Redraw();
            }
        }
#endif

        public void SetValue(int v)
        {
            if (Application.isPlaying && v == value)
                return;
            value = v;
            Redraw();
        }

        void Redraw()
        {
            if (!digitsRoot || !digitTemplate || glyphs == null || glyphs.Length < 10)
                return;

            // 1) Değeri karakterlere çevir
            var chars = ToCharArray(value);

            // 2) Havuz hazırlığı
            EnsureCapacity(chars.Length);
            HideAll();

            // 3) 1 ekran pikselinin UI birimindeki karşılığı
            float sf = Mathf.Max(1f, _canvas ? _canvas.scaleFactor : 1f);
            float px = 1f / sf;

            int gap = Mathf.Max(0, pixelSpacing);

            // 4) Toplam genişlik (px cinsinden)
            int totalWpx = chars.Length * GLYPH_W + Mathf.Max(0, chars.Length - 1) * gap;

            // 5) Yerleşim (alignRight=true → doğru sırayla sola büyüt)
            for (int i = 0; i < chars.Length; i++)
            {
                int visualIndex = alignRight ? (chars.Length - 1 - i) : i;
                char ch = chars[visualIndex];

                int gi = CharToGlyphIndex(ch);
                if (gi < 0 || gi >= glyphs.Length)
                    gi = 0;

                var img = _pool[i];
                img.sprite = glyphs[gi];
                img.gameObject.SetActive(true);

                var rt = img.rectTransform;

                // SABİT 6x8 px boy ver (SetNativeSize YOK)
                rt.sizeDelta = new Vector2(GLYPH_W * px, GLYPH_H * px);
                rt.localScale = Vector3.one;

                if (alignRight)
                {
                    // Sağ kenara yasla; i=0 en sağdaki basamak
                    rt.anchorMin = rt.anchorMax = new Vector2(1f, 0.5f);
                    rt.pivot = new Vector2(1f, 0.5f);

                    int xPixels = -((i + 1) * GLYPH_W + i * gap); // tam px
                    rt.anchoredPosition = new Vector2(xPixels * px, 0f);
                }
                else
                {
                    // Sola yasla; sağa doğru büyüt
                    rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
                    rt.pivot = new Vector2(0f, 0.5f);

                    int xPixels = i * (GLYPH_W + gap);
                    rt.anchoredPosition = new Vector2(xPixels * px, 0f);
                }
            }

            // 6) Coin’i sola kaydır
            if (iconRt)
            {
                iconRt.anchorMin = iconRt.anchorMax = new Vector2(1f, 0.5f);
                iconRt.pivot = new Vector2(1f, 0.5f);

                int iconXPixels = iconOnLeft ? -(totalWpx + iconSpacing) : 0;
                iconRt.anchoredPosition = new Vector2(iconXPixels * px, 0f);
            }

            // 7) Parent layout’a preferred size bildir (kullanıyorsan)
            if (layoutElement)
            {
                float iconW = iconRt ? iconRt.rect.width : GLYPH_W * px;
                float iconH = iconRt ? iconRt.rect.height : GLYPH_H * px;
                float width = iconOnLeft
                    ? (iconW + iconSpacing * px + totalWpx * px)
                    : (totalWpx * px + iconSpacing * px + iconW);
                float height = Mathf.Max(iconH, GLYPH_H * px);
                layoutElement.preferredWidth = width;
                layoutElement.preferredHeight = height;
            }
        }

        // --- Helpers ---

        static char[] ToCharArray(int v)
        {
            if (v == 0)
                return new[] { '0' };
            bool neg = v < 0;
            if (neg)
                v = -v;
            var s = v.ToString();
            if (neg)
                s = "-" + s;
            return s.ToCharArray();
        }

        int CharToGlyphIndex(char c)
        {
            if (c >= '0' && c <= '9')
                return c - '0';
            return c switch
            {
                '+' => 10,
                '-' => 11,
                'x' => 12,
                '/' => 13,
                ':' => 14,
                _ => 0,
            };
        }

        void EnsureTemplateDisabled()
        {
            if (!digitTemplate)
                return;
            digitTemplate.raycastTarget = false;
            if (digitTemplate.gameObject.activeSelf)
                digitTemplate.gameObject.SetActive(false);
        }

        void HideAll()
        {
            for (int i = 0; i < _pool.Count; i++)
                if (_pool[i])
                    _pool[i].gameObject.SetActive(false);
        }

        void AdoptExistingChildren()
        {
            if (!digitsRoot || _pool.Count > 0)
                return;

            for (int i = 0; i < digitsRoot.childCount; i++)
            {
                var t = digitsRoot.GetChild(i);
                if (digitTemplate && t == digitTemplate.transform)
                    continue;

                var img = t.GetComponent<Image>();
                if (!img)
                    continue;

                ConfigureDigitImage(img);
                img.gameObject.SetActive(false);
                _pool.Add(img);
            }
        }

        void EnsureCapacity(int need)
        {
            AdoptExistingChildren();
            while (_pool.Count < need)
            {
                var img = Instantiate(digitTemplate, digitsRoot);
                img.name = $"Digit_{_pool.Count}";
                ConfigureDigitImage(img);
                img.gameObject.SetActive(false);
                _pool.Add(img);
            }
        }

        static void ConfigureDigitImage(Image img)
        {
            img.raycastTarget = false;
            var le = img.GetComponent<LayoutElement>();
            if (!le)
                le = img.gameObject.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
        }
    }
}
