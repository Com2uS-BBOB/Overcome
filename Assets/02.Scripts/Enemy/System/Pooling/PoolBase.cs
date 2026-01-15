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

    // 프리팹과 초기 크기 등록
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

    // 객체 생성
    protected TObject Create(TEnum type)
    {
        TObject obj = Instantiate(_prefabs[type], transform);
        obj.gameObject.SetActive(false);
        return obj;
    }

    // 객체 스폰
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
        obj.gameObject.SetActive(false);
        obj.transform.SetPositionAndRotation(position, rotation);
        return obj;
    }

    // 객체 디스폰
    public virtual void Despawn(TEnum type, TObject obj)
    {
        obj.gameObject.SetActive(false);
        _pool[type].Enqueue(obj);
    }
}
