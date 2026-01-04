using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    public class MoveState : PlayerStateBase
    {
        public MoveState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Update()
        {
            Movement.Move(Input.MoveInput);
            if (!HasMoveInput()) StateMachine.ChangeState<IdleState>();
        }
    }
}