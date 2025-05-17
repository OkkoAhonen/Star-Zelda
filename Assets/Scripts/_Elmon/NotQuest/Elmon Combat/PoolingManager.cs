using System.Collections.Generic;
using UnityEngine;

public class PoolingManager : MonoBehaviour
{
    public static PoolingManager Instance { get; private set; }

    [System.Serializable]
    public struct Pool
    {
        public GameObject prefab;   // splash prefab
        public int initialSize;  // how many to pre-warm
        public int maxSize;      // maximum pooled instances
    }

    [Tooltip("Configure each splash prefab, its pre-warm count, and max pool size")]
    public List<Pool> pools;

    private class PoolData
    {
        public Queue<GameObject> queue;
        public int totalCount;     // active + pooled
        public int maxSize;        // from Pool.maxSize
    }

    private Dictionary<GameObject, PoolData> _poolDictionary;
    private Transform _projectilesParent;

    void Awake()
    {
        // singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // find or make the Projectiles parent
        var go = GameObject.Find("Projectiles") ?? new GameObject("Projectiles");
        _projectilesParent = go.transform;

        // build pools
        _poolDictionary = new Dictionary<GameObject, PoolData>();
        foreach (var pool in pools)
        {
            var data = new PoolData
            {
                queue = new Queue<GameObject>(),
                totalCount = 0,
                maxSize = pool.maxSize
            };

            // pre-warm
            for (int i = 0; i < pool.initialSize; i++)
            {
                var obj = Instantiate(pool.prefab, _projectilesParent);
                obj.SetActive(false);
                obj.GetComponent<SplashArea>().prefabReference = pool.prefab;
                data.queue.Enqueue(obj);
                data.totalCount++;
            }

            _poolDictionary[pool.prefab] = data;
        }
    }

    /// <summary>
    /// Gets you an active instance (recycled or new up to maxSize).  
    /// Returns null if you’re at maxSize and the pool is empty.
    /// </summary>
    public GameObject GetPooledObject(GameObject prefab)
    {
        if (!_poolDictionary.TryGetValue(prefab, out var data))
        {
            Debug.LogWarning($"[PoolingManager] No pool for {prefab.name}");
            return null;
        }

        // 1) reuse if available
        if (data.queue.Count > 0)
        {
            var obj = data.queue.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        // 2) else if we haven’t yet hit maxSize, instantiate
        if (data.totalCount < data.maxSize)
        {
            var obj = Instantiate(prefab, _projectilesParent);
            obj.SetActive(true);
            obj.GetComponent<SplashArea>().prefabReference = prefab;
            data.totalCount++;
            return obj;
        }

        // 3) pool empty + at cap ? nothing to give
        return null;
    }

    /// <summary>
    /// Returns an object to its pool, or destroys it if the pool is already full.
    /// </summary>
    public void ReturnToPool(GameObject prefab, GameObject obj)
    {
        if (_poolDictionary.TryGetValue(prefab, out var data))
        {
            // only enqueue back if we haven't reached maxSize in the queue
            if (data.queue.Count < data.maxSize)
            {
                obj.SetActive(false);
                data.queue.Enqueue(obj);
                return;
            }
            // otherwise destroy and reduce totalCount
            data.totalCount--;
        }
        else
        {
            Debug.LogWarning($"[PoolingManager] Returning unpooled object {prefab.name}");
        }

        Destroy(obj);
    }
}
