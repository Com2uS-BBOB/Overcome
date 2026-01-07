using System;
using System.Collections;
using UnityEngine;
using _02.Scripts.Player.Common;
using _02.Scripts.Player.Interfaces;
using _02.Scripts.Player.Data;

namespace _02.Scripts.Player.Combat
{
    // 용검 (기본 공격) 스킬 - 2타 콤보
    public class DragonSwordSkill : MonoBehaviour, ISkill
    {
        private const string CooldownKey = "Attack";
        private const int MaxCombo = 2;

        [Header("Duration Settings")]
        [SerializeField] private float _attack1Duration = 0.8f;   // Attack1 애니메이션 길이
        [SerializeField] private float _attack2Duration = 0.9f;   // Attack2 애니메이션 길이
        [SerializeField] private float _comboResetTime = 1.0f;    // 콤보 리셋 시간

        [Header("Hitbox Timing")]
        [SerializeField] private float _hitboxStartTime = 0.15f;  // 히트박스 활성화 시작
        [SerializeField] private float _hitboxDuration = 0.3f;    // 히트박스 활성화 시간

        [Header("References")]
        [SerializeField] private MeleeHitbox _hitbox;

        private PlayerStats _stats;
        private CooldownManager _cooldownManager;
        private bool _isAttacking;
        private int _comboCount;  // 0: 대기, 1: 1타 가능, 2: 2타 가능
        private Coroutine _comboResetCoroutine;

        // ISkill
        public string SkillName => "용검";
        public float Cooldown => _stats != null ? _stats.AttackCooldown : 0.5f;
        public bool CanUse => CanAttack;
        public bool CanAttack => !_isAttacking && _cooldownManager.IsReady(CooldownKey, Cooldown);
        public bool IsAttacking => _isAttacking;
        public int ComboCount => _comboCount;

        private float AttackDamage => _stats != null ? _stats.AttackDamage : 10f;
        private int CurrentComboStep => _comboCount + 1;  // 실행할 콤보 단계 (1 또는 2)
        private float CurrentAttackDuration => CurrentComboStep == 1 ? _attack1Duration : _attack2Duration;

        public event Action OnSkillUsed;
        public event Action OnAttackStarted;
        public event Action OnAttackEnded;
        public event Action<int> OnComboAttack;
        public event Action<IDamageable, float> OnEnemyHit;

        private void Awake()
        {
            _cooldownManager = new CooldownManager();
        }

        public void Initialize(PlayerStats stats) => _stats = stats;

        private void OnEnable() { if (_hitbox != null) _hitbox.OnHit += HandleHit; }
        private void OnDisable() { if (_hitbox != null) _hitbox.OnHit -= HandleHit; }

        public void Use() => Attack();

        /// <summary>
        /// 공격 실행 - 콤보 카운트에 따라 1타 또는 2타 실행
        /// </summary>
        public void Attack()
        {
            if (!CanAttack) return;

            // 콤보 리셋 타이머 취소
            if (_comboResetCoroutine != null)
                StopCoroutine(_comboResetCoroutine);

            StartCoroutine(AttackCoroutine());
        }

        private IEnumerator AttackCoroutine()
        {
            _isAttacking = true;
            _cooldownManager.Use(CooldownKey);

            int comboStep = CurrentComboStep;  // 현재 실행할 콤보 단계 (1 또는 2)
            float duration = CurrentAttackDuration;

            OnAttackStarted?.Invoke();
            OnSkillUsed?.Invoke();
            OnComboAttack?.Invoke(comboStep);

            // Phase 1: 히트박스 시작 전 대기
            yield return new WaitForSeconds(_hitboxStartTime);

            // Phase 2: 히트박스 활성화
            if (_hitbox != null) _hitbox.EnableHitDetection(AttackDamage);

            // Phase 3: 히트박스 종료까지 대기
            yield return new WaitForSeconds(_hitboxDuration);
            if (_hitbox != null) _hitbox.DisableHitDetection();

            // Phase 4: 공격 종료까지 대기
            float remainingTime = duration - _hitboxStartTime - _hitboxDuration;
            if (remainingTime > 0)
                yield return new WaitForSeconds(remainingTime);

            _isAttacking = false;

            // 콤보 카운트 증가 (최대치 도달 시 리셋)
            _comboCount = (_comboCount + 1) % MaxCombo;

            // 콤보 리셋 타이머 시작
            _comboResetCoroutine = StartCoroutine(ResetComboAfterDelay());

            OnAttackEnded?.Invoke();
        }

        private IEnumerator ResetComboAfterDelay()
        {
            yield return new WaitForSeconds(_comboResetTime);
            _comboCount = 0;
        }

        public void ResetCombo()
        {
            _comboCount = 0;
            if (_comboResetCoroutine != null)
                StopCoroutine(_comboResetCoroutine);
        }

        private void HandleHit(IDamageable target, float damage) => OnEnemyHit?.Invoke(target, damage);
    }
}