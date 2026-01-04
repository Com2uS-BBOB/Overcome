using System.Collections.Generic;
using UnityEngine;

namespace _02.Scripts.Common
{
    // 제너릭 오브젝트 풀
    public class ObjectPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _pool = new();
        private readonly HashSet<T> _activeObjects = new();

        public int ActiveCount => _activeObjects.Count;
        public int PooledCount => _pool.Count;

        public ObjectPool(T prefab, Transform parent = null, int initialSize = 0)
        {
            _prefab = prefab;
            _parent = parent;
            if (initialSize > 0) Prewarm(initialSize);
        }

        // 풀에서 꺼내기
        public T Get()
        {
            T item = _pool.Count > 0
                ? _pool.Dequeue()
                : Object.Instantiate(_prefab, _parent);

            item.gameObject.SetActive(true);
            _activeObjects.Add(item);
            return item;
        }

        // 풀에 반환
        public void Return(T item)
        {
            if (item == null || !_activeObjects.Contains(item)) return;

            _activeObjects.Remove(item);
            item.gameObject.SetActive(false);
            _pool.Enqueue(item);
        }

        // 미리 생성
        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var item = Object.Instantiate(_prefab, _parent);
                item.gameObject.SetActive(false);
                _pool.Enqueue(item);
            }
        }

        // 모든 활성 객체 반환
        public void ReturnAll()
        {
            foreach (var item in new List<T>(_activeObjects))
                Return(item);
        }

        // 풀 정리
        public void Clear()
        {
            foreach (var item in _activeObjects)
                if (item != null) Object.Destroy(item.gameObject);
            _activeObjects.Clear();

            while (_pool.Count > 0)
            {
                var item = _pool.Dequeue();
                if (item != null) Object.Destroy(item.gameObject);
            }
        }
    }
}