using _02.Scripts.Player.Core;

namespace _02.Scripts.Player.StateMachine.States
{
    /// <summary>
    /// Overdrive 진입 애니메이션 재생 상태
    /// 애니메이션 이벤트를 통해 VFX 타이밍과 상태 종료를 제어
    /// </summary>
    public class OverdriveActivationState : PlayerStateBase
    {
        public OverdriveActivationState(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            // 애니메이션 이벤트 구독
            if (Controller.PlayerAnimatorController != null)
            {
                Controller.PlayerAnimatorController.OnOverdriveReady += HandleOverdriveReady;
                Controller.PlayerAnimatorController.OnOverdriveEnd += HandleOverdriveEnd;
            }

            // Overdrive 진입 애니메이션 재생
            Controller.PlayerAnimatorController?.PlayOverdriveActivation();
        }


        public override void Update()
        {
            // 애니메이션 종료 이벤트를 기다림 (HandleOverdriveEnd에서 상태 전환)
        }

        public override void Exit()
        {
            // 애니메이션 이벤트 구독 해제
            if (Controller.PlayerAnimatorController != null)
            {
                Controller.PlayerAnimatorController.OnOverdriveReady -= HandleOverdriveReady;
                Controller.PlayerAnimatorController.OnOverdriveEnd -= HandleOverdriveEnd;
            }
        }

        /// <summary>
        /// 애니메이션 이벤트: VFX 시작 타이밍
        /// 실제 Overdrive 상태를 활성화하고 VFX 시작
        /// </summary>
        private void HandleOverdriveReady()
        {
            // 게이지 체크 없이 강제 Overdrive 발동
            Controller.GaugeManager?.ForceActivateOverDrive();
        }

        /// <summary>
        /// 애니메이션 이벤트: 진입 애니메이션 종료
        /// Idle 상태로 복귀
        /// </summary>
        private void HandleOverdriveEnd()
        {
            StateMachine.ChangeState<IdleState>();
        }
    }
}
