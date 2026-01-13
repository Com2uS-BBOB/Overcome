using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    /// <summary>
    /// 지상 질풍참 (대시 공격) 상태
    /// 콤보 없음, 캔슬 불가
    /// </summary>
    public class DashAttackState : CombatStateBase
    {
        protected override bool IsAirCombat => false;
        protected override bool UseRootMotion => false;  // 대시는 자체 이동 처리

        // 대시 중 이동/회전 불가
        protected override bool AllowMovementDuringAttack => false;
        protected override bool AllowRotationDuringAttack => false;

        // DashAttack은 콤보 없음
        protected override bool IsSkillActive => Controller.DashAttack != null && Controller.DashAttack.IsDashing;
        protected override bool IsInComboGrace => false;
        protected override void ResetCombo() { }

        public DashAttackState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            base.Enter();

            if (Controller.DashAttack != null && Controller.DashAttack.CanUse)
            {
                Controller.DashAttack.OnDashEnded += OnSkillEnded;
                Controller.DashAttack.Use();
            }
            else
            {
                ReturnToPreviousState();
            }
        }

        public override void Update()
        {
            // DashAttack은 캔슬 불가, 완료까지 대기
            // base.Update() 호출 안 함 (콤보 유예 체크 불필요)
        }

        public override void Exit()
        {
            base.Exit();

            if (Controller.DashAttack != null)
                Controller.DashAttack.OnDashEnded -= OnSkillEnded;
        }
    }
}
