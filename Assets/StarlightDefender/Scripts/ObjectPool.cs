using System.Collections.Generic;
using UnityEngine;

namespace StarlightDefender
{
    public interface IPoolable
    {
        void OnPoolSpawned();
        void OnPoolDespawned();
    }

    public sealed class ObjectPool : MonoBehaviour
    {
        public static ObjectPool Instance { get; private set; }

        private readonly Dictionary<int, Queue<GameObject>> pools = new();
        private readonly Dictionary<int, int> instanceKeys = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;
            int key = prefab.GetInstanceID();
            if (!pools.TryGetValue(key, out Queue<GameObject> queue))
            {
                queue = new Queue<GameObject>();
                pools.Add(key, queue);
            }

            GameObject instance = null;
            while (queue.Count > 0 && instance == null) instance = queue.Dequeue();
            if (instance == null)
            {
                instance = Instantiate(prefab);
                instance.name = prefab.name;
                instanceKeys[instance.GetInstanceID()] = key;
            }

            instance.transform.SetParent(null);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);
            Notify(instance, true);
            return instance;
        }

        public void Despawn(GameObject instance)
        {
            if (instance == null || !instance.activeSelf) return;
            Notify(instance, false);
            int id = instance.GetInstanceID();
            if (!instanceKeys.TryGetValue(id, out int key) || !pools.TryGetValue(key, out Queue<GameObject> queue))
            {
                Destroy(instance);
                return;
            }
            instance.SetActive(false);
            instance.transform.SetParent(transform);
            queue.Enqueue(instance);
        }

        private static void Notify(GameObject target, bool spawned)
        {
            MonoBehaviour[] behaviours = target.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is not IPoolable poolable) continue;
                if (spawned) poolable.OnPoolSpawned();
                else poolable.OnPoolDespawned();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
