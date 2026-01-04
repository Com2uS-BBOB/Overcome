using System;
using System.Collections;
using UnityEngine;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 플레이어 전투 시스템
    /// 용검 (기본 공격) 처리
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Attack")]
        [SerializeField] private float _attackDamage = 10f;
        [SerializeField] private float _attackCooldown = 0.5f;
        [SerializeField] private float _attackDuration = 0.3f;

        [Header("References")]
        [SerializeField] private MeleeHitbox _hitbox;

        private float _lastAttackTime = -100f;
        private bool _isAttacking;

        public bool IsAttacking => _isAttacking;

        /// <summary>
        /// 공격 가능 여부
        /// </summary>
        public bool CanAttack => !_isAttacking && Time.time >= _lastAttackTime + _attackCooldown;

        // 이벤트
        public event Action OnAttackStarted;
        public event Action OnAttackEnded;
        public event Action<Collider, float> OnEnemyHit;

        private void OnEnable()
        {
            if (_hitbox != null)
            {
                _hitbox.OnHit += HandleHit;
            }
        }

        private void OnDisable()
        {
            if (_hitbox != null)
            {
                _hitbox.OnHit -= HandleHit;
            }
        }

        /// <summary>
        /// 공격 실행
        /// </summary>
        public void Attack()
        {
            if (_isAttacking)
            {
                Debug.Log("[Combat] 공격 불가 - 이미 공격 중");
                return;
            }

            if (Time.time < _lastAttackTime + _attackCooldown)
            {
                float remaining = (_lastAttackTime + _attackCooldown) - Time.time;
                Debug.Log($"[Combat] 공격 불가 - 쿨타임 {remaining:F2}초 남음");
                return;
            }

            StartCoroutine(AttackCoroutine());
        }

        private IEnumerator AttackCoroutine()
        {
            _isAttacking = true;
            _lastAttackTime = Time.time;
            OnAttackStarted?.Invoke();

            Debug.Log("[Combat] 용검 공격!");

            // 히트박스 활성화
            if (_hitbox != null)
            {
                _hitbox.EnableHitbox(_attackDamage);
            }

            // 공격 지속시간
            yield return new WaitForSeconds(_attackDuration);

            // 히트박스 비활성화
            if (_hitbox != null)
            {
                _hitbox.DisableHitbox();
            }

            _isAttacking = false;
            OnAttackEnded?.Invoke();

            Debug.Log("[Combat] 용검 공격 종료");
        }

        private void HandleHit(Collider target, float damage)
        {
            Debug.Log($"[Combat] 적 피격! 대상: {target.name}, 데미지: {damage}");
            OnEnemyHit?.Invoke(target, damage);
        }
    }
}