using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    public class CrescentState : PlayerStateBase
    {
        public CrescentState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            Movement.RotateToCamera();

            if (Crescent != null)
            {
                Crescent.OnCrescentEnded += OnCrescentEnded;
            }
        }

        public override void Exit()
        {
            if (Crescent != null) Crescent.OnCrescentEnded -= OnCrescentEnded;
        }

        private void OnCrescentEnded() => ReturnToPreviousState();

        private void ReturnToPreviousState()
        {
            if (HasMoveInput()) StateMachine.ChangeState<MoveState>();
            else StateMachine.ChangeState<IdleState>();
        }
    }
}
