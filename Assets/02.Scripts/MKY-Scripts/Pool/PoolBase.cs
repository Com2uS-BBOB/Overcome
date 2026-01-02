using UnityEngine;
using System.Collections.Generic;


public abstract class PoolBase : MonoBehaviour
{
    protected virtual void Awake()
    {
        PoolManager.Instance.RegisterPool(this);
    }
}

public abstract class PoolBase<TEnum, TObject> : PoolBase
    where TEnum : System.Enum
    where TObject : Component
{
    [SerializeField] protected int _defaultSize = 10;

    protected Dictionary<TEnum, Queue<TObject>> _pool = new();
    protected Dictionary<TEnum, TObject> _prefabs = new();

    protected abstract void SetupPrefabs();

    protected virtual void Start()
    {
        SetupPrefabs();

        foreach (var pair in _prefabs)
        {
            Queue<TObject> queue = new Queue<TObject>();

            for (int i = 0; i < _defaultSize; i++)
            {
                TObject obj = Create(pair.Key);
                queue.Enqueue(obj);
            }

            _pool.Add(pair.Key, queue);
        }
    }

    protected TObject Create(TEnum type)
    {
        TObject obj = Instantiate(_prefabs[type], transform);
        obj.gameObject.SetActive(false);
        return obj;
    }

    public virtual TObject Spawn(TEnum type, Vector3 pos, Quaternion rot)
    {
        if (!_pool.ContainsKey(type))
        {
            Debug.LogError($"Pool not found: {type}");
            return null;
        }

        if (_pool[type].Count == 0)
            _pool[type].Enqueue(Create(type));

        TObject obj = _pool[type].Dequeue();
        obj.transform.SetPositionAndRotation(pos, rot);
        obj.gameObject.SetActive(true);
        return obj;
    }

    public virtual void Despawn(TEnum type, TObject obj)
    {
        obj.gameObject.SetActive(false);
        _pool[type].Enqueue(obj);
    }
}

