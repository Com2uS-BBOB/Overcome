using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    /// <summary>
    /// 공중 질풍참 (대시 공격) 상태
    /// 콤보 없음, 캔슬 불가
    /// </summary>
    public class AirDashAttackState : CombatStateBase
    {
        protected override bool IsAirCombat => true;
        protected override bool UseRootMotion => false;  // 대시는 자체 이동 처리
        protected override float AirGravityScale => 0.1f;  // 대시 중 거의 부유

        // 대시 중 이동/회전 불가
        protected override bool AllowMovementDuringAttack => false;
        protected override bool AllowRotationDuringAttack => false;

        // DashAttack은 콤보 없음
        protected override bool IsSkillActive => Controller.DashAttack != null && Controller.DashAttack.IsDashing;
        protected override bool IsInComboGrace => false;
        protected override void ResetCombo() { }

        public AirDashAttackState(PlayerController controller, PlayerStateMachine stateMachine)
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
            // 착지 체크도 안 함 (대시 완료 후 자연스럽게 복귀)
        }

        public override void Exit()
        {
            base.Exit();

            if (Controller.DashAttack != null)
                Controller.DashAttack.OnDashEnded -= OnSkillEnded;
        }
    }
}
