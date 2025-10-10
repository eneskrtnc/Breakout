// Assets/Scripts/ShipSmokeTest.cs
using SpaceTrader.Data;
using UnityEngine;

public class ShipSmokeTest : MonoBehaviour
{
    private async void Start()
    {
        await GameDatabase.InitAsync();

        var def = GameDatabase.Get<ShipDef>("starter_ship");
        Debug.Log(def ? $"Ship: {def.displayName}" : "starter_ship yok");

        if (def?.prefabRef != null)
        {
            var prefab = await def.prefabRef.LoadAssetAsync<GameObject>().Task;
            if (prefab)
                Instantiate(prefab, Vector3.zero, Quaternion.identity);
        }
    }
}
