using UnityEngine;
using SpaceTrader.Core;

public class TestSpawn : MonoBehaviour
{
    public PrefabFactory factory;
    public string key = "debug.cube";
    public Vector3 position = new Vector3(0, 0, 0);

    private void Start()
    {
        var go = factory.Spawn(key, position, Quaternion.identity);
        Debug.Log("[TestSpawn] " + (go ? "OK" : "FAIL"));
    }
}
