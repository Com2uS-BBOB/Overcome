using UnityEngine;
using _02.Scripts.Player.Core;
using _02.Scripts.Player.Combat;
using _02.Scripts.Player.Gauge;

namespace _02.Scripts.Player.StateMachine.States
{
    /// <summary>
    /// 가드 상태
    /// - E키 홀드로 유지
    /// - 전방 180도 가드 가능
    /// - 선딜/후딜 0 (즉시 전환)
    /// </summary>
    public class GuardState : PlayerStateBase
    {
        private readonly GuardSettings _guardSettings;
        private readonly GuardManager _guardManager;
        private readonly GaugeManager _gaugeManager;

        public GuardState(PlayerController controller, PlayerStateMachine stateMachine,
            GuardSettings guardSettings, GuardManager guardManager, GaugeManager gaugeManager)
            : base(controller, stateMachine)
        {
            _guardSettings = guardSettings;
            _guardManager = guardManager;
            _gaugeManager = gaugeManager;
        }

        public override void Enter()
        {
            // 가드 시작 알림
            _guardManager.StartGuard();

            // 가드 포즈 애니메이션 (Trigger + Bool)
            Controller.PlayerAnimatorController?.PlayGuard();

            // 입력 이벤트 구독
            Input.OnGuardCanceled += HandleGuardRelease;
            Input.OnAttackPerformed += HandleAttackInput;
            Input.OnCrescentPerformed += HandleSkillInput;
            Input.OnDashAttackPerformed += HandleDashInput;
        }

        public override void Update()
        {
            // 공중으로 떨어지면 가드 해제
            if (!Movement.IsGrounded)
            {
                ExitGuard();
                return;
            }

            // 카메라 방향으로 회전
            Movement.RotateToCamera();
        }

        public override void Exit()
        {
            _guardManager.EndGuard();

            Controller.PlayerAnimatorController?.StopGuard();

            Input.OnGuardCanceled -= HandleGuardRelease;
            Input.OnAttackPerformed -= HandleAttackInput;
            Input.OnCrescentPerformed -= HandleSkillInput;
            Input.OnDashAttackPerformed -= HandleDashInput;
        }

        private void HandleGuardRelease()
        {
            ExitGuard();
        }

        private void HandleAttackInput()
        {
            // 가드 상태에서 즉시 공격으로 전환 (선딜레이 0)
            _guardManager.ForceEndGuard();
            StateMachine.ChangeState<DragonSwordState>();
            Controller.DragonSwordSkill?.Attack();
            Controller.CombatStateHandler?.SetCurrentAttackFromSkill(Controller.DragonSwordSkill);
        }

        private void HandleSkillInput()
        {
            // 가드 상태에서 즉시 크레센트로 전환
            if (Controller.Crescent != null && Controller.Crescent.CanUse)
            {
                _guardManager.ForceEndGuard();
                StateMachine.ChangeState<CrescentState>();
                Controller.Crescent.Attack();
                Controller.CombatStateHandler?.SetCurrentAttackFromSkill(Controller.Crescent);
            }
        }

        private void HandleDashInput()
        {
            // 가드 상태에서 대시 어택으로 전환
            if (Controller.DashAttack != null && Controller.DashAttack.CanUse)
            {
                _guardManager.ForceEndGuard();
                StateMachine.ChangeState<DashAttackState>();
            }
        }

        private void ExitGuard()
        {
            if (!Movement.IsGrounded)
            {
                StateMachine.ChangeState<FallState>();
                return;
            }

            if (HasMoveInput())
                StateMachine.ChangeState<MoveState>();
            else
                StateMachine.ChangeState<IdleState>();
        }
    }
}