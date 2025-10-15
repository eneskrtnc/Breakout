using SpaceTrader.Core.Pooling;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SpaceTrader.Core.Tests
{
    /// <summary>
    /// Test spawner: Space'e basıldığında havuzdan VFX üretir.
    /// </summary>
    public class VfxPoolSpawner : MonoBehaviour
    {
        public SimplePool vfxPool;
        public Transform spawnPoint;
        public Vector3 offset;

        void Update()
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                Spawn();
        }

        public void Spawn()
        {
            if (!vfxPool)
                return;

            var pos = (spawnPoint ? spawnPoint.position : transform.position) + offset;
            var rot = spawnPoint ? spawnPoint.rotation : Quaternion.identity;

            var go = vfxPool.Get(pos, rot, null);
            // Not: go üzerinde VfxOneShotPoolReturner olmalı ki bitince iade etsin.
        }
    }
}
