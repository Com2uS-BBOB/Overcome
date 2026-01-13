using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    /// <summary>
    /// 전투 상태 기반 클래스 (지상/공중 전투 공통 로직)
    /// </summary>
    public abstract class CombatStateBase : PlayerStateBase
    {
        protected abstract bool IsAirCombat { get; }
        protected abstract bool UseRootMotion { get; }

        // 공중 부유감 설정 (서브클래스에서 오버라이드 가능)
        protected virtual float AirGravityScale => 0.3f;
        protected virtual float ComboLiftForce => 1.5f;

        // 공격 중 자유 이동/회전 설정
        protected virtual bool AllowMovementDuringAttack => true;   // 이동 허용
        protected virtual bool AllowRotationDuringAttack => true;   // 카메라 추적 회전 허용
        protected virtual float CombatMoveSpeedMultiplier => 0.3f;  // 이동 속도 30%

        protected CombatStateBase(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            // 전투 중 이동 애니메이션 비활성화
            Controller.PlayerAnimatorController?.SetMoving(false);
            Movement.RotateToCamera();

            if (UseRootMotion)
                Controller.PlayerAnimatorController?.EnableRootMotion();

            if (IsAirCombat)
                Movement.SetGravityScale(AirGravityScale);
        }

        public override void Update()
        {
            // 공격 중 자유 이동/회전 처리
            if (IsSkillActive)
            {
                HandleCombatMovement();
            }

            // 공격 종료 + 유예 아님 → 상태 탈출
            if (!IsSkillActive && !IsInComboGrace)
            {
                ReturnToPreviousState();
                return;
            }

            // 유예 중 이동 입력 → 즉시 전환
            if (!IsSkillActive && HasMoveInput())
            {
                ResetCombo();
                StateMachine.ChangeState<MoveState>();
            }
        }

        /// <summary>
        /// 공격 중 이동/회전 처리 (DMC/Bayonetta 스타일)
        /// </summary>
        protected virtual void HandleCombatMovement()
        {
            // 1. 카메라 방향으로 부드럽게 회전
            if (AllowRotationDuringAttack)
            {
                Movement.SmoothRotateToCamera();
            }

            // 2. WASD 입력 시 위치만 이동 (회전 없이)
            if (AllowMovementDuringAttack)
            {
                var moveInput = Controller.Input.MoveInput;
                Movement.MoveWithoutRotation(moveInput, CombatMoveSpeedMultiplier);
            }
        }

        public override void Exit()
        {
            Controller.PlayerAnimatorController?.DisableRootMotion();

            if (IsAirCombat)
                Movement.ResetGravityScale();
        }

        // 서브클래스에서 구현해야 할 추상 멤버
        protected abstract bool IsSkillActive { get; }
        protected abstract bool IsInComboGrace { get; }
        protected abstract void ResetCombo();

        /// <summary>
        /// 콤보 입력 시 체공 연장 (공중 전용)
        /// </summary>
        protected void ApplyComboLift()
        {
            if (IsAirCombat && ComboLiftForce > 0)
                Movement.AddVerticalVelocity(ComboLiftForce);
        }

        /// <summary>
        /// 이전 상태로 복귀
        /// </summary>
        protected void ReturnToPreviousState()
        {
            // 공중이면 Idle로 전환 (Animator가 IsGrounded=false로 Fall 처리)
            if (!Movement.IsGrounded)
            {
                StateMachine.ChangeState<IdleState>();
                return;
            }

            if (HasMoveInput()) StateMachine.ChangeState<MoveState>();
            else StateMachine.ChangeState<IdleState>();
        }

        /// <summary>
        /// 공격 종료 이벤트 핸들러
        /// </summary>
        protected void OnSkillEnded() => ReturnToPreviousState();
    }
}