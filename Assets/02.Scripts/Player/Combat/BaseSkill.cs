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

        /// <summary>
        /// 스킬 코루틴 (Animation Event 기반)
        /// 히트박스/스킬종료는 Animation Event에서 처리
        /// </summary>
        protected virtual IEnumerator SkillCoroutine()
        {
            _isActive = true;
            _comboQueued = false;
            _inComboGrace = false;

            // 스킬 시작 콜백
            OnSkillStart();
            OnSkillUsed?.Invoke();
            OnComboAttack?.Invoke(_comboStep);

            // Animation Event에서 OnAnimEventSkillEnd()가 호출될 때까지 대기
            while (_isActive)
            {
                // 콤보 큐잉 시 즉시 다음 콤보로
                if (_comboQueued && CanContinueCombo)
                {
                    DisableHitbox();
                    ExecuteNextCombo();
                    yield break;
                }

                yield return null;
            }

            // 스킬 종료 처리 (OnAnimEventSkillEnd에서 _isActive = false 설정됨)
            HandleSkillEnd();
        }

        /// <summary>
        /// 스킬 종료 처리
        /// </summary>
        private void HandleSkillEnd()
        {
            Debug.Log($"[BaseSkill] HandleSkillEnd - comboStep={_comboStep}, MaxCombo={MaxCombo}, CanContinueCombo={CanContinueCombo}");
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
                Debug.Log("[BaseSkill] Last combo finished, invoking OnSkillEnded");
                _comboStep = 0;
                OnSkillEnded?.Invoke();
            }
        }

        // === Animation Event Handlers ===

        /// <summary>
        /// Animation Event: 히트박스 활성화
        /// </summary>
        public void OnAnimEventEnableHitbox()
        {
            if (_isActive)
                EnableHitbox();
        }

        /// <summary>
        /// Animation Event: 히트박스 비활성화
        /// </summary>
        public void OnAnimEventDisableHitbox()
        {
            DisableHitbox();
        }

        /// <summary>
        /// Animation Event: 스킬 종료
        /// </summary>
        public void OnAnimEventSkillEnd()
        {
            Debug.Log($"[BaseSkill] OnAnimEventSkillEnd called, _isActive={_isActive}");
            if (!_isActive) return;

            _isActive = false;
            DisableHitbox();  // 안전장치
            Debug.Log("[BaseSkill] _isActive set to false");
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