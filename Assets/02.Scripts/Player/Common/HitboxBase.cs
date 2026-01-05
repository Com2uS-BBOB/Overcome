using System;
using System.Collections.Generic;
using UnityEngine;
using _02.Scripts.Player.Interfaces;

namespace _02.Scripts.Player.Common
{
    // 히트박스 공통 로직 (MeleeHitbox, CrescentProjectile 상속)
    [RequireComponent(typeof(Collider))]
    public abstract class HitboxBase : MonoBehaviour, IHitDetector
    {
        protected HashSet<Collider> _hitTargets = new();
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
            if (_hitTargets.Contains(other)) return;
            if (ShouldIgnore(other)) return;

            var damageable = other.GetComponent<IDamageable>()
                          ?? other.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                _hitTargets.Add(other);
                damageable.TakeDamage(_damage, GetOwner());
                OnHit?.Invoke(damageable, _damage);
                OnHitSuccess(other, damageable);
            }
        }

        // 무시 대상 체크
        protected abstract bool ShouldIgnore(Collider other);

        // 히트 성공 후 추가 처리
        protected virtual void OnHitSuccess(Collider other, IDamageable damageable) { }

        // 공격자 반환
        protected virtual GameObject GetOwner() => transform.root.gameObject;
    }
}