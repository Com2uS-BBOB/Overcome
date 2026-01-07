using System;
using UnityEngine;
using _02.Scripts.Player.StateMachine;
using _02.Scripts.Player.StateMachine.States;

namespace _02.Scripts.Player.Animation
{
    // 플레이어 애니메이션 관리 (3-Layer 시스템)
    // Layer 0: Locomotion (이동, 점프)
    // Layer 1: UpperCombat (공중 상체 공격)
    // Layer 2: Combat (지상 전신 공격)
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private Animator _animator;

        // Layer 인덱스
        private const int UpperCombatLayerIndex = 1;
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
        private float _upperCombatLayerWeight;
        private float _combatLayerWeight;

        public void Initialize(PlayerStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
            _stateMachine.OnStateChanged += HandleStateChanged;
        }

        private void Update()
        {
            // UpperCombat Layer Weight 부드럽게 전환
            float currentUpper = _animator.GetLayerWeight(UpperCombatLayerIndex);
            float newUpper = Mathf.Lerp(currentUpper, _upperCombatLayerWeight, 10f * Time.deltaTime);
            _animator.SetLayerWeight(UpperCombatLayerIndex, newUpper);

            // Combat Layer Weight 부드럽게 전환
            float currentCombat = _animator.GetLayerWeight(CombatLayerIndex);
            float newCombat = Mathf.Lerp(currentCombat, _combatLayerWeight, 10f * Time.deltaTime);
            _animator.SetLayerWeight(CombatLayerIndex, newCombat);
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
            // DragonSwordState: 애니메이션은 OnComboAttack 이벤트에서 처리
            // DashAttackState: 여기서 직접 처리
            if (newState == typeof(DashAttackState))
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
        /// 용검 공격 (지상: 전신, 공중: 상체만)
        /// </summary>
        public void PlayAttack(int comboStep, bool isGrounded)
        {
            _animator.SetInteger(ComboCountHash, comboStep);
            _animator.SetTrigger(AttackHash);

            if (isGrounded)
            {
                _combatLayerWeight = 1f;
                _upperCombatLayerWeight = 0f;
            }
            else
            {
                _upperCombatLayerWeight = 1f;
                _combatLayerWeight = 0f;
            }
        }

        /// <summary>
        /// 질풍참 (항상 전신)
        /// </summary>
        public void PlayDashAttack()
        {
            _animator.SetTrigger(DashAttackHash);
            _combatLayerWeight = 1f;
            _upperCombatLayerWeight = 0f;
        }

        /// <summary>
        /// 크레센트 (지상: 전신, 공중: 상체만)
        /// </summary>
        public void PlayCrescent(bool isGrounded)
        {
            _animator.SetTrigger(CrescentHash);

            if (isGrounded)
            {
                _combatLayerWeight = 1f;
                _upperCombatLayerWeight = 0f;
            }
            else
            {
                _upperCombatLayerWeight = 1f;
                _combatLayerWeight = 0f;
            }
        }

        /// <summary>
        /// 모든 전투 레이어 비활성화 및 파라미터 리셋
        /// </summary>
        public void EndCombat()
        {
            _combatLayerWeight = 0f;
            _upperCombatLayerWeight = 0f;

            // Trigger 및 ComboCount 리셋
            _animator.ResetTrigger(AttackHash);
            _animator.SetInteger(ComboCountHash, 0);
        }

        #endregion

        private void OnDestroy()
        {
            if (_stateMachine != null)
                _stateMachine.OnStateChanged -= HandleStateChanged;
        }
    }
}