using System;
using UnityEngine;
using _02.Scripts.Player.StateMachine;
using _02.Scripts.Player.StateMachine.States;

namespace _02.Scripts.Player.Animation
{
    // 플레이어 애니메이션 관리
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        // Layer 인덱스
        private const int CombatLayerIndex = 2;

        // Animator 파라미터 해시 - Locomotion
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int JumpHash = Animator.StringToHash("Jump");
        private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");

        // Animator 파라미터 해시 - Combat
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int ComboCountHash = Animator.StringToHash("ComboCount");
        private static readonly int DashAttackHash = Animator.StringToHash("DashAttack");
        private static readonly int CrescentHash = Animator.StringToHash("Crescent");

        private PlayerStateMachine _stateMachine;
        private float _combatLayerWeight;

        public void Initialize(PlayerStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
            _stateMachine.OnStateChanged += HandleStateChanged;
        }

        private void Update()
        {
            // Combat Layer Weight 부드럽게 전환
            float currentWeight = _animator.GetLayerWeight(CombatLayerIndex);
            float newWeight = Mathf.Lerp(currentWeight, _combatLayerWeight, 10f * Time.deltaTime);
            _animator.SetLayerWeight(CombatLayerIndex, newWeight);
        }

        #region Locomotion

        /// <summary>
        /// 이동 상태 설정 (Idle ↔ Run 전환)
        /// </summary>
        public void SetMoving(bool isMoving) => _animator.SetBool(IsMovingHash, isMoving);

        /// <summary>
        /// 점프 애니메이션 트리거
        /// </summary>
        public void PlayJump() => _animator.SetTrigger(JumpHash);

        /// <summary>
        /// 지면 상태 동기화
        /// </summary>
        public void SetGrounded(bool isGrounded) => _animator.SetBool(IsGroundedHash, isGrounded);

        #endregion

        #region Combat

        private void HandleStateChanged(Type previousState, Type newState)
        {
            // 전투 상태 진입 시 Combat Layer 활성화
            if (newState == typeof(DragonSwordState))
            {
                _combatLayerWeight = 1f;
                // 콤보 애니메이션은 OnComboAttack 이벤트에서 처리
            }
            else if (newState == typeof(DashAttackState))
            {
                _combatLayerWeight = 1f;
                _animator.SetTrigger(DashAttackHash);
            }
            // Idle/Move 상태로 복귀 시 Combat Layer 비활성화
            else if (newState == typeof(IdleState) || newState == typeof(MoveState))
            {
                _combatLayerWeight = 0f;
            }
        }

        /// <summary>
        /// 콤보 공격 애니메이션 재생
        /// </summary>
        public void PlayAttack(int comboStep)
        {
            _combatLayerWeight = 1f;
            _animator.SetInteger(ComboCountHash, comboStep);
            _animator.SetTrigger(AttackHash);
        }

        public void PlayCrescent()
        {
            _combatLayerWeight = 1f;
            _animator.SetTrigger(CrescentHash);
        }

        public void EndCombat()
        {
            _combatLayerWeight = 0f;
        }

        #endregion

        private void OnDestroy()
        {
            if (_stateMachine != null)
                _stateMachine.OnStateChanged -= HandleStateChanged;
        }
    }
}