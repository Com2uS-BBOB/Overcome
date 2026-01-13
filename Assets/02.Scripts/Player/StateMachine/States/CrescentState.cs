using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    public class CrescentState : PlayerStateBase
    {
        public CrescentState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            // 전투 중 이동 애니메이션 비활성화
            Controller.PlayerAnimatorController?.SetMoving(false);

            Movement.RotateToCamera();

            if (Crescent != null)
            {
                Crescent.OnCrescentEnded += OnCrescentEnded;
            }
        }

        public override void Update()
        {
            // 콤보 유예 중 이동 입력 시 즉시 State 전환
            if (!Crescent.IsUsing && HasMoveInput())
            {
                Crescent.ResetCombo();
                StateMachine.ChangeState<MoveState>();
            }
        }

        public override void Exit()
        {
            if (Crescent != null) Crescent.OnCrescentEnded -= OnCrescentEnded;
        }

        private void OnCrescentEnded() => ReturnToPreviousState();

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
