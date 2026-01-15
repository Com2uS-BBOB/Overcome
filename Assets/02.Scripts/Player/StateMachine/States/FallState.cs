using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    /// <summary>
    /// 낙하 상태 - 점프 없이 지면에서 떨어졌을 때
    /// JumpState와 유사하지만 점프 입력 없이 진입
    /// </summary>
    public class FallState : PlayerStateBase
    {
        public FallState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            // 이동 애니메이션 비활성화
            Controller.PlayerAnimatorController?.SetMoving(false);
            // 낙하 애니메이션은 Animator의 IsGrounded=false로 자동 처리
        }

        public override void Update()
        {
            // 공중 이동 처리
            Movement.Move(Input.MoveInput);

            // 착지 시 이전 상태로 복귀
            if (Movement.IsGrounded)
            {
                if (HasMoveInput())
                    StateMachine.ChangeState<MoveState>();
                else
                    StateMachine.ChangeState<IdleState>();
            }
        }
    }
}
