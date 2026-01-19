using UnityEngine;
using _02.Scripts.Player.Core;
using _02.Scripts.Player.Data;
using _02.Scripts.Player.Combat;

namespace _02.Scripts.Player.StateMachine.States
{
    /// <summary>
    /// 피격 상태
    /// - 0.3초 경직
    /// - 가드 입력만 허용
    /// - 넉백은 EnemyKnockbackHitbox에서 처리
    /// </summary>
    public class HitState : PlayerStateBase
    {
        private readonly GuardSettings _guardSettings;
        private readonly GuardManager _guardManager;

        private float _hitTimer;
        private AttackInfo _attackInfo;

        public HitState(PlayerController controller, PlayerStateMachine stateMachine,
            GuardSettings guardSettings, GuardManager guardManager)
            : base(controller, stateMachine)
        {
            _guardSettings = guardSettings;
            _guardManager = guardManager;
        }

        /// <summary>
        /// 피격 정보 설정 (상태 진입 전 호출)
        /// </summary>
        public void SetAttackInfo(AttackInfo attackInfo)
        {
            _attackInfo = attackInfo;
        }

        public override void Enter()
        {
            _hitTimer = 0f;

            // 피격 애니메이션 재생
            Controller.PlayerAnimatorController?.PlayHit();

            // 모든 스킬 리셋
            Controller.DragonSwordSkill?.ResetCombo();
            Controller.Crescent?.ResetCombo();

            // CombatStateHandler 상태 정리 (캔슬 체크 버그 방지)
            Controller.CombatStateHandler?.ClearCurrentAttack();
            Controller.InputBuffer?.Clear();

            // 가드 입력 이벤트 구독 (피격 중 가드로 탈출 가능)
            Input.OnGuardStarted += HandleGuardInput;
        }

        public override void Update()
        {
            _hitTimer += Time.deltaTime;

            // 피격 시간 종료 (넉백은 EnemyKnockbackHitbox에서 처리)
            if (_hitTimer >= _guardSettings.HitStaggerDuration)
            {
                ReturnToNormalState();
            }
        }

        public override void Exit()
        {
            Input.OnGuardStarted -= HandleGuardInput;
        }

        private void HandleGuardInput()
        {
            // 피격 중에도 가드로 전환 가능
            if (_guardManager != null)
            {
                StateMachine.ChangeState<GuardState>();
            }
        }

        private void ReturnToNormalState()
        {
            // 가드 키를 홀드 중이면 가드 상태로 복귀
            if (Input.IsGuardHeld && _guardManager != null && Movement.IsGrounded)
            {
                StateMachine.ChangeState<GuardState>();
                return;
            }

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