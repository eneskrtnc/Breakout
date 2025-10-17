using UnityEngine;
using UnityEngine.UI;

namespace SpaceTrader.Game.UI
{
    /// <summary>
    /// Bar'ı piksel alanında sabit hızla (px/s) hedefe taşır (pixel-perfect).
    /// Horizontal/Vertical yönleri destekler. Opsiyonel back bar ve "full" FX (flash/pulse).
    /// </summary>
    [ExecuteAlways]
    public class UiBar : MonoBehaviour
    {
        public enum Orientation { Horizontal, Vertical }
        public enum FullFxMode { None, FlashOnce, PulseWhileFull }

        [Header("Orientation")]
        public Orientation orientation = Orientation.Horizontal; // Energy için Vertical

        [Header("Targets")]
        public RectTransform fillRt;         // Mask altındaki ÖN bar (asıl)
        public Image fillImage;      // (ops) tint
        [Tooltip("Opsiyonel: hasar animasyonu için geciken arka bar")]
        public RectTransform backFillRt;     // Mask altındaki ARKA bar (ops)

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

        [Header("Full FX")]
        public FullFxMode fullFxMode = FullFxMode.FlashOnce;
        [Range(0.95f, 1f)] public float fullThreshold = 0.999f;
        public Image fullFxImage;                 // Full olduğunda parlayacak overlay Image
        public Color fullFxColor = Color.white;   // Bu renge boyanır
        [Tooltip("Flash süreleri (sn): in/hold/out")]
        public float flashIn = 0.08f, flashHold = 0.10f, flashOut = 0.25f;
        [Tooltip("Pulse periyodu (sn) – full kaldığı sürece")]
        public float pulsePeriod = 0.8f;
        [Tooltip("Pulse minimum/maksimum opaklık")]
        [Range(0f, 1f)] public float pulseAlphaMin = 0.10f;
        [Range(0f, 1f)] public float pulseAlphaMax = 0.40f;

        // Dahili durum
        float _maxLen;    // yatayda: genişlik, dikeyde: yükseklik (px)
        float _frontLen;  // ön bar mevcut piksel uzunluğu
        float _backLen;   // arka bar mevcut piksel uzunluğu
        Canvas _canvas;

        bool _wasFull;
        float _fxTimer;   // flash zamanlayıcı
        bool _flashPlaying;

        void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            CacheMax();
            SnapImmediate();
        }

        void OnEnable()
        {
            CacheMax();
            SnapImmediate();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            value01 = Mathf.Clamp01(value01);
            pixelsPerSecond = Mathf.Max(0f, pixelsPerSecond);
            backPixelsPerSecond = Mathf.Max(0f, backPixelsPerSecond);
            pulsePeriod = Mathf.Max(0.01f, pulsePeriod);
            flashIn = Mathf.Max(0f, flashIn);
            flashHold = Mathf.Max(0f, flashHold);
            flashOut = Mathf.Max(0f, flashOut);

            if (!Application.isPlaying) { CacheMax(); SnapImmediate(); }
        }
#endif

        void OnRectTransformDimensionsChange()
        {
            CacheMax();
            if (!Application.isPlaying) SnapImmediate();
        }

        void Update()
        {
            if (!Application.isPlaying) return;
            float dt = Time.unscaledDeltaTime;
            StepAnimate(dt);
            StepFullFx(dt);
        }

        // --- Helpers ---
        void CacheMax()
        {
            if (!fillRt) return;
            var parent = fillRt.parent as RectTransform; // Mask
            if (parent)
                _maxLen = (orientation == Orientation.Horizontal) ? parent.rect.width : parent.rect.height;
            else
                _maxLen = (orientation == Orientation.Horizontal) ? fillRt.sizeDelta.x : fillRt.sizeDelta.y;

            _maxLen = Mathf.Max(0f, _maxLen);
        }

        // Hedef piksel uzunluğu – pixel-perfect
        float TargetLen()
        {
            float px = _canvas ? 1f / Mathf.Max(1f, _canvas.scaleFactor) : 1f;
            return Mathf.Round((_maxLen * Mathf.Clamp01(value01)) / px) * px;
        }

        void SnapImmediate()
        {
            float t = TargetLen();
            _frontLen = t;
            _backLen = Mathf.Max(_backLen, t); // arka bar geride kalabilir
            ApplyDims();
            ApplyTint();
            ApplyFullFxInstant();
        }

        void StepAnimate(float dt)
        {
            float t = TargetLen();

            // Ön bar → hedefe sabit piksel hızıyla
            _frontLen = MoveTowardsPixels(_frontLen, t, pixelsPerSecond * dt);

            // Arka bar (varsa) → sadece GERİDEN gelir (damage chip)
            if (backFillRt && backPixelsPerSecond > 0f)
            {
                if (_backLen > _frontLen)
                    _backLen = MoveTowardsPixels(_backLen, _frontLen, backPixelsPerSecond * dt);
                else
                    _backLen = _frontLen; // heal'de anında yetişsin (istersen hız ver)
            }

            ApplyDims();
            ApplyTint();
        }

        static float MoveTowardsPixels(float current, float target, float step)
        {
            if (Mathf.Approximately(current, target)) return target;
            float dir = Mathf.Sign(target - current);
            float next = current + dir * step;
            if ((dir > 0f && next > target) || (dir < 0f && next < target)) next = target;
            return next;
        }

        void ApplyDims()
        {
            if (!fillRt) return;

            var s = fillRt.sizeDelta;
            if (orientation == Orientation.Horizontal) s.x = _frontLen; else s.y = _frontLen;
            fillRt.sizeDelta = s;

            if (backFillRt)
            {
                var sb = backFillRt.sizeDelta;
                if (orientation == Orientation.Horizontal) sb.x = _backLen; else sb.y = _backLen;
                backFillRt.sizeDelta = sb;
            }
        }

        void ApplyTint()
        {
            if (!fillImage) return;
            float k = Mathf.InverseLerp(criticalThreshold, 0f, value01);
            fillImage.color = Color.Lerp(normalTint, criticalTint, Mathf.Clamp01(k));
        }

        // --- FULL FX ---

        void ApplyFullFxInstant()
        {
            if (!fullFxImage) return;

            bool isFull = value01 >= fullThreshold;
            _wasFull = isFull;
            _flashPlaying = false;
            _fxTimer = 0f;

            var c = fullFxColor;
            c.a = (fullFxMode == FullFxMode.PulseWhileFull && isFull) ? pulseAlphaMin : 0f;
            fullFxImage.color = c;
        }

        void StepFullFx(float dt)
        {
            if (!fullFxImage || fullFxMode == FullFxMode.None)
                return;

            bool isFull = value01 >= fullThreshold;

            // eşik yakalandı (rising edge)
            if (isFull && !_wasFull)
            {
                if (fullFxMode == FullFxMode.FlashOnce)
                {
                    _flashPlaying = true;
                    _fxTimer = 0f;
                    // rengi uygula
                    var c = fullFxColor; c.a = 0f; fullFxImage.color = c;
                }
            }

            if (fullFxMode == FullFxMode.FlashOnce)
            {
                if (_flashPlaying)
                {
                    _fxTimer += dt;
                    float a = 0f;
                    float t = _fxTimer;
                    if (t <= flashIn)                          // fade in
                        a = Mathf.Clamp01(t / flashIn);
                    else if (t <= flashIn + flashHold)         // hold
                        a = 1f;
                    else if (t <= flashIn + flashHold + flashOut) // fade out
                        a = 1f - Mathf.Clamp01((t - flashIn - flashHold) / flashOut);
                    else
                    {
                        a = 0f;
                        _flashPlaying = false; // bitti
                    }

                    var c = fullFxColor; c.a = a;
                    fullFxImage.color = c;
                }
                else
                {
                    // full değilse overlay kapalı kalsın
                    if (!isFull)
                    {
                        var c = fullFxImage.color; c.a = 0f; fullFxImage.color = c;
                    }
                }
            }
            else if (fullFxMode == FullFxMode.PulseWhileFull)
            {
                if (isFull)
                {
                    _fxTimer += dt;
                    float phase = Mathf.PingPong(_fxTimer / pulsePeriod, 1f);
                    float a = Mathf.Lerp(pulseAlphaMin, pulseAlphaMax, phase);
                    var c = fullFxColor; c.a = a; fullFxImage.color = c;
                }
                else
                {
                    _fxTimer = 0f;
                    var c = fullFxImage.color; c.a = 0f; fullFxImage.color = c;
                }
            }

            _wasFull = isFull;
        }

        // Dıştan çağır
        public void Set01(float t) => value01 = Mathf.Clamp01(t);
    }
}
