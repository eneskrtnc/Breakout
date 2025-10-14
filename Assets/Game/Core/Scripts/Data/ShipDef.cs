using UnityEngine;

namespace SpaceTrader.Core.Data
{
    [CreateAssetMenu(menuName = "Game/Defs/Ship", fileName = "ShipDef")]
    public class ShipDef : BaseDef
    {
        [Header("Ship Stats")]
        public int hp = 10;
        public int cargo = 4;
        public float speed = 3f;

        // Bu türün anahtar ön eki
        public override string KeyPrefix => "ship";
    }
}
