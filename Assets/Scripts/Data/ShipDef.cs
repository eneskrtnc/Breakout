using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SpaceTrader.Data
{
    [CreateAssetMenu(menuName = "SpaceTrader/Ship", fileName = "Ship_")]
    [DataLabel("ships")]
    public class ShipDef : BaseDef
    {
        [Header("Stats")]
        public float hp = 100;
        public float cargo = 8;
        public float speed = 12.5f;

        [Header("Assets")]
        public AssetReferenceSprite iconRef;
        public AssetReferenceGameObject prefabRef;
    }
}
