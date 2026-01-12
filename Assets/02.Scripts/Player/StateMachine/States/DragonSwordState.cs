using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    public class DragonSwordState : PlayerStateBase
    {
        public DragonSwordState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            Movement.RotateToCamera();

            // 지상 공격에서만 Root Motion 활성화
            if (Movement.IsGrounded)
            {
                Controller.PlayerAnimatorController?.EnableRootMotion();
            }

            if (Combat != null)
            {
                Combat.OnAttackEnded += OnAttackEnded;
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
            if (HasMoveInput()) StateMachine.ChangeState<MoveState>();
            else StateMachine.ChangeState<IdleState>();
        }
    }
}