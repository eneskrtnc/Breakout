// Assets/Game/Core/Scripts/UI/CoinHudBinder.cs
using UnityEngine;

namespace SpaceTrader.Game.UI
{
    public class CoinHudBinder : MonoBehaviour
    {
        public Wallet wallet;
        public UiCounterBitmap counter;

        void OnEnable()
        {
            if (!wallet || !counter)
                return;
            wallet.OnCoinsChanged += counter.SetValue;
            counter.SetValue(wallet.Coins);
        }

        void OnDisable()
        {
            if (wallet != null)
                wallet.OnCoinsChanged -= counter.SetValue;
        }
    }
}
