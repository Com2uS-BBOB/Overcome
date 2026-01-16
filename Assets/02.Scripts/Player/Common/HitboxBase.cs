using System;
using System.Collections.Generic;
using UnityEngine;
using _02.Scripts.Player.Combat;
using _02.Scripts.Player.Interfaces;

namespace _02.Scripts.Player.Common
{
    // 히트박스 공통 로직 (MeleeHitbox, CrescentProjectile 상속)
    [RequireComponent(typeof(Collider))]
    public abstract class HitboxBase : MonoBehaviour, IHitDetector
    {
        [SerializeField] protected bool _spawnHitEffect = false;

        protected HashSet<IDamageable> _hitTargets = new();
        protected float _damage;
        protected bool _isActive;

        public event Action<IDamageable, float> OnHit;

        protected virtual void Awake() => GetComponent<Collider>().isTrigger = true;

        public virtual void EnableHitDetection(float damage)
        {
            _damage = damage;
            _isActive = true;
            _hitTargets.Clear();
        }

        public virtual void DisableHitDetection()
        {
            _isActive = false;
            _hitTargets.Clear();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (_isActive) ProcessHit(other);
        }

        protected virtual void ProcessHit(Collider other)
        {
            if (ShouldIgnore(other)) return;

            var damageable = other.GetComponent<IDamageable>();

            if (damageable == null) return;
            if (_hitTargets.Contains(damageable)) return;  // IDamageable 기준 중복 체크

            // 히트 이펙트 스폰 (플레이어 히트박스만)
            if (_spawnHitEffect)
            {
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                HitEffectPool.Instance?.SpawnAt(hitPoint);
            }

            _hitTargets.Add(damageable);
            damageable.TakeDamage(_damage, GetOwner());
            OnHit?.Invoke(damageable, _damage);
            OnHitSuccess(other, damageable);
        }

        // 무시 대상 체크
        protected abstract bool ShouldIgnore(Collider other);

        // 히트 성공 후 추가 처리
        protected virtual void OnHitSuccess(Collider other, IDamageable damageable) { }

        // 공격자 반환
        protected virtual GameObject GetOwner() => transform.root.gameObject;
    }
}