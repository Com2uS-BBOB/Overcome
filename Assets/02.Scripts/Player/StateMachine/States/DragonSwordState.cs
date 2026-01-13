using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    public class DragonSwordState : PlayerStateBase
    {
        private bool _wasGroundedOnEnter;

        public DragonSwordState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            // 전투 중 이동 애니메이션 비활성화
            Controller.PlayerAnimatorController?.SetMoving(false);

            Movement.RotateToCamera();
            _wasGroundedOnEnter = Movement.IsGrounded;

            // 지상 공격에서만 Root Motion 활성화
            if (_wasGroundedOnEnter)
            {
                Controller.PlayerAnimatorController?.EnableRootMotion();
            }

            if (Combat != null)
            {
                Combat.OnAttackEnded += OnAttackEnded;
            }
        }

        public override void Update()
        {
            // 공중에서 시작했지만 착지한 경우 Root Motion 활성화
            if (!_wasGroundedOnEnter && Movement.IsGrounded)
            {
                _wasGroundedOnEnter = true;
                Controller.PlayerAnimatorController?.EnableRootMotion();
            }

            // 콤보 유예 중 이동 입력 시 즉시 State 전환
            if (!Combat.IsAttacking && HasMoveInput())
            {
                Combat.ResetCombo();
                StateMachine.ChangeState<MoveState>();
            }
        }

        public override void Exit()
        {
            // Root Motion 비활성화
            Controller.PlayerAnimatorController?.DisableRootMotion();

            if (Combat != null) Combat.OnAttackEnded -= OnAttackEnded;
        }

        private void OnAttackEnded() => ReturnToPreviousState();

        private void ReturnToPreviousState()
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
    }
}