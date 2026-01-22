using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    public class IdleState : PlayerStateBase
    {
        public IdleState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            Controller.PlayerAnimatorController?.SetMoving(false);
            // IsGrounded=true 강제 동기화 (Hit 후 지상 상태 전환 시 안전장치)
            Controller.PlayerAnimatorController?.SetGrounded(true);
        }

        public override void Update()
        {
            // 낙하 감지 (지면에서 떨어짐)
            if (!Movement.IsGrounded)
            {
                StateMachine.ChangeState<FallState>();
                return;
            }

            if (HasMoveInput()) StateMachine.ChangeState<MoveState>();
        }
    }
}