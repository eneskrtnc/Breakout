using System.Collections.Generic;
using UnityEngine;

namespace SpaceTrader.Core.Pooling
{
    /// <summary>
    /// Basit, tek-prefab GameObject havuzu.
    /// Prewarm yapar, maxSize ile sınır koyulabilir (0=sınırsız).
    /// </summary>
    [DefaultExecutionOrder(-50)]
    public class SimplePool : MonoBehaviour
    {
        [Header("Setup")]
        public GameObject prefab;

        [Min(0)]
        public int initialSize = 10; // Başlangıç üretim adedi

        [Min(0)]
        public int maxSize = 0; // 0 = sınırsız
        public Transform container; // Boşsa bu objenin altı

        readonly Queue<PooledObject> _queue = new Queue<PooledObject>();
        int _totalCreated;

        void Awake()
        {
            if (!container)
                container = transform;
            if (!prefab)
            {
                Debug.LogError("[SimplePool] Prefab atanmadı.", this);
                enabled = false;
                return;
            }
            Prewarm(initialSize);
        }

        void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
                EnqueueNewInstance();
        }

        PooledObject EnqueueNewInstance()
        {
            if (maxSize > 0 && _totalCreated >= maxSize)
                return null;

            var go = Instantiate(prefab, container, false);
            go.SetActive(false);

            var po = go.GetComponent<PooledObject>();
            if (!po)
                po = go.AddComponent<PooledObject>();
            po.AttachToPool(this);

            _totalCreated++;
            _queue.Enqueue(po);
            return po;
        }

        /// <summary> Havuzdan obje alır ve konum/rotasyon ayarlar. </summary>
        public GameObject Get(Vector3 pos, Quaternion rot, Transform parent = null)
        {
            if (_queue.Count == 0)
            {
                var created = EnqueueNewInstance();
                if (!created)
                {
                    Debug.LogWarning("[SimplePool] Havuz tükendi ve maxSize'a ulaşıldı.", this);
                    return null;
                }
            }

            var po = _queue.Dequeue();
            var go = po.gameObject;

            if (parent)
                go.transform.SetParent(parent, true);
            go.transform.SetPositionAndRotation(pos, rot);
            go.SetActive(true);

            // Kancalar
            var hooks = go.GetComponentsInChildren<IPoolable>(true);
            for (int i = 0; i < hooks.Length; i++)
                hooks[i].OnTakenFromPool();

            return go;
        }

        /// <summary> Objeyi havuza iade eder. Yanlış havuza iade edilirse sessizce devre dışı bırakır. </summary>
        public void Return(GameObject go)
        {
            if (!go)
                return;

            var po = go.GetComponent<PooledObject>();
            if (!po || po.OriginPool != this)
            {
                go.SetActive(false);
                return;
            }

            // Kancalar
            var hooks = go.GetComponentsInChildren<IPoolable>(true);
            for (int i = 0; i < hooks.Length; i++)
                hooks[i].OnReturnedToPool();

            go.SetActive(false);
            go.transform.SetParent(container, false);
            _queue.Enqueue(po);
        }
    }
}
