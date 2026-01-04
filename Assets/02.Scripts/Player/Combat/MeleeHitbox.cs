using System;
using System.Collections.Generic;
using UnityEngine;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 근접 공격 히트박스
    /// Trigger Collider로 피격 판정
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class MeleeHitbox : MonoBehaviour
    {
        private float _damage;
        private HashSet<Collider> _hitTargets = new HashSet<Collider>();
        private Collider _collider;

        // 피격 이벤트
        public event Action<Collider, float> OnHit;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;

            // 기본 비활성화
            DisableHitbox();
        }

        /// <summary>
        /// 히트박스 활성화
        /// </summary>
        public void EnableHitbox(float damage)
        {
            _damage = damage;
            _hitTargets.Clear();
            _collider.enabled = true;

            Debug.Log($"[MeleeHitbox] 활성화 - 데미지: {damage}");
        }

        /// <summary>
        /// 히트박스 비활성화
        /// </summary>
        public void DisableHitbox()
        {
            _collider.enabled = false;
            _hitTargets.Clear();

            Debug.Log("[MeleeHitbox] 비활성화");
        }

        private void OnTriggerEnter(Collider other)
        {
            // 이미 피격된 대상 무시 (중복 피격 방지)
            if (_hitTargets.Contains(other))
            {
                return;
            }

            // 자기 자신 무시
            if (other.transform.root == transform.root)
            {
                return;
            }

            _hitTargets.Add(other);

            Debug.Log($"[MeleeHitbox] 피격! 대상: {other.name}, 데미지: {_damage}");

            // 피격 이벤트 발행
            OnHit?.Invoke(other, _damage);

            // TODO: IDamageable 인터페이스로 데미지 전달
            // var damageable = other.GetComponent<IDamageable>();
            // damageable?.TakeDamage(_damage, ...);
        }

        private void OnDrawGizmos()
        {
            // 히트박스 시각화
            var col = GetComponent<Collider>();
            if (col == null) return;

            Gizmos.color = col.enabled ? new Color(1, 0, 0, 0.5f) : new Color(0, 1, 0, 0.3f);

            if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
            }
        }
    }
}