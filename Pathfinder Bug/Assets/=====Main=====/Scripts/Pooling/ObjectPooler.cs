using System.Collections.Generic;
using Clouds.Ultilities;
using Sirenix.Serialization;
using UnityEngine;
public class ObjectPooler : Singleton<ObjectPooler>, IObjectPooler 
{
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary;
    public override void Awake()
    {
        base.Awake();
        poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
    }

    public void CreatePool(GameObject prefab, int initialSize)
    {
        if (prefab == null)
        {
            Debug.LogError("Cannot create pool for null prefab.");
            return;
        }

        if (!poolDictionary.ContainsKey(prefab))
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = Instantiate(prefab, transform); // Parent to pooler for organization
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(prefab, objectPool);
            Debug.Log($"Created pool for {prefab.name} with initial size {initialSize}");
        }
    }

    public GameObject GetPooledObject(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
    {
        if (!poolDictionary.ContainsKey(prefab))
        {
            Debug.LogWarning($"Pool for {prefab.name} does not exist. Creating a small pool now.");
            CreatePool(prefab, 5); 
            if (!poolDictionary.ContainsKey(prefab)) 
                return null;
        }

        Queue<GameObject> objectPool = poolDictionary[prefab];
        GameObject objToSpawn;

        if (objectPool.Count > 0)
        {
            objToSpawn = objectPool.Dequeue();
        }
        else
        {
            //If  Pool for {prefab.name} is empty. Instantiating new object. Consider increasing initial pool size.
            objToSpawn = Instantiate(prefab, transform); 
        }

        objToSpawn.transform.position = position;
        objToSpawn.transform.rotation = rotation;
        objToSpawn.transform.SetParent(parent); 
        objToSpawn.SetActive(true);

        IPoolable poolable = objToSpawn.GetComponent<IPoolable>();
        poolable?.OnGetFromPool();

        return objToSpawn;
    }
    
    public GameObject GetPooledObject(GameObject prefab)
    {
        return GetPooledObject(prefab, Vector3.zero, Quaternion.identity, transform); 
    }
    public void ReturnPooledObject(GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false);
        obj.transform.SetParent(transform); // Parent back to pooler for clean hierarchy

        IPoolable poolable = obj.GetComponent<IPoolable>();
        poolable?.OnReturnToPool();

        PooledObjectInfo info = obj.GetComponent<PooledObjectInfo>();
        if (info != null && info.originalPrefab != null && poolDictionary.ContainsKey(info.originalPrefab))
        {
            poolDictionary[info.originalPrefab].Enqueue(obj);
        }
        else
        {
            //If Could not return {obj.name} to pool. Destroying it instead.
            Destroy(obj);
        }
    }
}