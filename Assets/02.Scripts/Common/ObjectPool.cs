using System;
using System.Collections.Generic;
using UnityEngine;

namespace _02.Scripts.Common
{
    /// <summary>
    /// 제너릭 오브젝트 풀
    /// 프로젝타일 등 자주 생성/파괴되는 객체 재사용
    /// </summary>
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

            if (initialSize > 0)
            {
                Prewarm(initialSize);
            }
        }

        /// <summary>
        /// 풀에서 객체 가져오기
        /// </summary>
        public T Get()
        {
            T item;

            if (_pool.Count > 0)
            {
                item = _pool.Dequeue();
            }
            else
            {
                item = UnityEngine.Object.Instantiate(_prefab, _parent);
            }

            item.gameObject.SetActive(true);
            _activeObjects.Add(item);

            return item;
        }

        /// <summary>
        /// 객체를 풀에 반환
        /// </summary>
        public void Return(T item)
        {
            if (item == null) return;
            if (!_activeObjects.Contains(item)) return;

            _activeObjects.Remove(item);
            item.gameObject.SetActive(false);
            _pool.Enqueue(item);
        }

        /// <summary>
        /// 풀 미리 채우기
        /// </summary>
        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var item = UnityEngine.Object.Instantiate(_prefab, _parent);
                item.gameObject.SetActive(false);
                _pool.Enqueue(item);
            }
        }

        /// <summary>
        /// 모든 활성 객체 반환
        /// </summary>
        public void ReturnAll()
        {
            var activeList = new List<T>(_activeObjects);
            foreach (var item in activeList)
            {
                Return(item);
            }
        }

        /// <summary>
        /// 풀 초기화 (모든 객체 파괴)
        /// </summary>
        public void Clear()
        {
            foreach (var item in _activeObjects)
            {
                if (item != null)
                {
                    UnityEngine.Object.Destroy(item.gameObject);
                }
            }
            _activeObjects.Clear();

            while (_pool.Count > 0)
            {
                var item = _pool.Dequeue();
                if (item != null)
                {
                    UnityEngine.Object.Destroy(item.gameObject);
                }
            }
        }
    }
}