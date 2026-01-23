using System;
using UnityEngine;
using _02.Scripts.Player.StateMachine;
using _02.Scripts.Player.StateMachine.States;

namespace _02.Scripts.Player.Animation
{
    // 플레이어 애니메이션 관리 (1-Layer Sub-State Machine 구조)
    // Base Layer: Locomotion, Jump, GroundCombat, AirCombat
    public class PlayerAnimatorController : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private RootMotionProxy _rootMotionProxy;
        [SerializeField] private AnimationEventProxy _animEventProxy;

        public event Action<Vector3> OnRootMotionUpdate;
        public event Action OnCrescentFireEvent;

        // Animation Event 전달용 이벤트
        public event Action OnAttackHitboxEnable;
        public event Action OnAttackHitboxDisable;
        public event Action OnAttackEnd;
        public event Action OnCancelWindowEnter;
        public event Action OnCancelWindowExit;

        // Overdrive 이벤트
        public event Action OnOverdriveReady;   // VFX 시작 타이밍
        public event Action OnOverdriveEnd;     // 애니메이션 종료

        // 발소리 이벤트
        public event Action OnFootstep;

        // Animator 파라미터 해시 - Locomotion
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int DoubleJumpHash = Animator.StringToHash("DoubleJump");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");

        // Animator 파라미터 해시 - Combat
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int AttackComboCountHash = Animator.StringToHash("AttackComboCount");
        private static readonly int DashAttackHash = Animator.StringToHash("DashAttack");
        private static readonly int CrescentHash = Animator.StringToHash("Crescent");
        private static readonly int CrescentComboCountHash = Animator.StringToHash("CrescentComboCount");

        // Animator 파라미터 해시 - Hit/Guard
        private static readonly int HitHash = Animator.StringToHash("Hit");
        private static readonly int GuardHash = Animator.StringToHash("Guard");
        private static readonly int IsGuardingHash = Animator.StringToHash("IsGuarding");
        private static readonly int GuardBlockHash = Animator.StringToHash("GuardBlock");
        private static readonly int GuardBreakHash = Animator.StringToHash("GuardBreak");

        // Animator 파라미터 해시 - Overdrive
        private static readonly int OverdriveActivationHash = Animator.StringToHash("OverdriveActivation");

        private PlayerStateMachine _stateMachine;

        public void Initialize(PlayerStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
            _stateMachine.OnStateChanged += HandleStateChanged;

            if (_rootMotionProxy != null)
                _rootMotionProxy.OnRootMotionUpdate += HandleRootMotionUpdate;

            if (_animEventProxy != null)
            {
                _animEventProxy.OnCrescentFire += HandleCrescentFire;
                _animEventProxy.OnAttackHitboxEnable += HandleAttackHitboxEnable;
                _animEventProxy.OnAttackHitboxDisable += HandleAttackHitboxDisable;
                _animEventProxy.OnAttackEnd += HandleAttackEnd;
                _animEventProxy.OnCancelWindowEnter += HandleCancelWindowEnter;
                _animEventProxy.OnCancelWindowExit += HandleCancelWindowExit;
                _animEventProxy.OnOverdriveReady += HandleOverdriveReady;
                _animEventProxy.OnOverdriveEnd += HandleOverdriveEnd;
                _animEventProxy.OnFootstep += HandleFootstep;
            }
            else
            {
                Debug.LogError("[PlayerAnimatorController] AnimEventProxy is NULL! Animation events will not work.");
            }
        }

        #region Animation Event Handlers

        private void HandleCrescentFire() => OnCrescentFireEvent?.Invoke();
        private void HandleAttackHitboxEnable() => OnAttackHitboxEnable?.Invoke();
        private void HandleAttackHitboxDisable() => OnAttackHitboxDisable?.Invoke();
        private void HandleAttackEnd() => OnAttackEnd?.Invoke();
        private void HandleCancelWindowEnter() => OnCancelWindowEnter?.Invoke();
        private void HandleCancelWindowExit() => OnCancelWindowExit?.Invoke();
        private void HandleOverdriveReady() => OnOverdriveReady?.Invoke();
        private void HandleOverdriveEnd() => OnOverdriveEnd?.Invoke();
        private void HandleFootstep() => OnFootstep?.Invoke();

        #endregion

        #region Root Motion

        public void EnableRootMotion() => _rootMotionProxy?.EnableRootMotion();
        public void DisableRootMotion() => _rootMotionProxy?.DisableRootMotion();

        private void HandleRootMotionUpdate(Vector3 deltaPosition)
            => OnRootMotionUpdate?.Invoke(deltaPosition);

        #endregion


        #region Locomotion

        /// <summary>
        /// 이동 상태 설정 (Idle ↔ Run 전환)
        /// </summary>
        public void SetMoving(bool isMoving) => _animator.SetBool(IsMovingHash, isMoving);

        /// <summary>
        /// 1단 점프 애니메이션 트리거
        /// </summary>
        public void PlayJump() => _animator.SetTrigger(JumpHash);

        /// <summary>
        /// 2단 점프 애니메이션 트리거
        /// </summary>
        public void PlayDoubleJump() => _animator.SetTrigger(DoubleJumpHash);

        /// <summary>
        /// 지면 상태 동기화
        /// </summary>
        public void SetGrounded(bool isGrounded) => _animator.SetBool(IsGroundedHash, isGrounded);

        #endregion

        #region Combat

        private void HandleStateChanged(Type previousState, Type newState)
        {
            // DragonSwordState: 애니메이션은 OnComboAttack 이벤트에서 처리
            // DashAttackState / AirDashAttackState: 여기서 직접 처리
            if (newState == typeof(DashAttackState) || newState == typeof(AirDashAttackState))
            {
                PlayDashAttack();
            }
            // 전투 상태에서 비전투 상태로 전환될 때만 EndCombat() 호출
            // 착지 시 공격 입력이 씹히는 문제 방지
            else if (IsCombatState(previousState) && IsNonCombatState(newState))
            {
                EndCombat();
            }
        }

        private bool IsCombatState(Type state) =>
            state == typeof(DragonSwordState) || state == typeof(AirDragonSwordState) ||
            state == typeof(CrescentState) || state == typeof(AirCrescentState) ||
            state == typeof(DashAttackState) || state == typeof(AirDashAttackState);

        private bool IsNonCombatState(Type state) =>
            state == typeof(IdleState) || state == typeof(MoveState) ||
            state == typeof(FallState) || state == typeof(JumpState);

        /// <summary>
        /// 용검 공격
        /// </summary>
        public void PlayAttack(int comboStep, bool isGrounded)
        {
            // 기존 트리거 리셋 (이전 상태의 잔여 트리거 제거)
            _animator.ResetTrigger(AttackHash);
            // 공격 시작 시점에 IsGrounded 즉시 동기화
            _animator.SetBool(IsGroundedHash, isGrounded);
            _animator.SetInteger(AttackComboCountHash, comboStep);
            _animator.SetTrigger(AttackHash);
        }

        /// <summary>
        /// 질풍참
        /// </summary>
        public void PlayDashAttack()
        {
            _animator.SetTrigger(DashAttackHash);
        }

        /// <summary>
        /// 크레센트
        /// </summary>
        public void PlayCrescent(int comboStep, bool isGrounded)
        {
            // 기존 트리거 리셋 (이전 상태의 잔여 트리거 제거)
            _animator.ResetTrigger(CrescentHash);
            // 공격 시작 시점에 IsGrounded 즉시 동기화
            _animator.SetBool(IsGroundedHash, isGrounded);
            _animator.SetInteger(CrescentComboCountHash, comboStep);
            _animator.SetTrigger(CrescentHash);
        }

        /// <summary>
        /// 파라미터 리셋
        /// </summary>
        public void EndCombat()
        {
            _animator.ResetTrigger(AttackHash);
            _animator.ResetTrigger(CrescentHash);
            _animator.ResetTrigger(DashAttackHash);
            _animator.SetInteger(AttackComboCountHash, 0);
            _animator.SetInteger(CrescentComboCountHash, 0);
        }

        #endregion

        #region Hit/Guard

        /// <summary>
        /// 피격 애니메이션 재생
        /// </summary>
        public void PlayHit()
        {
            _animator.ResetTrigger(HitHash);
            _animator.SetTrigger(HitHash);
        }

        /// <summary>
        /// Hit 종료 후 Locomotion 상태 강제 동기화
        /// StateMachine과 Animator 간 불일치 방지
        /// </summary>
        public void SyncLocomotionState(bool isGrounded, bool isMoving)
        {
            // 모든 전투 관련 트리거 리셋 (잔여 트리거 제거)
            _animator.ResetTrigger(HitHash);
            _animator.ResetTrigger(AttackHash);
            _animator.ResetTrigger(CrescentHash);
            _animator.ResetTrigger(DashAttackHash);

            // Locomotion 파라미터 강제 동기화
            _animator.SetBool(IsGroundedHash, isGrounded);
            _animator.SetBool(IsMovingHash, isMoving);

            // 콤보 카운트 리셋
            _animator.SetInteger(AttackComboCountHash, 0);
            _animator.SetInteger(CrescentComboCountHash, 0);
        }

        /// <summary>
        /// 현재 Animator의 IsGrounded 상태 반환 (디버깅용)
        /// </summary>
        public bool GetAnimatorGrounded() => _animator.GetBool(IsGroundedHash);

        /// <summary>
        /// 가드 시작 (Trigger + Bool)
        /// </summary>
        public void PlayGuard()
        {
            _animator.SetTrigger(GuardHash);
            _animator.SetBool(IsGuardingHash, true);
        }

        /// <summary>
        /// 가드 종료 (Bool만 false)
        /// </summary>
        public void StopGuard()
        {
            _animator.SetBool(IsGuardingHash, false);
        }

        /// <summary>
        /// 가드 포즈 설정 (Legacy)
        /// </summary>
        public void SetGuarding(bool isGuarding)
        {
            _animator.SetBool(IsGuardingHash, isGuarding);
        }

        /// <summary>
        /// 가드 성공 리액션 (일반/저스트 공용)
        /// </summary>
        public void PlayGuardBlock()
        {
            _animator.ResetTrigger(GuardBlockHash);
            _animator.SetTrigger(GuardBlockHash);
            _animator.SetBool(IsGuardingHash, true);  // 가드 상태 유지 보장
        }

        /// <summary>
        /// 가드 브레이크 애니메이션 재생
        /// </summary>
        public void PlayGuardBreak()
        {
            _animator.SetBool(IsGuardingHash, false);
            _animator.ResetTrigger(GuardBreakHash);
            _animator.SetTrigger(GuardBreakHash);
        }

        #endregion

        #region Overdrive

        /// <summary>
        /// Overdrive 진입 애니메이션 재생
        /// </summary>
        public void PlayOverdriveActivation(bool isGrounded)
        {
            _animator.SetBool(IsGroundedHash, isGrounded);
            _animator.ResetTrigger(OverdriveActivationHash);
            _animator.SetTrigger(OverdriveActivationHash);
        }

        #endregion

        private void OnDestroy()
        {
            if (_stateMachine != null)
                _stateMachine.OnStateChanged -= HandleStateChanged;

            if (_rootMotionProxy != null)
                _rootMotionProxy.OnRootMotionUpdate -= HandleRootMotionUpdate;

            if (_animEventProxy != null)
            {
                _animEventProxy.OnCrescentFire -= HandleCrescentFire;
                _animEventProxy.OnAttackHitboxEnable -= HandleAttackHitboxEnable;
                _animEventProxy.OnAttackHitboxDisable -= HandleAttackHitboxDisable;
                _animEventProxy.OnAttackEnd -= HandleAttackEnd;
                _animEventProxy.OnCancelWindowEnter -= HandleCancelWindowEnter;
                _animEventProxy.OnCancelWindowExit -= HandleCancelWindowExit;
                _animEventProxy.OnOverdriveReady -= HandleOverdriveReady;
                _animEventProxy.OnOverdriveEnd -= HandleOverdriveEnd;
                _animEventProxy.OnFootstep -= HandleFootstep;
            }
        }
    }
}