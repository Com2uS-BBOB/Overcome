using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    public class IdleState : PlayerStateBase
    {
        public IdleState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            Controller.PlayerAnimator?.SetMoving(false);
        }

        public override void Update()
        {
            if (HasMoveInput()) StateMachine.ChangeState<MoveState>();
        }
    }
}