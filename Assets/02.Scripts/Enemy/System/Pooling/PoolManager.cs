using UnityEngine;
using System;
using System.Collections.Generic;

[DefaultExecutionOrder(-100)]  // 우선순위로 실행되도록 설정

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

    // 풀 등록
    public void RegisterPool<T>(T pool) where T : PoolBase
    {
        Type type = typeof(T);

        if (_pools.ContainsKey(type))
            return;

        _pools.Add(type, pool);
    }

    // 풀 가져오기
    public T GetPool<T>() where T : PoolBase
    {
        if (_pools.TryGetValue(typeof(T), out PoolBase pool))
        {
            return pool as T;
        }
        return null;
    }
}