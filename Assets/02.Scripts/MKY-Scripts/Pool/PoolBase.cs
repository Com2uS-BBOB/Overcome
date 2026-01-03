using UnityEngine;
using System.Collections.Generic;

public abstract class PoolBase : MonoBehaviour
{
    protected virtual void Awake()
    {
        PoolManager.Instance.RegisterPool(this);
    }
}

public abstract class PoolBase<TEnum, TObject> : MonoBehaviour
    where TEnum : System.Enum
    where TObject : Component
{
    protected Dictionary<TEnum, Queue<TObject>> _pool = new();
    protected Dictionary<TEnum, TObject> _prefabs = new();

    protected void RegisterType(TEnum type, TObject prefab, int initialSize)
    {
        _prefabs[type] = prefab;

        Queue<TObject> queue = new();

        for (int i = 0; i < initialSize; i++)
        {
            queue.Enqueue(Create(type));
        }

        _pool[type] = queue;
    }

    protected TObject Create(TEnum type)
    {
        TObject obj = Instantiate(_prefabs[type], transform);
        obj.gameObject.SetActive(false);
        return obj;
    }

    public virtual TObject Spawn(TEnum type, Vector3 position, Quaternion rotation)
    {
        if (!_pool.ContainsKey(type))
        {
            Debug.LogError($"Pool not found: {type}");
            return null;
        }

        if (_pool[type].Count == 0)
        {
            _pool[type].Enqueue(Create(type));
        }

        TObject obj = _pool[type].Dequeue();
        obj.transform.SetPositionAndRotation(position, rotation);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public virtual void Despawn(TEnum type, TObject obj)
    {
        obj.gameObject.SetActive(false);
        _pool[type].Enqueue(obj);
    }
}
