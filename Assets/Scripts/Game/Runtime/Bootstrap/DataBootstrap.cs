// Assets/Scripts/Game/Runtime/Bootstrap/DataBootstrap.cs
using SpaceTrader.Game.Data;
using UnityEngine;

namespace SpaceTrader.Game.Runtime.Bootstrap
{
    /// <summary>
    /// Oyun başında GameDatabase'i başlatır.
    /// Inspector'da 'database' atanmışsa onu kullanır; boşsa GameDatabase.Instance ile bulur.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    [AddComponentMenu("SpaceTrader/Bootstrap/DataBootstrap")]
    public class DataBootstrap : MonoBehaviour
    {
        [SerializeField]
        private GameDatabase database; // GameDatabase.asset'i buraya sürükleyebilirsin

        [SerializeField]
        private readonly bool verboseLog = true; // konsola durum yazsın mı?

        private async void Awake()
        {
            // Inspector boşsa otomatik bul
            if (!database)
                database = GameDatabase.Instance;

            if (!database)
            {
                Debug.LogError(
                    "[DataBootstrap] GameDatabase asset bulunamadı. "
                        + "Project'te GameDatabase.asset oluşturup bu bileşenin 'database' alanına atayın "
                        + "ya da Resources/GameDatabase.asset olarak yerleştirin."
                );
                return;
            }

            if (verboseLog)
                Debug.Log("[DataBootstrap] Initializing GameDatabase…");
            await database.InitAsync();

            if (verboseLog)
                Debug.Log("[DataBootstrap] Database ready: " + database.Health);
        }
    }
}
