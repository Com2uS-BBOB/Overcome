using System;
using System.Collections;
using UnityEngine;
using _02.Scripts.Player.Common;
using _02.Scripts.Player.Interfaces;
using _02.Scripts.Player.Data;

namespace _02.Scripts.Player.Combat
{
    // 용검 (기본 공격) 스킬 - 2타 콤보
    // 콤보 윈도우 + 콤보 유예 시스템
    // 콤보 윈도우 1타 (0.3 ~ 0.9(즉발)+0.1(유예)) + 2타(0.9초)
    public class DragonSwordSkill : MonoBehaviour, ISkill
    {
        private const string CooldownKey = "Attack";
        private const int MaxCombo = 3;

        [Header("Duration Settings")]
        [SerializeField] private float _attack1Duration = 0.8f;
        [SerializeField] private float _attack2Duration = 0.9f;
        [SerializeField] private float _attack3Duration = 1.0f;

        [Header("Combo Settings")]
        [SerializeField] private float _comboWindowStart = 0.3f;   // 콤보 윈도우 시작 (1타 중)
        [SerializeField] private float _comboGraceTime = 0.1f;     // 1타 끝난 후 유예 시간

        [Header("Hitbox Timing")]
        [SerializeField] private float _hitboxStartTime = 0.15f;
        [SerializeField] private float _hitboxDuration = 0.3f;

        [Header("References")]
        [SerializeField] private MeleeHitbox _hitbox;

        private PlayerStats _stats;
        private CooldownManager _cooldownManager;

        // 상태
        private bool _isAttacking;
        private int _comboStep;              // 현재 콤보 단계 (1 또는 2)
        private bool _comboQueued;           // 다음 콤보 예약됨
        private bool _inComboWindow;         // 콤보 윈도우 내
        private bool _inComboGrace;          // 콤보 유예 구간
        private bool _skipToNextCombo;       // 즉시 다음 콤보로 스킵
        private float _attackElapsedTime;    // 현재 공격 경과 시간
        private float _currentAttackDuration; // 현재 공격 전체 시간

        private Coroutine _attackCoroutine;
        private Coroutine _comboGraceCoroutine;

        // ISkill
        public string SkillName => "용검";
        public float Cooldown => _stats != null ? _stats.AttackCooldown : 0.5f;
        public bool CanUse => CanAttack;
        public bool IsAttacking => _isAttacking;
        public int ComboStep => _comboStep;

        // 공격 가능 조건: 공격 중 아님 + 쿨다운 OK + 유예 구간 아님
        public bool CanAttack => !_isAttacking && !_inComboGrace && _cooldownManager.IsReady(CooldownKey, Cooldown);

        // 콤보 큐잉 가능: 공격 중 + 콤보 윈도우 내 + 아직 예약 안됨 + 마지막 콤보 아님
        public bool CanQueueCombo => _isAttacking && _inComboWindow && !_comboQueued && _comboStep < MaxCombo;

        // 콤보 유예 가능: 유예 구간 내 + 다음 콤보 있음
        public bool CanComboGrace => _inComboGrace && _comboStep < MaxCombo;

        private float AttackDamage => _stats != null ? _stats.AttackDamage : 10f;
        private float GetAttackDuration(int step) => step switch
        {
            1 => _attack1Duration,
            2 => _attack2Duration,
            3 => _attack3Duration,
            _ => _attack1Duration
        };

        public event Action OnSkillUsed;
        public event Action OnAttackStarted;
        public event Action OnAttackEnded;
        public event Action<int> OnComboAttack;       // 콤보 단계 전달
        public event Action<int> OnComboQueued;       // 콤보 예약됨 (애니메이션 연동용)
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
        /// 공격 실행 - 상황에 따라 첫 공격/콤보 큐잉/콤보 유예 처리
        /// </summary>
        public void Attack()
        {
            // Case 1: 콤보 큐잉 (1타 진행 중 입력) → 즉시 스킵
            if (CanQueueCombo)
            {
                _comboQueued = true;
                _skipToNextCombo = true;  // 콤보 윈도우 내 입력 시 즉시 다음 콤보로
                OnComboQueued?.Invoke(_comboStep + 1);
                return;
            }

            // Case 2: 콤보 유예 (1타 끝난 직후 입력)
            if (CanComboGrace)
            {
                StopGraceTimer();
                ExecuteNextCombo();
                return;
            }

            // Case 3: 첫 공격
            if (CanAttack)
            {
                StopAllTimers();
                _comboStep = 1;
                _attackCoroutine = StartCoroutine(AttackCoroutine());
            }
        }

        private IEnumerator AttackCoroutine()
        {
            _isAttacking = true;
            _comboQueued = false;
            _inComboWindow = false;
            _inComboGrace = false;
            _skipToNextCombo = false;
            _cooldownManager.Use(CooldownKey);

            _currentAttackDuration = GetAttackDuration(_comboStep);
            _attackElapsedTime = 0f;

            float hitboxEndTime = _hitboxStartTime + _hitboxDuration;
            bool hitboxEnabled = false;

            OnAttackStarted?.Invoke();
            OnSkillUsed?.Invoke();
            OnComboAttack?.Invoke(_comboStep);

            // 매 프레임 경과 시간 추적 + 스킵 체크
            while (_attackElapsedTime < _currentAttackDuration)
            {
                _attackElapsedTime += Time.deltaTime;

                // Phase별 처리
                // 히트박스 시작
                if (!hitboxEnabled && _attackElapsedTime >= _hitboxStartTime)
                {
                    hitboxEnabled = true;
                    if (_hitbox != null) _hitbox.EnableHitDetection(AttackDamage);
                }

                // 콤보 윈도우 시작
                if (!_inComboWindow && _attackElapsedTime >= _comboWindowStart && _comboStep < MaxCombo)
                {
                    _inComboWindow = true;
                }

                // 히트박스 종료
                if (hitboxEnabled && _attackElapsedTime >= hitboxEndTime)
                {
                    hitboxEnabled = false;
                    if (_hitbox != null) _hitbox.DisableHitDetection();
                }

                // 스킵 체크: 콤보가 큐잉되고 스킵 플래그가 설정되면 즉시 종료
                if (_skipToNextCombo && _comboQueued)
                {
                    if (_hitbox != null) _hitbox.DisableHitDetection();
                    break;
                }

                yield return null;
            }

            // 공격 종료
            _isAttacking = false;
            _inComboWindow = false;
            _skipToNextCombo = false;

            // 콤보 처리
            if (_comboQueued && _comboStep < MaxCombo)
            {
                // 예약된 콤보 실행
                ExecuteNextCombo();
            }
            else if (_comboStep < MaxCombo)
            {
                // 콤보 유예 시작
                StartGraceTimer();
            }
            else
            {
                // 마지막 콤보 완료
                _comboStep = 0;
                OnAttackEnded?.Invoke();
            }
        }

        private void ExecuteNextCombo()
        {
            _comboStep++;
            _comboQueued = false;
            StartCoroutine(AttackCoroutine());
        }

        private void StartGraceTimer()
        {
            _inComboGrace = true;
            _comboGraceCoroutine = StartCoroutine(GraceTimerCoroutine());
        }

        private IEnumerator GraceTimerCoroutine()
        {
            yield return new WaitForSeconds(_comboGraceTime);

            // 유예 시간 종료
            _inComboGrace = false;
            _comboStep = 0;
            OnAttackEnded?.Invoke();
        }

        private void StopGraceTimer()
        {
            _inComboGrace = false;
            if (_comboGraceCoroutine != null)
            {
                StopCoroutine(_comboGraceCoroutine);
                _comboGraceCoroutine = null;
            }
        }

        private void StopAllTimers()
        {
            StopGraceTimer();
            if (_attackCoroutine != null)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
            }
        }

        private void HandleHit(IDamageable target, float damage) => OnEnemyHit?.Invoke(target, damage);
    }
}