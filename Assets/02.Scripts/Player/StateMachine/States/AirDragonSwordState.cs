using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    /// <summary>
    /// 공중 용검 공격 상태
    /// </summary>
    public class AirDragonSwordState : CombatStateBase
    {
        protected override bool IsAirCombat => true;
        protected override bool UseRootMotion => false;  // 공중: Root Motion OFF
        protected override float AirGravityScale => 0.8f;  // 부유감
        protected override float ComboLiftForce => 0.1f;   // 콤보마다 위로

        protected override bool IsSkillActive => Combat.IsAttacking;
        protected override bool IsInComboGrace => Combat.IsInComboGrace;
        protected override void ResetCombo() => Combat.ResetCombo();

        public AirDragonSwordState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            base.Enter();

            if (Combat != null)
            {
                Combat.OnAttackEnded += OnSkillEnded;
                Combat.OnComboAttack += OnComboAttack;
            }
        }

        public override void Update()
        {
            base.Update();

            // 착지 시 지상 State로 전환
            if (Movement.IsGrounded && IsSkillActive)
            {
                // 공격 중 착지 → 지상 공격으로 전환
                StateMachine.ChangeState<DragonSwordState>();
            }
        }

        public override void Exit()
        {
            base.Exit();

            if (Combat != null)
            {
                Combat.OnAttackEnded -= OnSkillEnded;
                Combat.OnComboAttack -= OnComboAttack;
            }
        }

        private void OnComboAttack(int comboStep)
        {
            ApplyComboLift();  // 콤보마다 약간 위로
        }
    }
}