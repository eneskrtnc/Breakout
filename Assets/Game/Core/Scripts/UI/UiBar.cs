using UnityEngine;
using UnityEngine.UI;

namespace SpaceTrader.Game.UI
{
    /// <summary>
    /// HP barı piksel alanında sabit hızla (px/s) hedef genişliğe taşır.
    /// Pixel-perfect kalır; opsiyonel "back/lag" bar desteği vardır.
    /// </summary>
    [ExecuteAlways]
    public class UiBar : MonoBehaviour
    {
        [Header("Targets")]
        public RectTransform fillRt;         // Mask altındaki ÖN bar (asıl)
        public Image fillImage;              // (opsiyonel) renk tint'i
        [Tooltip("Opsiyonel: hasar animasyonu için geciken arka bar")]
        public RectTransform backFillRt;     // Mask altındaki ARKA bar (isteğe bağlı)

        [Header("Value")]
        [Range(0, 1)] public float value01 = 1f;      // hedef değer (0..1)

        [Header("Animation")]
        [Tooltip("Ön bar hızı (px/s)")]
        public float pixelsPerSecond = 320f;
        [Tooltip("Arka bar hızı (px/s). 0 → arka bar devre dışı")]
        public float backPixelsPerSecond = 160f;

        [Header("Critical (optional)")]
        [Range(0, 1)] public float criticalThreshold = 0.25f;
        public Color normalTint = Color.white;
        public Color criticalTint = new Color(1f, 0.75f, 0.25f, 1f);

        float _maxW;            // mask iç genişliği (px)
        float _frontW;          // ön barın mevcut genişliği (px)
        float _backW;           // arka barın mevcut genişliği (px)
        Canvas _canvas;

        void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            CacheMaxWidth();
            SnapImmediate(); // editörde/ilk karede hedefe oturt
        }

        void OnEnable()
        {
            CacheMaxWidth();
            SnapImmediate();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            value01 = Mathf.Clamp01(value01);
            pixelsPerSecond = Mathf.Max(0f, pixelsPerSecond);
            backPixelsPerSecond = Mathf.Max(0f, backPixelsPerSecond);
            // Editörde anlık önizleme
            if (!Application.isPlaying)
            {
                CacheMaxWidth();
                SnapImmediate();
            }
        }
#endif

        void OnRectTransformDimensionsChange()
        {
            CacheMaxWidth();
            if (!Application.isPlaying) SnapImmediate();
        }

        void Update()
        {
            if (!Application.isPlaying) return;
            StepAnimate(Time.unscaledDeltaTime);
        }

        // --- Helpers ---

        void CacheMaxWidth()
        {
            if (!fillRt) return;
            var parent = fillRt.parent as RectTransform; // Mask
            _maxW = parent ? parent.rect.width : fillRt.sizeDelta.x;
            _maxW = Mathf.Max(0f, _maxW);
        }

        // Hedef genişliği (px) – pixel-perfect
        float TargetW()
        {
            float px = _canvas ? 1f / Mathf.Max(1f, _canvas.scaleFactor) : 1f;
            return Mathf.Round((_maxW * Mathf.Clamp01(value01)) / px) * px;
        }

        void SnapImmediate()
        {
            float tw = TargetW();
            _frontW = tw;
            _backW = Mathf.Max(_backW, tw); // arka bar geride kalabilir
            ApplyWidths();
            ApplyTint();
        }

        void StepAnimate(float dt)
        {
            float tw = TargetW();

            // Ön bar → hedefe sabit piksel hızıyla
            _frontW = MoveTowardsPixels(_frontW, tw, pixelsPerSecond * dt);

            // Arka bar (varsa) → sadece GERİDEN gelir (damage chip etkisi)
            if (backFillRt && backPixelsPerSecond > 0f)
            {
                // Ön bar küçülmüşse arka bar yavaşça onu yakalasın
                if (_backW > _frontW)
                    _backW = MoveTowardsPixels(_backW, _frontW, backPixelsPerSecond * dt);
                else
                    _backW = _frontW; // heal'de arka bar hemen yetişsin (istersen burada da hız ver)
            }

            ApplyWidths();
            ApplyTint();
        }

        static float MoveTowardsPixels(float current, float target, float step)
        {
            if (Mathf.Approximately(current, target)) return target;
            float dir = Mathf.Sign(target - current);
            float next = current + dir * step;
            // aşmayalım
            if ((dir > 0f && next > target) || (dir < 0f && next < target)) next = target;
            return next;
        }

        void ApplyWidths()
        {
            if (!fillRt) return;
            var s = fillRt.sizeDelta; s.x = _frontW; fillRt.sizeDelta = s;

            if (backFillRt)
            {
                var sb = backFillRt.sizeDelta; sb.x = _backW; backFillRt.sizeDelta = sb;
            }
        }

        void ApplyTint()
        {
            if (!fillImage) return;
            float k = Mathf.InverseLerp(criticalThreshold, 0f, value01);
            fillImage.color = Color.Lerp(normalTint, criticalTint, Mathf.Clamp01(k));
        }

        // Dıştan çağırmak için
        public void Set01(float t) => value01 = Mathf.Clamp01(t);
    }
}
