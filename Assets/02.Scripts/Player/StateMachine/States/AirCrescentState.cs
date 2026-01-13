using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    /// <summary>
    /// 공중 크레센트 스킬 상태
    /// </summary>
    public class AirCrescentState : CombatStateBase
    {
        protected override bool IsAirCombat => true;
        protected override bool UseRootMotion => false;  // 공중: Root Motion OFF
        protected override float AirGravityScale => 0.8f;  // 부유감
        protected override float ComboLiftForce => 0.1f;   // 콤보마다 위로
        protected override float CombatMoveSpeedMultiplier => 0.3f;  // 공중 이동 속도 30%

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
