using SpaceTrader.Core.Pooling;
using UnityEngine;
using UnityEngine.VFX;

namespace SpaceTrader.Core.VFX
{
    /// <summary>
    /// 1) VFX Graph Output Event (finishEventName) gelirse iade eder.
    /// 2) Event gelmezse aliveParticleCount==0 durumunu izleyip iade eder.
    /// 3) İstersen sabit süre (fallbackLifetime) sonunda iade eder.
    /// </summary>
    [RequireComponent(typeof(VisualEffect))]
    public class VfxOneShotPoolReturner : MonoBehaviour, IPoolable
    {
        [Header("Event ile iade")]
        [Tooltip(
            "VFX Graph Output Event adı (Graph'ta Output Event Name ile eşleşmeli). Boş bırakılabilir."
        )]
        public string finishEventName = "Finished";

        [Header("Süreye dayalı emniyet")]
        [Tooltip("Output Event gelmezse X sn sonra iade (0 = kapalı)")]
        public float fallbackLifetime = 0f;

        [Header("Particle sayısını izleyerek iade")]
        [Tooltip("Yaşayan partikül sayısı 0 olduğunda iade et")]
        public bool returnWhenNoAlive = true;

        [Tooltip("0 alive üst üste kaç frame görüldüğünde iade edilsin")]
        public int noAliveFramesToReturn = 2;

        [Tooltip(
            "Başladıktan sonra kaç saniye geçmeden '0 alive' iadesini yapmayalım (erken iade önlemi)"
        )]
        public float minUptime = 0.05f;

        VisualEffect _vfx;
        PooledObject _po;
        int _finishEventId;
        bool _playing;
        int _zeroAliveFrames;
        float _startedAt;

        void Awake()
        {
            _vfx = GetComponent<VisualEffect>();
            _po = GetComponent<PooledObject>();
            _finishEventId = !string.IsNullOrEmpty(finishEventName)
                ? Shader.PropertyToID(finishEventName)
                : -1;
        }

        void OnEnable()
        {
            if (_vfx != null && _finishEventId != -1)
                _vfx.outputEventReceived += OnVfxEvent;

            if (fallbackLifetime > 0f)
                Invoke(nameof(ReturnByTimer), fallbackLifetime);
        }

        void OnDisable()
        {
            if (_vfx != null && _finishEventId != -1)
                _vfx.outputEventReceived -= OnVfxEvent;

            if (IsInvoking(nameof(ReturnByTimer)))
                CancelInvoke(nameof(ReturnByTimer));
        }

        // Yeni API: Action<VFXOutputEventArgs>
        void OnVfxEvent(VFXOutputEventArgs args)
        {
            if (_finishEventId != -1 && args.nameId == _finishEventId)
                ReturnNow();
        }

        void Update()
        {
            if (!_playing || !returnWhenNoAlive || _vfx == null)
                return;

            if (Time.unscaledTime - _startedAt < minUptime)
                return;

            // Event gelmiyorsa doğal bitiş: yaşayan partikül sayısı 0
            if (_vfx.aliveParticleCount <= 0)
            {
                _zeroAliveFrames++;
                if (_zeroAliveFrames >= noAliveFramesToReturn)
                    ReturnNow();
            }
            else
            {
                _zeroAliveFrames = 0;
            }
        }

        void ReturnByTimer() => ReturnNow();

        void ReturnNow()
        {
            if (IsInvoking(nameof(ReturnByTimer)))
                CancelInvoke(nameof(ReturnByTimer));

            _playing = false;
            _po?.ReturnToPool();
        }

        // Havuz kancaları
        public void OnTakenFromPool()
        {
            _zeroAliveFrames = 0;
            _startedAt = Time.unscaledTime;
            _playing = true;

            if (_vfx != null)
            {
                // Üst üste binmeyi kesin olarak engelle:
                _vfx.Stop();
                _vfx.Reinit();
                _vfx.Play();
            }
        }

        public void OnReturnedToPool()
        {
            _playing = false;
            if (_vfx != null)
                _vfx.Stop();
        }
    }
}
