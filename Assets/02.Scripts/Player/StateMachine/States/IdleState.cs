using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    public class IdleState : PlayerStateBase
    {
        public IdleState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            // 이동 애니메이션 정지 (Idle로 블렌딩)
            Controller.PlayerAnimator?.StopMovement();
        }

        public override void Update()
        {
            // Idle 상태에서도 부드러운 블렌딩을 위해 계속 호출
            Controller.PlayerAnimator?.StopMovement();

            if (HasMoveInput()) StateMachine.ChangeState<MoveState>();
        }
    }
}