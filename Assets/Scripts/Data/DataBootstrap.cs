using UnityEngine;

namespace SpaceTrader.Data
{
    public class DataBootstrap : MonoBehaviour
    {
        private async void Awake()
        {
            await GameDatabase.InitAsync();
            Debug.Log("[Bootstrap] GameDB ready.");
        }
    }
}
