using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    /// <summary>
    /// 질풍참 상태
    /// </summary>
    public class DashAttackState : PlayerStateBase
    {
        public DashAttackState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            // 질풍참 시작
            if (Movement.CanDash)
            {
                Movement.OnDashEnded += OnDashEnded;
                Movement.DashAttack();
            }
            else
            {
                // 질풍참 불가 → 즉시 복귀
                ReturnToPreviousState();
            }
        }

        public override void Exit()
        {
            base.Exit();
            Movement.OnDashEnded -= OnDashEnded;
        }

        private void OnDashEnded()
        {
            // 질풍참 종료 → 이동 입력에 따라 전환
            ReturnToPreviousState();
        }

        private void ReturnToPreviousState()
        {
            if (HasMoveInput())
            {
                StateMachine.ChangeState<MoveState>();
            }
            else
            {
                StateMachine.ChangeState<IdleState>();
            }
        }
    }
}