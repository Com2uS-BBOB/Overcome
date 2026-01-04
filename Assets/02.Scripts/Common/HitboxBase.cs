using System;
using System.Collections.Generic;
using UnityEngine;
using _02.Scripts.Interfaces;

namespace _02.Scripts.Common
{
    /// <summary>
    /// 히트박스 공통 로직 추상 클래스
    /// MeleeHitbox, CrescentProjectile 등이 상속
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public abstract class HitboxBase : MonoBehaviour, IHitDetector
    {
        protected HashSet<Collider> _hitTargets = new();
        protected float _damage;
        protected bool _isActive;

        public event Action<IDamageable, float> OnHit;

        protected virtual void Awake()
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        /// <summary>
        /// 히트 감지 활성화
        /// </summary>
        public virtual void EnableHitDetection(float damage)
        {
            _damage = damage;
            _isActive = true;
            _hitTargets.Clear();
        }

        /// <summary>
        /// 히트 감지 비활성화
        /// </summary>
        public virtual void DisableHitDetection()
        {
            _isActive = false;
            _hitTargets.Clear();
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!_isActive) return;

            ProcessHit(other);
        }

        /// <summary>
        /// 히트 처리
        /// </summary>
        protected virtual void ProcessHit(Collider other)
        {
            // 중복 히트 방지
            if (_hitTargets.Contains(other)) return;

            // 무시해야 할 대상 체크
            if (ShouldIgnore(other)) return;

            // IDamageable 컴포넌트 찾기
            var damageable = other.GetComponent<IDamageable>();
            if (damageable == null)
            {
                damageable = other.GetComponentInParent<IDamageable>();
            }

            if (damageable != null)
            {
                _hitTargets.Add(other);
                damageable.TakeDamage(_damage, GetOwner());
                OnHit?.Invoke(damageable, _damage);

                OnHitSuccess(other, damageable);
            }
        }

        /// <summary>
        /// 무시해야 할 대상인지 체크
        /// 하위 클래스에서 구현
        /// </summary>
        protected abstract bool ShouldIgnore(Collider other);

        /// <summary>
        /// 히트 성공 시 추가 처리
        /// 하위 클래스에서 오버라이드 가능
        /// </summary>
        protected virtual void OnHitSuccess(Collider other, IDamageable damageable) { }

        /// <summary>
        /// 공격자 GameObject 반환
        /// 하위 클래스에서 오버라이드
        /// </summary>
        protected virtual GameObject GetOwner()
        {
            return transform.root.gameObject;
        }
    }
}