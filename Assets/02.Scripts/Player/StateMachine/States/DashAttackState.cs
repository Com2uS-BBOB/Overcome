using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    public class DashAttackState : PlayerStateBase
    {
        public DashAttackState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            Movement.RotateToCamera();

            if (Controller.DashAttack != null && Controller.DashAttack.CanUse)
            {
                Controller.DashAttack.OnDashEnded += OnDashEnded;
                Controller.DashAttack.Use();
            }
            else
            {
                ReturnToPreviousState();
            }
        }

        public override void Exit() => Controller.DashAttack.OnDashEnded -= OnDashEnded;

        private void OnDashEnded() => ReturnToPreviousState();

        private void ReturnToPreviousState()
        {
            if (HasMoveInput()) StateMachine.ChangeState<MoveState>();
            else StateMachine.ChangeState<IdleState>();
        }
    }
}