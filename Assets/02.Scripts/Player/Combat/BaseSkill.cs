using System;
using System.Collections;
using UnityEngine;
using _02.Scripts.Player.Data;
using _02.Scripts.Player.Interfaces;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 스킬 기본 클래스
    /// 콤보 시스템, 타이밍 관리, 이벤트 등 공통 로직 제공
    /// DragonSwordSkill, CrescentSkill 등이 상속
    /// </summary>
    public abstract class BaseSkill : MonoBehaviour, ISkill
    {
        [Header("Skill Data")]
        [SerializeField] protected SkillDataSet _skillDataSet;

        // 런타임 상태
        protected bool _isActive;
        protected int _comboStep;
        protected bool _comboQueued;
        protected bool _inComboGrace;
        protected bool _wasGroundedOnComboStart;
        protected float _elapsedTime;
        protected float _currentDuration;

        protected Coroutine _skillCoroutine;
        protected Coroutine _graceCoroutine;

        // === ISkill 구현 ===
        public abstract string SkillName { get; }
        public virtual float Cooldown => 0f;
        public abstract bool CanUse { get; }
        public void Use() => StartSkill(GetIsGrounded());

        // === Properties ===
        public bool IsActive => _isActive;
        public int ComboStep => _comboStep;
        public bool IsInComboGrace => _inComboGrace;
        public bool WasGroundedOnComboStart => _wasGroundedOnComboStart;
        public SkillDataSet SkillDataSet => _skillDataSet;

        /// <summary>
        /// 현재 SkillData (콤보 시작 시점 기준)
        /// </summary>
        public SkillData CurrentSkillData =>
            _skillDataSet?.GetSkillDataByComboStart(_wasGroundedOnComboStart);

        /// <summary>
        /// 최대 콤보 수
        /// </summary>
        public int MaxCombo => CurrentSkillData?.MaxCombo ?? 1;

        /// <summary>
        /// 콤보 진행 가능 여부
        /// </summary>
        public bool CanContinueCombo => _comboStep < MaxCombo;

        /// <summary>
        /// 콤보 유예 가능 여부
        /// </summary>
        public bool CanComboGrace => _inComboGrace && CanContinueCombo;

        // === Events ===
        public event Action OnSkillUsed;
        public event Action OnSkillEnded;
        public event Action<int> OnComboAttack;
        public event Action<int> OnComboQueued;
        public event Action<IDamageable, float> OnEnemyHit;

        // === Abstract/Virtual Methods ===

        /// <summary>
        /// 현재 지상 여부 (Movement 참조용)
        /// </summary>
        protected abstract bool GetIsGrounded();

        /// <summary>
        /// 스킬 데미지 반환
        /// </summary>
        protected abstract float GetDamage();

        /// <summary>
        /// 스킬 시작 시 호출 (쿨다운 사용, 프로젝타일 발사 등)
        /// </summary>
        protected virtual void OnSkillStart() { }

        /// <summary>
        /// 히트박스 활성화 (MeleeHitbox 사용 시)
        /// </summary>
        protected virtual void EnableHitbox() { }

        /// <summary>
        /// 히트박스 비활성화 (MeleeHitbox 사용 시)
        /// </summary>
        protected virtual void DisableHitbox() { }

        /// <summary>
        /// 콤보 유예 시간 (SkillData에서 가져오거나 Override)
        /// </summary>
        protected virtual float GetComboGraceTime()
        {
            return CurrentSkillData?.ComboGraceTime ?? 0.1f;
        }

        /// <summary>
        /// 현재 콤보의 지속 시간
        /// </summary>
        protected virtual float GetCurrentDuration()
        {
            return CurrentSkillData?.GetDuration(_comboStep) ?? 0.8f;
        }

        /// <summary>
        /// 히트박스 시작 시간 (정규화 시간 → 실제 시간)
        /// </summary>
        protected virtual float GetHitboxStartTime()
        {
            float normalizedStart = CurrentSkillData?.GetHitboxStart(_comboStep) ?? 0.15f;
            return _currentDuration * normalizedStart;
        }

        /// <summary>
        /// 히트박스 종료 시간 (정규화 시간 → 실제 시간)
        /// </summary>
        protected virtual float GetHitboxEndTime()
        {
            float normalizedEnd = CurrentSkillData?.GetHitboxEnd(_comboStep) ?? 0.45f;
            return _currentDuration * normalizedEnd;
        }

        // === Core Methods ===

        /// <summary>
        /// 스킬 시작 (첫 공격)
        /// </summary>
        public virtual void StartSkill(bool isGrounded)
        {
            if (!CanUse) return;

            StopAllTimers();
            _comboStep = 1;
            _wasGroundedOnComboStart = isGrounded;
            _skillCoroutine = StartCoroutine(SkillCoroutine());
        }

        /// <summary>
        /// 콤보 큐잉 (CombatStateHandler에서 호출)
        /// </summary>
        public virtual void QueueCombo()
        {
            if (_isActive && !_comboQueued && CanContinueCombo)
            {
                _comboQueued = true;
                OnComboQueued?.Invoke(_comboStep + 1);
            }
        }

        /// <summary>
        /// 콤보 유예 중 다음 콤보 실행
        /// </summary>
        public virtual void ExecuteGraceCombo()
        {
            if (CanComboGrace)
            {
                StopGraceTimer();
                ExecuteNextCombo();
            }
        }

        /// <summary>
        /// 콤보 리셋 (스킬 전환 시 호출)
        /// </summary>
        public virtual void ResetCombo()
        {
            StopAllTimers();
            _comboStep = 0;
            _isActive = false;
            _comboQueued = false;
            _inComboGrace = false;
            DisableHitbox();
        }

        // === Coroutine Logic ===

        protected virtual IEnumerator SkillCoroutine()
        {
            _isActive = true;
            _comboQueued = false;
            _inComboGrace = false;
            _elapsedTime = 0f;

            _currentDuration = GetCurrentDuration();
            float hitboxStartTime = GetHitboxStartTime();
            float hitboxEndTime = GetHitboxEndTime();
            bool hitboxEnabled = false;

            // 스킬 시작 콜백
            OnSkillStart();
            OnSkillUsed?.Invoke();
            OnComboAttack?.Invoke(_comboStep);

            // 매 프레임 경과 시간 추적
            while (_elapsedTime < _currentDuration)
            {
                _elapsedTime += Time.deltaTime;

                // 히트박스 활성화
                if (!hitboxEnabled && _elapsedTime >= hitboxStartTime)
                {
                    hitboxEnabled = true;
                    EnableHitbox();
                }

                // 히트박스 비활성화
                if (hitboxEnabled && _elapsedTime >= hitboxEndTime)
                {
                    hitboxEnabled = false;
                    DisableHitbox();
                }

                // 콤보 큐잉 시 즉시 다음 콤보로
                if (_comboQueued)
                {
                    DisableHitbox();
                    break;
                }

                yield return null;
            }

            // 스킬 종료
            _isActive = false;

            // 콤보 처리
            if (_comboQueued && CanContinueCombo)
            {
                ExecuteNextCombo();
            }
            else if (CanContinueCombo)
            {
                StartGraceTimer();
            }
            else
            {
                // 마지막 콤보 완료
                _comboStep = 0;
                OnSkillEnded?.Invoke();
            }
        }

        protected void ExecuteNextCombo()
        {
            _comboStep++;
            _comboQueued = false;
            _skillCoroutine = StartCoroutine(SkillCoroutine());
        }

        // === Grace Timer ===

        protected void StartGraceTimer()
        {
            _inComboGrace = true;
            _graceCoroutine = StartCoroutine(GraceTimerCoroutine());
        }

        protected IEnumerator GraceTimerCoroutine()
        {
            yield return new WaitForSeconds(GetComboGraceTime());

            _inComboGrace = false;
            _comboStep = 0;
            OnSkillEnded?.Invoke();
        }

        protected void StopGraceTimer()
        {
            _inComboGrace = false;
            if (_graceCoroutine != null)
            {
                StopCoroutine(_graceCoroutine);
                _graceCoroutine = null;
            }
        }

        protected void StopAllTimers()
        {
            StopGraceTimer();
            if (_skillCoroutine != null)
            {
                StopCoroutine(_skillCoroutine);
                _skillCoroutine = null;
            }
        }

        // === Event Helpers ===

        protected void InvokeEnemyHit(IDamageable target, float damage)
        {
            OnEnemyHit?.Invoke(target, damage);
        }
    }
}