using UnityEngine;
using System;
using System.Collections.Generic;

[DefaultExecutionOrder(-100)]
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private Dictionary<Type, PoolBase> _pools = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterPool<T>(T pool) where T : PoolBase
    {
        Type type = typeof(T);

        if (_pools.ContainsKey(type))
            return;

        _pools.Add(type, pool);
    }

    public T GetPool<T>() where T : PoolBase
    {
        if (_pools.TryGetValue(typeof(T), out PoolBase pool))
            return pool as T;

        Debug.LogError($"[PoolManager] Pool not found: {typeof(T)}");
        return null;
    }
}