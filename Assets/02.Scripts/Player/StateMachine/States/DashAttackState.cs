using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    public class DashAttackState : PlayerStateBase
    {
        public DashAttackState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            // 전투 중 이동 애니메이션 비활성화
            Controller.PlayerAnimatorController?.SetMoving(false);

            Movement.RotateToCamera();

            if (Controller.DashAttack != null && Controller.DashAttack.CanUse)
            {
                Controller.DashAttack.OnDashEnded += OnDashEnded;
                Controller.DashAttack.Use();
            }
            else
            {
                ReturnToPreviousState();
            }
        }

        public override void Exit() => Controller.DashAttack.OnDashEnded -= OnDashEnded;

        private void OnDashEnded() => ReturnToPreviousState();

        private void ReturnToPreviousState()
        {
            // 공중이면 Idle로 전환 (Animator가 IsGrounded=false로 Fall 처리)
            if (!Movement.IsGrounded)
            {
                StateMachine.ChangeState<IdleState>();
                return;
            }

            if (HasMoveInput()) StateMachine.ChangeState<MoveState>();
            else StateMachine.ChangeState<IdleState>();
        }
    }
}