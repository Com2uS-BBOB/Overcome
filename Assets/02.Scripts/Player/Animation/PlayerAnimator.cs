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

        // Animator 파라미터 해시
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int DashAttackHash = Animator.StringToHash("DashAttack");
        private static readonly int CrescentHash = Animator.StringToHash("Crescent");

        private PlayerStateMachine _stateMachine;

        public void Initialize(PlayerStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
            _stateMachine.OnStateChanged += HandleStateChanged;
        }

        private void HandleStateChanged(Type previousState, Type newState)
        {
            if (newState == typeof(IdleState))
            {
                _animator.SetBool(IsMovingHash, false);
            }
            else if (newState == typeof(MoveState))
            {
                _animator.SetBool(IsMovingHash, true);
            }
            else if (newState == typeof(DragonSwordState))
            {
                _animator.SetTrigger(AttackHash);
            }
            else if (newState == typeof(DashAttackState))
            {
                _animator.SetTrigger(DashAttackHash);
            }
        }

        // 크레센트, state 없는 행동 -> 추후 작업
        public void PlayCrescent() => _animator.SetTrigger(CrescentHash);

        private void OnDestroy()
        {
            if (_stateMachine != null)
                _stateMachine.OnStateChanged -= HandleStateChanged;
        }
    }
}