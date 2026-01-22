using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    /// <summary>
    /// 점프 상태 - 공중에서의 기본 상태
    /// </summary>
    public class JumpState : PlayerStateBase
    {
        public JumpState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            // 점프 애니메이션은 이미 PlayerController.HandleJump()에서 트리거됨
            // IsGrounded=false 강제 동기화 (Hit 후 공중 상태 전환 시 안전장치)
            Controller.PlayerAnimatorController?.SetGrounded(false);
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