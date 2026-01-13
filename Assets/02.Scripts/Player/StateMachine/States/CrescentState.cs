using _02.Scripts.Player.Combat;
using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    /// <summary>
    /// 지상 크레센트 스킬 상태
    /// SkillData에서 UseRootMotion, MoveSpeedMultiplier 등을 동적으로 참조
    /// </summary>
    public class CrescentState : CombatStateBase
    {
        protected override bool IsAirCombat => false;
        protected override BaseSkill GetSkill() => Crescent;

        protected override bool IsSkillActive => Crescent.IsUsing;
        protected override bool IsInComboGrace => Crescent.IsInComboGrace;
        protected override void ResetCombo() => Crescent.ResetCombo();

        public CrescentState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            base.Enter();

            if (Crescent != null)
                Crescent.OnCrescentEnded += OnSkillEnded;
        }

        public override void Exit()
        {
            base.Exit();

            if (Crescent != null)
                Crescent.OnCrescentEnded -= OnSkillEnded;
        }
    }
}