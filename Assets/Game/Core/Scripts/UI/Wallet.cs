// Assets/Game/Core/Scripts/UI/CoinHudBinder.cs
using UnityEngine;

namespace SpaceTrader.Game.UI
{
    public class Wallet : MonoBehaviour
    {
        public int Coins { get; private set; }
        public System.Action<int> OnCoinsChanged;

        public void Add(int amount) => Set(Mathf.Max(0, Coins + amount));

        public void Set(int value)
        {
            if (value == Coins)
                return;
            Coins = value;
            OnCoinsChanged?.Invoke(Coins);
        }

        void Awake()
        {
            var wallet = FindFirstObjectByType<Wallet>();
            wallet.Add(100);
            wallet.Set(500);
        }
    }
}
