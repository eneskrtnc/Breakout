using UnityEngine;

namespace SpaceTrader.Core.Pooling
{
    /// <summary>
    /// Bir GameObject'in hangi havuza ait olduğunu tutar ve iade etmeyi sağlar.
    /// </summary>
    public class PooledObject : MonoBehaviour
    {
        public SimplePool OriginPool { get; private set; }
        bool _attached;

        internal void AttachToPool(SimplePool pool)
        {
            OriginPool = pool;
            _attached = true;
        }

        /// <summary> Objeyi geldiği havuza iade eder. </summary>
        public void ReturnToPool()
        {
            if (_attached && OriginPool != null)
                OriginPool.Return(gameObject);
            else
                gameObject.SetActive(false);
        }
    }
}
