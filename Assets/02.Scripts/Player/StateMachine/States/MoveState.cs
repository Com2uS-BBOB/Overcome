using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    /// <summary>
    /// 이동 상태
    /// </summary>
    public class MoveState : PlayerStateBase
    {
        public MoveState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine)
        {
        }

        public override void Update()
        {
            // 이동 처리
            Movement.Move(Input.MoveInput);

            // 입력 없음 → IdleState
            if (!HasMoveInput())
            {
                StateMachine.ChangeState<IdleState>();
            }
        }
    }
}