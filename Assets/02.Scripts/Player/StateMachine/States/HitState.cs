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
    /// - 넉백 레벨에 따른 밀림
    /// </summary>
    public class HitState : PlayerStateBase
    {
        private readonly GuardSettings _guardSettings;
        private readonly GuardManager _guardManager;

        private float _hitTimer;
        private AttackInfo _attackInfo;
        private Vector3 _knockbackVelocity;
        private float _knockbackTimer;

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
            _knockbackTimer = 0f;

            // 넉백 속도 계산
            if (_attackInfo.KnockbackLevel != KnockbackLevel.None && _attackInfo.KnockbackDuration > 0f)
            {
                float speed = _attackInfo.KnockbackDistance / _attackInfo.KnockbackDuration;
                Vector3 knockbackDir = _attackInfo.Direction;
                knockbackDir.y = 0f;
                knockbackDir.Normalize();
                _knockbackVelocity = knockbackDir * speed;
            }
            else
            {
                _knockbackVelocity = Vector3.zero;
            }

            // 피격 애니메이션 재생
            Controller.PlayerAnimatorController?.PlayHit();

            // 모든 스킬 리셋
            Controller.DragonSwordSkill?.ResetCombo();
            Controller.Crescent?.ResetCombo();

            // 가드 입력 이벤트 구독 (피격 중 가드로 탈출 가능)
            Input.OnGuardStarted += HandleGuardInput;
        }

        public override void Update()
        {
            _hitTimer += Time.deltaTime;

            // 넉백 적용
            if (_knockbackVelocity.sqrMagnitude > 0.01f && _knockbackTimer < _attackInfo.KnockbackDuration)
            {
                _knockbackTimer += Time.deltaTime;
                CharacterController.Move(_knockbackVelocity * Time.deltaTime);
            }

            // 피격 시간 종료
            if (_hitTimer >= _guardSettings.HitStaggerDuration)
            {
                ReturnToNormalState();
            }
        }

        public override void Exit()
        {
            Input.OnGuardStarted -= HandleGuardInput;
            _knockbackVelocity = Vector3.zero;
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