// Örneğin Core sahnesinde bir GO'ya ekle
using UnityEngine;
using SpaceTrader.Core;
using SpaceTrader.Core.Data;

public class GenericSmokeTest : MonoBehaviour
{
    public SpawnService spawner;

    private void Start()
    {
        // Tip güvenli spawn: ShipDef
        var go1 = spawner.Spawn<ShipDef>("starter_ship", Vector3.zero, Quaternion.identity);

        // Anahtarla spawn (prefab library'e birebir)
        var go2 = spawner.SpawnByKey("ship.starter_ship", new Vector3(2, 0, 0), Quaternion.identity);

        Debug.Log($"[GenericSmokeTest] {(go1 && go2 ? "OK" : "FAIL")}");
    }
}
