using System;
using UnityEngine;
using _02.Scripts.Player.Common;

namespace _02.Scripts.Player.Combat
{
    // 크레센트 프로젝타일 (관통, 풀링 지원)
    public class CrescentProjectile : HitboxBase
    {
        private float _speed;
        private float _maxDistance;
        private Vector3 _startPosition;
        private Vector3 _direction;
        private GameObject _owner;
        private Action<CrescentProjectile> _returnToPool;

        public void Initialize(float damage, float speed, float maxDistance, Vector3 direction, GameObject owner, Action<CrescentProjectile> returnCallback = null)
        {
            _speed = speed;
            _maxDistance = maxDistance;
            _direction = direction.normalized;
            _startPosition = transform.position;
            _owner = owner;
            _returnToPool = returnCallback;

            EnableHitDetection(damage);

            if (_direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(_direction);
        }

        private void Update()
        {
            if (!_isActive) return;

            transform.position += _direction * _speed * Time.deltaTime;

            if (Vector3.Distance(_startPosition, transform.position) >= _maxDistance)
                ReturnOrDestroy();
        }

        protected override bool ShouldIgnore(Collider other) => other.CompareTag(GameTags.Player);

        protected override GameObject GetOwner() => _owner;

        private void ReturnOrDestroy()
        {
            DisableHitDetection();

            if (_returnToPool != null)
                _returnToPool.Invoke(this);
            else
                Destroy(gameObject);
        }
    }
}