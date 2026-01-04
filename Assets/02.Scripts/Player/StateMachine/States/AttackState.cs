using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    /// <summary>
    /// 공격 상태 (용검)
    /// </summary>
    public class AttackState : PlayerStateBase
    {
        public AttackState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine)
        {
        }

        public override void Enter()
        {
            base.Enter();

            // 공격 시작
            if (Combat != null && Combat.CanAttack)
            {
                Combat.OnAttackEnded += OnAttackEnded;
                Combat.Attack();
            }
            else
            {
                // 공격 불가 → 즉시 복귀
                ReturnToPreviousState();
            }
        }

        public override void Exit()
        {
            base.Exit();

            if (Combat != null)
            {
                Combat.OnAttackEnded -= OnAttackEnded;
            }
        }

        private void OnAttackEnded()
        {
            // 공격 종료 → 이동 입력에 따라 전환
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