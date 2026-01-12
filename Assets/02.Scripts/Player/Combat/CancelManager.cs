using System;
using System.Collections.Generic;
using _02.Scripts.Player.StateMachine;
using _02.Scripts.Player.StateMachine.States;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 스킬/액션 캔슬 우선순위 중앙 관리
    /// 우선순위: DashAttack > DragonSword = Crescent > Move > Idle
    /// </summary>
    public class CancelManager
    {
        // 액션 우선순위 정의 (높을수록 우선)
        private static readonly Dictionary<Type, int> ActionPriority = new()
        {
            { typeof(DashAttackState), 100 },
            { typeof(DragonSwordState), 50 },
            { typeof(CrescentState), 50 },
            { typeof(MoveState), 10 },
            { typeof(IdleState), 0 }
        };

        // 캔슬 규칙 정의: (현재 상태, 목표 상태) -> 캔슬 가능 여부
        private static readonly HashSet<(Type from, Type to)> CancelRules = new()
        {
            // Idle/Move에서는 모든 전투 상태로 전환 가능
            (typeof(IdleState), typeof(DragonSwordState)),
            (typeof(IdleState), typeof(CrescentState)),
            (typeof(IdleState), typeof(DashAttackState)),
            (typeof(MoveState), typeof(DragonSwordState)),
            (typeof(MoveState), typeof(CrescentState)),
            (typeof(MoveState), typeof(DashAttackState)),

            // DashAttack은 다른 것으로 캔슬 불가 (완료까지 대기)

            // DragonSword 콤보 중에는 자기 자신으로만 전환 (콤보 연결)
            (typeof(DragonSwordState), typeof(DragonSwordState)),

            // Crescent 콤보 중에는 자기 자신으로만 전환 (콤보 연결)
            (typeof(CrescentState), typeof(CrescentState)),
        };

        private readonly PlayerStateMachine _stateMachine;

        public CancelManager(PlayerStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        /// <summary>
        /// 현재 상태에서 목표 상태로 캔슬 가능한지 확인
        /// </summary>
        public bool CanCancelTo<TTarget>() where TTarget : IPlayerState
        {
            var currentType = _stateMachine.CurrentState?.GetType();
            var targetType = typeof(TTarget);

            // 현재 상태 없음 -> 전환 가능
            if (currentType == null) return true;

            // 같은 상태 -> 규칙 확인
            if (currentType == targetType)
            {
                return CancelRules.Contains((currentType, targetType));
            }

            // 우선순위 비교
            int currentPriority = GetPriority(currentType);
            int targetPriority = GetPriority(targetType);

            // 높은 우선순위로만 캔슬 가능 (또는 명시적 규칙 있을 때)
            if (targetPriority > currentPriority) return true;

            return CancelRules.Contains((currentType, targetType));
        }

        /// <summary>
        /// 캔슬 가능한 경우 상태 전환 시도
        /// </summary>
        public bool TryCancelTo<TTarget>() where TTarget : IPlayerState
        {
            if (!CanCancelTo<TTarget>()) return false;

            _stateMachine.ChangeState<TTarget>();
            return true;
        }

        private int GetPriority(Type stateType)
        {
            return ActionPriority.TryGetValue(stateType, out int priority) ? priority : 0;
        }
    }
}