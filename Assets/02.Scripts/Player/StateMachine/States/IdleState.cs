using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    /// <summary>
    /// 대기 상태
    /// </summary>
    public class IdleState : PlayerStateBase
    {
        public IdleState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine)
        {
        }

        public override void Update()
        {
            // 이동 입력 → MoveState
            if (HasMoveInput())
            {
                StateMachine.ChangeState<MoveState>();
            }
        }
    }
}