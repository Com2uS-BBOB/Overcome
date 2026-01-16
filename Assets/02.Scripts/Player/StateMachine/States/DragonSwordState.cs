using _02.Scripts.Player.Combat;
using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    /// <summary>
    /// 지상 용검 공격 상태
    /// SkillData에서 UseRootMotion, MoveSpeedMultiplier 등을 동적으로 참조
    /// </summary>
    public class DragonSwordState : CombatStateBase
    {
        protected override bool IsAirCombat => false;
        protected override BaseSkill GetSkill() => Combat;

        protected override bool IsSkillActive => Combat.IsAttacking;
        protected override bool IsInComboGrace => Combat.IsInComboGrace;
        protected override void ResetCombo() => Combat.ResetCombo();

        public DragonSwordState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            base.Enter();

            if (Combat != null)
                Combat.OnAttackEnded += OnSkillEnded;
        }

        public override void Exit()
        {
            base.Exit();

            if (Combat != null)
                Combat.OnAttackEnded -= OnSkillEnded;
        }
    }
}