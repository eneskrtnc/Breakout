using SpaceTrader.Game.Data;
using SpaceTrader.Game.Runtime.Services;
using UnityEngine;

namespace SpaceTrader.Game.Dev
{
    public class ShipSmokeTest : MonoBehaviour
    {
        public string shipId = "starter_ship";
        GameObject spawned;

        async void Start()
        {
            var db = GameDatabase.Instance;
            if (!db)
            {
                Debug.LogError(
                    "[ShipSmokeTest] GameDatabase bulunamadı. DataBootstrap ve GameDatabase.asset ekli mi?"
                );
                return;
            }

            await db.InitAsync();

            if (AssetService.Instance == null)
            {
                Debug.LogError(
                    "[ShipSmokeTest] AssetService sahnede yok. GameSystems objesine ekleyin."
                );
                return;
            }

            spawned = await AssetService.Instance.SpawnAsync(
                shipId,
                Vector3.zero,
                Quaternion.identity
            );
        }

        void OnDestroy()
        {
            if (spawned)
                AssetService.Instance?.Despawn(spawned);
        }
    }
}
