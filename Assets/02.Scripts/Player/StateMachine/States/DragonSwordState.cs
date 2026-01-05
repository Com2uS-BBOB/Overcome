using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    public class DragonSwordState : PlayerStateBase
    {
        public DragonSwordState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            if (Combat != null && Combat.CanAttack)
            {
                Combat.OnAttackEnded += OnAttackEnded;
                Combat.Attack();
            }
            else
            {
                ReturnToPreviousState();
            }
        }

        public override void Exit()
        {
            if (Combat != null) Combat.OnAttackEnded -= OnAttackEnded;
        }

        private void OnAttackEnded() => ReturnToPreviousState();

        private void ReturnToPreviousState()
        {
            if (HasMoveInput()) StateMachine.ChangeState<MoveState>();
            else StateMachine.ChangeState<IdleState>();
        }
    }
}