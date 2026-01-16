using System;
using UnityEngine;
using _02.Scripts.Player.Common;
using _02.Scripts.Player.Interfaces;

namespace _02.Scripts.Player.Combat
{
    // 검기
    public class CrescentProjectile : HitboxBase
    {
        [Header("VFX")]
        [SerializeField] private ProjectileVFXController _vfxController;

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
            _spawnHitEffect = true;  // 플레이어 투사체는 히트 이펙트 스폰

            EnableHitDetection(damage);

            if (_direction != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(_direction);

            // VFX 활성화
            _vfxController?.OnFired();
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

            // VFX 비활성화
            _vfxController?.OnReturned();

            if (_returnToPool != null)
                _returnToPool.Invoke(this);
            else
                Destroy(gameObject);
        }

        // 히트 이펙트는 HitboxBase.ProcessHit()에서 _spawnHitEffect 플래그로 처리
        protected override void OnHitSuccess(Collider other, IDamageable damageable) { }
    }
}