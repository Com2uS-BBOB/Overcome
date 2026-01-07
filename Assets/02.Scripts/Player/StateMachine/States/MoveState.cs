using _02.Scripts.Player.Core;
using UnityEngine;

namespace _02.Scripts.Player.StateMachine.States
{
    public class MoveState : PlayerStateBase
    {
        public MoveState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Update()
        {
            // 이동 처리
            Movement.Move(Input.MoveInput);

            // 애니메이션 업데이트 (4방향 Blend Tree)
            Vector2 localVelocity = Movement.GetLocalVelocity(Input.MoveInput);
            Controller.PlayerAnimator?.UpdateMovement(localVelocity);

            // 상태 전환
            if (!HasMoveInput()) StateMachine.ChangeState<IdleState>();
        }
    }
}