using UnityEngine;
using _02.Scripts.Player.Core;
using _02.Scripts.Player.Combat;

namespace _02.Scripts.Player.StateMachine.States
{
    /// <summary>
    /// 가드 브레이크 상태
    /// - 1초 경직
    /// - 슬로우 모션 (0.3x)
    /// - 모든 입력 불가
    /// </summary>
    public class GuardBreakState : PlayerStateBase
    {
        private readonly GuardSettings _guardSettings;

        private float _breakTimer;
        private float _originalTimeScale;

        public GuardBreakState(PlayerController controller, PlayerStateMachine stateMachine,
            GuardSettings guardSettings)
            : base(controller, stateMachine)
        {
            _guardSettings = guardSettings;
        }

        public override void Enter()
        {
            _breakTimer = 0f;

            // 타임스케일 저장 및 변경
            _originalTimeScale = Time.timeScale;
            Time.timeScale = _guardSettings.GuardBreakTimeScale;

            // 가드 브레이크 애니메이션
            Controller.PlayerAnimatorController?.PlayGuardBreak();

            // 모든 스킬 리셋
            Controller.DragonSwordSkill?.ResetCombo();
            Controller.Crescent?.ResetCombo();

            // CombatStateHandler 상태 정리 (캔슬 체크 버그 방지)
            Controller.CombatStateHandler?.ClearCurrentAttack();
            Controller.InputBuffer?.Clear();
        }

        public override void Update()
        {
            // unscaledDeltaTime 사용 (타임스케일 영향 받지 않음)
            _breakTimer += Time.unscaledDeltaTime;

            if (_breakTimer >= _guardSettings.GuardBreakDuration)
            {
                ReturnToNormalState();
            }
        }

        public override void Exit()
        {
            Time.timeScale = _originalTimeScale;
        }

        private void ReturnToNormalState()
        {
            bool isGrounded = Movement.IsGrounded;
            bool isMoving = HasMoveInput();

            // Animator 상태 강제 동기화
            Controller.PlayerAnimatorController?.SyncLocomotionState(isGrounded, isMoving);

            if (!isGrounded)
            {
                StateMachine.ChangeState<FallState>();
                return;
            }

            if (isMoving)
                StateMachine.ChangeState<MoveState>();
            else
                StateMachine.ChangeState<IdleState>();
        }
    }
}