using System;
using UnityEngine;
using _02.Scripts.Player.StateMachine;
using _02.Scripts.Player.StateMachine.States;

namespace _02.Scripts.Player.Animation
{
    // 플레이어 애니메이션 관리 (1-Layer Sub-State Machine 구조)
    // Base Layer: Locomotion, Jump, GroundCombat, AirCombat
    public class PlayerAnimatorController : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private RootMotionProxy _rootMotionProxy;
        [SerializeField] private AnimationEventProxy _animEventProxy;

        public event Action<Vector3> OnRootMotionUpdate;
        public event Action OnCrescentFireEvent;

        // Animator 파라미터 해시 - Locomotion
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int DoubleJumpHash = Animator.StringToHash("DoubleJump");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");

        // Animator 파라미터 해시 - Combat
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int AttackComboCountHash = Animator.StringToHash("AttackComboCount");
        private static readonly int DashAttackHash = Animator.StringToHash("DashAttack");
        private static readonly int CrescentHash = Animator.StringToHash("Crescent");
        private static readonly int CrescentComboCountHash = Animator.StringToHash("CrescentComboCount");

        private PlayerStateMachine _stateMachine;

        public void Initialize(PlayerStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
            _stateMachine.OnStateChanged += HandleStateChanged;

            if (_rootMotionProxy != null)
                _rootMotionProxy.OnRootMotionUpdate += HandleRootMotionUpdate;

            if (_animEventProxy != null)
                _animEventProxy.OnCrescentFire += HandleCrescentFire;
        }

        private void HandleCrescentFire()
        {
            OnCrescentFireEvent?.Invoke();
        }

        #region Root Motion

        public void EnableRootMotion() => _rootMotionProxy?.EnableRootMotion();
        public void DisableRootMotion() => _rootMotionProxy?.DisableRootMotion();

        private void HandleRootMotionUpdate(Vector3 deltaPosition)
            => OnRootMotionUpdate?.Invoke(deltaPosition);

        #endregion


        #region Locomotion

        /// <summary>
        /// 이동 상태 설정 (Idle ↔ Run 전환)
        /// </summary>
        public void SetMoving(bool isMoving) => _animator.SetBool(IsMovingHash, isMoving);

        /// <summary>
        /// 1단 점프 애니메이션 트리거
        /// </summary>
        public void PlayJump() => _animator.SetTrigger(JumpHash);

        /// <summary>
        /// 2단 점프 애니메이션 트리거
        /// </summary>
        public void PlayDoubleJump() => _animator.SetTrigger(DoubleJumpHash);

        /// <summary>
        /// 지면 상태 동기화
        /// </summary>
        public void SetGrounded(bool isGrounded) => _animator.SetBool(IsGroundedHash, isGrounded);

        #endregion

        #region Combat

        private void HandleStateChanged(Type previousState, Type newState)
        {
            // DragonSwordState: 애니메이션은 OnComboAttack 이벤트에서 처리
            // DashAttackState / AirDashAttackState: 여기서 직접 처리
            if (newState == typeof(DashAttackState) || newState == typeof(AirDashAttackState))
            {
                PlayDashAttack();
            }
            // Idle/Move 상태로 복귀 시 모든 Combat Layer 비활성화
            else if (newState == typeof(IdleState) || newState == typeof(MoveState))
            {
                EndCombat();
            }
        }

        /// <summary>
        /// 용검 공격
        /// </summary>
        public void PlayAttack(int comboStep, bool isGrounded)
        {
            Debug.Log($"[Anim] PlayAttack called - comboStep: {comboStep}, isGrounded: {isGrounded}");
            // 공격 시작 시점에 IsGrounded 즉시 동기화
            _animator.SetBool(IsGroundedHash, isGrounded);
            _animator.SetInteger(AttackComboCountHash, comboStep);
            _animator.SetTrigger(AttackHash);
        }

        /// <summary>
        /// 질풍참
        /// </summary>
        public void PlayDashAttack()
        {
            _animator.SetTrigger(DashAttackHash);
        }

        /// <summary>
        /// 크레센트
        /// </summary>
        public void PlayCrescent(int comboStep, bool isGrounded)
        {
            // 공격 시작 시점에 IsGrounded 즉시 동기화
            _animator.SetBool(IsGroundedHash, isGrounded);
            _animator.SetInteger(CrescentComboCountHash, comboStep);
            _animator.SetTrigger(CrescentHash);
        }

        /// <summary>
        /// 파라미터 리셋
        /// </summary>
        public void EndCombat()
        {
            _animator.ResetTrigger(AttackHash);
            _animator.ResetTrigger(CrescentHash);
            _animator.SetInteger(AttackComboCountHash, 0);
            _animator.SetInteger(CrescentComboCountHash, 0);
        }

        #endregion

        private void OnDestroy()
        {
            if (_stateMachine != null)
                _stateMachine.OnStateChanged -= HandleStateChanged;

            if (_rootMotionProxy != null)
                _rootMotionProxy.OnRootMotionUpdate -= HandleRootMotionUpdate;

            if (_animEventProxy != null)
                _animEventProxy.OnCrescentFire -= HandleCrescentFire;
        }
    }
}