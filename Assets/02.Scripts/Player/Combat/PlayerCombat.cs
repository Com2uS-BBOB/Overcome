using System;
using System.Collections;
using UnityEngine;
using _02.Scripts.Common;
using _02.Scripts.Interfaces;
using _02.Scripts.Player.Data;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 플레이어 전투 시스템
    /// 용검 (기본 공격) 처리, ISkill 인터페이스 구현
    /// </summary>
    public class PlayerCombat : MonoBehaviour, ISkill
    {
        private const string CooldownKey = "Attack";

        [Header("Settings (고정값)")]
        [SerializeField] private float _attackDuration = 0.3f;

        [Header("References")]
        [SerializeField] private MeleeHitbox _hitbox;

        private PlayerRuntimeStats _stats;
        private CooldownManager _cooldownManager;
        private bool _isAttacking;

        // ISkill 구현
        public string SkillName => "용검";
        public float Cooldown => _stats != null ? _stats.AttackCooldown : 0.5f;
        public bool CanUse => CanAttack;

        // 스탯 참조
        private float AttackDamage => _stats != null ? _stats.AttackDamage : 10f;

        public bool IsAttacking => _isAttacking;

        /// <summary>
        /// 공격 가능 여부
        /// </summary>
        public bool CanAttack => !_isAttacking && _cooldownManager.IsReady(CooldownKey, Cooldown);

        // 이벤트
        public event Action OnSkillUsed;
        public event Action OnAttackStarted;
        public event Action OnAttackEnded;
        public event Action<IDamageable, float> OnEnemyHit;

        private void Awake()
        {
            _stats = GetComponent<PlayerRuntimeStats>();
            _cooldownManager = new CooldownManager();

            if (_stats == null)
            {
                Debug.LogWarning("[PlayerCombat] RuntimeStats 없음 - 기본값 사용");
            }
        }

        /// <summary>
        /// 외부에서 Stats 주입 (PlayerController에서 호출)
        /// </summary>
        public void Initialize(PlayerRuntimeStats stats)
        {
            _stats = stats;
        }

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
        /// 스킬 사용 (ISkill 구현)
        /// </summary>
        public void Use()
        {
            Attack();
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

            if (!_cooldownManager.IsReady(CooldownKey, Cooldown))
            {
                float remaining = _cooldownManager.GetRemainingTime(CooldownKey, Cooldown);
                Debug.Log($"[Combat] 공격 불가 - 쿨타임 {remaining:F2}초 남음");
                return;
            }

            StartCoroutine(AttackCoroutine());
        }

        private IEnumerator AttackCoroutine()
        {
            _isAttacking = true;
            _cooldownManager.Use(CooldownKey);
            OnAttackStarted?.Invoke();
            OnSkillUsed?.Invoke();

            Debug.Log("[Combat] 용검 공격!");

            // 히트박스 활성화
            if (_hitbox != null)
            {
                _hitbox.EnableHitDetection(AttackDamage);
            }

            // 공격 지속시간
            yield return new WaitForSeconds(_attackDuration);

            // 히트박스 비활성화
            if (_hitbox != null)
            {
                _hitbox.DisableHitDetection();
            }

            _isAttacking = false;
            OnAttackEnded?.Invoke();

            Debug.Log("[Combat] 용검 공격 종료");
        }

        private void HandleHit(IDamageable target, float damage)
        {
            Debug.Log($"[Combat] 적 피격! 데미지: {damage}");
            OnEnemyHit?.Invoke(target, damage);
        }
    }
}