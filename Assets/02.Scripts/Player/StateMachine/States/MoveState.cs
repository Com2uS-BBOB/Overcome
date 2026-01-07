using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    public class MoveState : PlayerStateBase
    {
        public MoveState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            Controller.PlayerAnimator?.SetMoving(true);
        }

        public override void Update()
        {
            // 이동 처리
            Movement.Move(Input.MoveInput);

            // 상태 전환
            if (!HasMoveInput()) StateMachine.ChangeState<IdleState>();
        }
    }
}