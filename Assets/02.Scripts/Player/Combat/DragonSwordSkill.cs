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

        [Header("Settings")]
        [SerializeField] private float _attackDuration = 0.3f;
        [SerializeField] private float _comboWindowStart = 0.2f;  // 콤보 입력 시작 시점
        [SerializeField] private float _comboResetTime = 0.8f;    // 콤보 리셋 시간

        [Header("References")]
        [SerializeField] private MeleeHitbox _hitbox;

        private PlayerStats _stats;
        private CooldownManager _cooldownManager;
        private bool _isAttacking;
        private int _comboCount;
        private bool _comboQueued;
        private Coroutine _comboResetCoroutine;

        // ISkill
        public string SkillName => "용검";
        public float Cooldown => _stats != null ? _stats.AttackCooldown : 0.5f;
        public bool CanUse => CanAttack;
        public bool CanAttack => !_isAttacking && _cooldownManager.IsReady(CooldownKey, Cooldown);
        public bool IsAttacking => _isAttacking;
        public int ComboCount => _comboCount;
        public bool CanQueueCombo => _isAttacking && _comboCount < MaxCombo && !_comboQueued;

        private float AttackDamage => _stats != null ? _stats.AttackDamage : 10f;

        public event Action OnSkillUsed;
        public event Action OnAttackStarted;
        public event Action OnAttackEnded;
        public event Action<int> OnComboAttack;  // 콤보 단계 전달
        public event Action<IDamageable, float> OnEnemyHit;

        private void Awake()
        {
            _cooldownManager = new CooldownManager();
        }

        public void Initialize(PlayerStats stats) => _stats = stats;

        private void OnEnable() { if (_hitbox != null) _hitbox.OnHit += HandleHit; }
        private void OnDisable() { if (_hitbox != null) _hitbox.OnHit -= HandleHit; }

        public void Use() => Attack();

        public void Attack()
        {
            // 콤보 입력 (공격 중일 때)
            if (_isAttacking && CanQueueCombo)
            {
                _comboQueued = true;
                return;
            }

            // 첫 공격
            if (!_isAttacking && _cooldownManager.IsReady(CooldownKey, Cooldown))
            {
                _comboCount = 1;
                StartCoroutine(AttackCoroutine());
            }
        }

        private IEnumerator AttackCoroutine()
        {
            _isAttacking = true;
            _comboQueued = false;
            _cooldownManager.Use(CooldownKey);

            // 콤보 리셋 타이머 취소
            if (_comboResetCoroutine != null)
                StopCoroutine(_comboResetCoroutine);

            OnAttackStarted?.Invoke();
            OnSkillUsed?.Invoke();
            OnComboAttack?.Invoke(_comboCount);

            if (_hitbox != null) _hitbox.EnableHitDetection(AttackDamage);

            // 콤보 입력 윈도우 대기
            yield return new WaitForSeconds(_comboWindowStart);

            // 나머지 공격 시간
            yield return new WaitForSeconds(_attackDuration - _comboWindowStart);

            if (_hitbox != null) _hitbox.DisableHitDetection();

            _isAttacking = false;

            // 콤보 입력이 있으면 다음 콤보 실행
            if (_comboQueued && _comboCount < MaxCombo)
            {
                _comboCount++;
                StartCoroutine(AttackCoroutine());
            }
            else
            {
                // 콤보 리셋 타이머 시작
                _comboResetCoroutine = StartCoroutine(ResetComboAfterDelay());
                OnAttackEnded?.Invoke();
            }
        }

        private IEnumerator ResetComboAfterDelay()
        {
            yield return new WaitForSeconds(_comboResetTime);
            _comboCount = 0;
        }

        public void ResetCombo()
        {
            _comboCount = 0;
            _comboQueued = false;
            if (_comboResetCoroutine != null)
                StopCoroutine(_comboResetCoroutine);
        }

        private void HandleHit(IDamageable target, float damage) => OnEnemyHit?.Invoke(target, damage);
    }
}