using _02.Scripts.Player.Combat;
using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    /// <summary>
    /// 공중 크레센트 스킬 상태
    /// SkillData에서 GravityScale, ComboLiftForce, MoveSpeedMultiplier 등을 동적으로 참조
    /// </summary>
    public class AirCrescentState : CombatStateBase
    {
        protected override bool IsAirCombat => true;
        protected override BaseSkill GetSkill() => Crescent;

        protected override bool IsSkillActive => Crescent.IsUsing;
        protected override bool IsInComboGrace => Crescent.IsInComboGrace;
        protected override void ResetCombo() => Crescent.ResetCombo();

        public AirCrescentState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            base.Enter();

            if (Crescent != null)
            {
                Crescent.OnCrescentEnded += OnSkillEnded;
                Crescent.OnComboAttack += OnComboAttack;
            }
        }

        public override void Update()
        {
            base.Update();

            // 착지 시 지상 State로 전환
            if (Movement.IsGrounded && IsSkillActive)
            {
                // 착지 시 Animator IsGrounded도 즉시 동기화
                Controller.PlayerAnimatorController?.SetGrounded(true);
                StateMachine.ChangeState<CrescentState>();
            }
        }

        public override void Exit()
        {
            base.Exit();

            if (Crescent != null)
            {
                Crescent.OnCrescentEnded -= OnSkillEnded;
                Crescent.OnComboAttack -= OnComboAttack;
            }
        }

        private void OnComboAttack(int comboStep)
        {
            ApplyComboLift();  // 콤보마다 약간 위로
        }
    }
}
