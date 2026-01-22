using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    public class MoveState : PlayerStateBase
    {
        public MoveState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            Controller.PlayerAnimatorController?.SetMoving(true);
            // IsGrounded=true 강제 동기화 (Hit 후 지상 상태 전환 시 안전장치)
            Controller.PlayerAnimatorController?.SetGrounded(true);
        }

        public override void Update()
        {
            // 낙하 감지 (이동 중 지면에서 떨어짐)
            if (!Movement.IsGrounded)
            {
                StateMachine.ChangeState<FallState>();
                return;
            }

            // 이동 처리
            Movement.Move(Input.MoveInput);

            // 상태 전환
            if (!HasMoveInput()) StateMachine.ChangeState<IdleState>();
        }
    }
}