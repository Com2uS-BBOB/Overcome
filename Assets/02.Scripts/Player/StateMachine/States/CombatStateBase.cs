using UnityEngine;
using _02.Scripts.Player.Combat;
using _02.Scripts.Player.Core;
using _02.Scripts.Player.Data;

namespace _02.Scripts.Player.StateMachine.States
{
    /// <summary>
    /// 전투 상태 기반 클래스 (지상/공중 전투 공통 로직)
    /// SkillData에서 물리 설정을 동적으로 참조
    /// </summary>
    public abstract class CombatStateBase : PlayerStateBase
    {
        protected abstract bool IsAirCombat { get; }

        /// <summary>
        /// 현재 상태에서 사용하는 스킬 반환
        /// </summary>
        protected abstract BaseSkill GetSkill();

        /// <summary>
        /// 현재 스킬의 SkillData (콤보 시작 시점 기준)
        /// </summary>
        protected SkillData CurrentSkillData => GetSkill()?.CurrentSkillData;

        // SkillData에서 동적으로 값 가져오기
        // 주의: CurrentSkillData는 _wasGroundedOnComboStart 기반이므로 Enter() 시점에 잘못된 값 참조 가능
        // 공중 상태에서는 AirSkill을, 지상 상태에서는 GroundSkill을 직접 참조
        protected virtual bool UseRootMotion => GetCurrentContextSkillData()?.UseRootMotion ?? !IsAirCombat;
        protected virtual float AirGravityScale => GetCurrentContextSkillData()?.GravityScale ?? 0.3f;
        protected virtual float ComboLiftForce => GetCurrentContextSkillData()?.ComboLiftForce ?? 1.5f;
        protected virtual float CombatMoveSpeedMultiplier => GetCurrentContextSkillData()?.MoveSpeedMultiplier ?? 0.2f;

        /// <summary>
        /// 현재 상태(공중/지상)에 맞는 SkillData 반환
        /// CurrentSkillData와 달리 _wasGroundedOnComboStart가 아닌 IsAirCombat 기반
        /// </summary>
        private Data.SkillData GetCurrentContextSkillData()
        {
            var skillDataSet = GetSkill()?.SkillDataSet;
            if (skillDataSet == null) return null;
            return IsAirCombat ? skillDataSet.AirSkill : skillDataSet.GroundSkill;
        }

        // 공격 중 자유 이동/회전 설정
        protected virtual bool AllowMovementDuringAttack => true;   // 이동 허용
        protected virtual bool AllowRotationDuringAttack => true;   // 카메라 추적 회전 허용

        protected CombatStateBase(PlayerController controller, PlayerStateMachine stateMachine)
            : base(controller, stateMachine) { }

        public override void Enter()
        {
            // 전투 중 이동 애니메이션 비활성화
            Controller.PlayerAnimatorController?.SetMoving(false);
            Movement.RotateToCamera();

            if (UseRootMotion)
                Controller.PlayerAnimatorController?.EnableRootMotion();

            if (IsAirCombat)
                Movement.SetGravityScale(AirGravityScale);
        }

        public override void Update()
        {
            // 공격 중 자유 이동/회전 처리
            if (IsSkillActive)
            {
                HandleCombatMovement();
            }

            // 공격 종료 + 유예 아님 → 상태 탈출
            if (!IsSkillActive && !IsInComboGrace)
            {
                ReturnToPreviousState();
                return;
            }

            // 유예 중 이동 입력 → 즉시 전환
            if (!IsSkillActive && HasMoveInput())
            {
                ResetCombo();
                StateMachine.ChangeState<MoveState>();
            }
        }

        /// <summary>
        /// 공격 중 이동/회전 처리 (DMC/Bayonetta 스타일)
        /// </summary>
        protected virtual void HandleCombatMovement()
        {
            // 1. 카메라 방향으로 부드럽게 회전
            if (AllowRotationDuringAttack)
            {
                Movement.SmoothRotateToCamera();
            }

            // 2. WASD 입력 시 위치만 이동 (회전 없이)
            if (AllowMovementDuringAttack)
            {
                var moveInput = Controller.Input.MoveInput;
                Movement.MoveWithoutRotation(moveInput, CombatMoveSpeedMultiplier);
            }
        }

        public override void Exit()
        {
            Controller.PlayerAnimatorController?.DisableRootMotion();

            if (IsAirCombat)
                Movement.ResetGravityScale();

            // 스킬 상태 정리 (점프 등으로 캔슬 시 Animation Event 미발동 대비)
            ResetCombo();
        }

        // 서브클래스에서 구현해야 할 추상 멤버
        protected abstract bool IsSkillActive { get; }
        protected abstract bool IsInComboGrace { get; }
        protected abstract void ResetCombo();

        /// <summary>
        /// 콤보 입력 시 체공 연장 (공중 전용)
        /// </summary>
        protected void ApplyComboLift()
        {
            if (IsAirCombat && ComboLiftForce > 0)
                Movement.AddVerticalVelocity(ComboLiftForce);
        }

        /// <summary>
        /// 이전 상태로 복귀
        /// </summary>
        protected void ReturnToPreviousState()
        {
            Debug.Log($"[CombatStateBase] ReturnToPreviousState - IsGrounded={Movement.IsGrounded}, HasMoveInput={HasMoveInput()}");

            // 공중이면 FallState로 전환 (착지까지 대기)
            if (!Movement.IsGrounded)
            {
                Debug.Log("[CombatStateBase] Changing to FallState");
                StateMachine.ChangeState<FallState>();
                return;
            }

            if (HasMoveInput())
            {
                Debug.Log("[CombatStateBase] Changing to MoveState");
                StateMachine.ChangeState<MoveState>();
            }
            else
            {
                Debug.Log("[CombatStateBase] Changing to IdleState");
                StateMachine.ChangeState<IdleState>();
            }
        }

        /// <summary>
        /// 공격 종료 이벤트 핸들러
        /// </summary>
        protected void OnSkillEnded()
        {
            Debug.Log("[CombatStateBase] OnSkillEnded called, calling ReturnToPreviousState");
            ReturnToPreviousState();
        }
    }
}