using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    public class DragonSwordState : PlayerStateBase
    {
        private bool _wasGroundedOnEnter;

        public DragonSwordState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            Movement.RotateToCamera();
            _wasGroundedOnEnter = Movement.IsGrounded;

            // 지상 공격에서만 Root Motion 활성화
            if (_wasGroundedOnEnter)
            {
                Controller.PlayerAnimatorController?.EnableRootMotion();
            }

            if (Combat != null)
            {
                Combat.OnAttackEnded += OnAttackEnded;
            }
        }

        public override void Update()
        {
            // 공중에서 시작했지만 착지한 경우 Root Motion 활성화
            if (!_wasGroundedOnEnter && Movement.IsGrounded)
            {
                _wasGroundedOnEnter = true;
                Controller.PlayerAnimatorController?.EnableRootMotion();
            }
        }

        public override void Exit()
        {
            // Root Motion 비활성화
            Controller.PlayerAnimatorController?.DisableRootMotion();

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