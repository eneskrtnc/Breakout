using UnityEngine;

namespace SpaceTrader.Core
{
    /// Çekirdek veri/sistem erişim noktası (şimdilik minimal).
    public class GameDatabase : MonoBehaviour
    {
        public static GameDatabase Instance { get; private set; }

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
