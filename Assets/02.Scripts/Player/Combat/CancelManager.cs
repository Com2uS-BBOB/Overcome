using System;
using System.Collections.Generic;
using _02.Scripts.Player.StateMachine;
using _02.Scripts.Player.StateMachine.States;

namespace _02.Scripts.Player.Combat
{
    /// <summary>
    /// 스킬/액션 캔슬 우선순위 중앙 관리
    /// 우선순위: DashAttack > DragonSword = Crescent > Move > Idle
    /// CombatStateHandler와 InputBuffer를 통합하여 정밀한 캔슬 윈도우 관리
    /// </summary>
    public class CancelManager
    {
        // 액션 우선순위 정의 (높을수록 우선)
        private static readonly Dictionary<Type, int> ActionPriority = new()
        {
            // 대시 공격 (최고 우선순위)
            { typeof(DashAttackState), 100 },
            { typeof(AirDashAttackState), 100 },
            // 전투 스킬
            { typeof(DragonSwordState), 50 },
            { typeof(AirDragonSwordState), 50 },
            { typeof(CrescentState), 50 },
            { typeof(AirCrescentState), 50 },
            // 점프
            { typeof(JumpState), 20 },
            // 이동
            { typeof(MoveState), 10 },
            { typeof(IdleState), 0 }
        };

        // 캔슬 규칙 정의: (현재 상태, 목표 상태) -> 캔슬 가능 여부
        private static readonly HashSet<(Type from, Type to)> CancelRules = new()
        {
            // Idle/Move에서는 모든 전투 상태로 전환 가능
            (typeof(IdleState), typeof(DragonSwordState)),
            (typeof(IdleState), typeof(AirDragonSwordState)),
            (typeof(IdleState), typeof(CrescentState)),
            (typeof(IdleState), typeof(AirCrescentState)),
            (typeof(IdleState), typeof(DashAttackState)),
            (typeof(IdleState), typeof(AirDashAttackState)),
            (typeof(IdleState), typeof(JumpState)),

            (typeof(MoveState), typeof(DragonSwordState)),
            (typeof(MoveState), typeof(AirDragonSwordState)),
            (typeof(MoveState), typeof(CrescentState)),
            (typeof(MoveState), typeof(AirCrescentState)),
            (typeof(MoveState), typeof(DashAttackState)),
            (typeof(MoveState), typeof(AirDashAttackState)),
            (typeof(MoveState), typeof(JumpState)),

            // JumpState에서 공중 전투 상태로 전환 가능
            (typeof(JumpState), typeof(AirDragonSwordState)),
            (typeof(JumpState), typeof(AirCrescentState)),
            (typeof(JumpState), typeof(AirDashAttackState)),

            // DashAttack은 다른 것으로 캔슬 불가 (완료까지 대기)

            // DragonSword 콤보 중에는 자기 자신으로만 전환 (콤보 연결)
            (typeof(DragonSwordState), typeof(DragonSwordState)),
            (typeof(AirDragonSwordState), typeof(AirDragonSwordState)),

            // Crescent 콤보 중에는 자기 자신으로만 전환 (콤보 연결)
            (typeof(CrescentState), typeof(CrescentState)),
            (typeof(AirCrescentState), typeof(AirCrescentState)),

            // === 스킬 간 양방향 전환 규칙 (캔슬 윈도우 내에서만) ===
            // 지상: DragonSword <-> Crescent
            (typeof(DragonSwordState), typeof(CrescentState)),
            (typeof(CrescentState), typeof(DragonSwordState)),

            // 공중: AirDragonSword <-> AirCrescent
            (typeof(AirDragonSwordState), typeof(AirCrescentState)),
            (typeof(AirCrescentState), typeof(AirDragonSwordState)),

            // DashAttack 후 다른 스킬로 전환
            (typeof(DashAttackState), typeof(DragonSwordState)),
            (typeof(DashAttackState), typeof(CrescentState)),
            (typeof(AirDashAttackState), typeof(AirDragonSwordState)),
            (typeof(AirDashAttackState), typeof(AirCrescentState)),
        };

        // ActionType -> State Type 매핑
        private static readonly Dictionary<ActionType, Type> ActionToStateMap = new()
        {
            { ActionType.Attack, typeof(DragonSwordState) },
            { ActionType.Skill, typeof(CrescentState) },
            { ActionType.DashAttack, typeof(DashAttackState) },
            { ActionType.Move, typeof(MoveState) },
        };

        private readonly PlayerStateMachine _stateMachine;
        private CombatStateHandler _combatHandler;
        private InputBuffer _inputBuffer;

        // 액션 실행 콜백 (PlayerController에서 등록)
        public event Action<ActionType> OnActionExecute;

        public CancelManager(PlayerStateMachine stateMachine)
        {
            _stateMachine = stateMachine;
        }

        /// <summary>
        /// CombatStateHandler 및 InputBuffer 연결
        /// </summary>
        public void SetCombatHandler(CombatStateHandler combatHandler, InputBuffer inputBuffer)
        {
            _combatHandler = combatHandler;
            _inputBuffer = inputBuffer;

            if (_combatHandler != null)
            {
                _combatHandler.OnCancelWindowEnter += HandleCancelWindowEnter;
                _combatHandler.OnBufferedActionReady += HandleBufferedActionReady;
            }
        }

        /// <summary>
        /// 액션 실행 시도 (캔슬 윈도우 + 상태 우선순위 통합 체크)
        /// </summary>
        public bool TryExecuteAction(ActionType action)
        {
            // 1. 상태 기반 우선순위 체크
            if (!CanExecuteByState(action))
            {
                // 캔슬 불가 → 버퍼에 저장
                _inputBuffer?.Buffer(action);
                return false;
            }

            // 2. 캔슬 윈도우 체크 (CombatStateHandler가 있는 경우)
            if (_combatHandler != null && _combatHandler.CurrentAttack != null)
            {
                if (!_combatHandler.CanCancelInto(action))
                {
                    // 캔슬 윈도우 밖 → 버퍼에 저장
                    _inputBuffer?.Buffer(action);
                    return false;
                }
            }

            // 3. 액션 실행
            ExecuteAction(action);
            return true;
        }

        /// <summary>
        /// 액션 실행
        /// </summary>
        private void ExecuteAction(ActionType action)
        {
            OnActionExecute?.Invoke(action);
        }

        /// <summary>
        /// 상태 기반으로 액션 실행 가능한지 확인
        /// </summary>
        private bool CanExecuteByState(ActionType action)
        {
            if (!ActionToStateMap.TryGetValue(action, out var targetState))
                return true; // 매핑 없으면 항상 허용

            var currentType = _stateMachine.CurrentState?.GetType();
            if (currentType == null) return true;

            // 같은 상태 → 규칙 확인
            if (currentType == targetState)
            {
                return CancelRules.Contains((currentType, targetState));
            }

            // 우선순위 비교
            int currentPriority = GetPriority(currentType);
            int targetPriority = GetPriority(targetState);

            if (targetPriority > currentPriority) return true;

            return CancelRules.Contains((currentType, targetState));
        }

        /// <summary>
        /// 캔슬 윈도우 진입 시 버퍼 소비
        /// </summary>
        private void HandleCancelWindowEnter()
        {
            if (_inputBuffer == null) return;

            if (_inputBuffer.TryConsume(out var buffered))
            {
                if (CanExecuteByState(buffered.Action))
                {
                    ExecuteAction(buffered.Action);
                }
            }
        }

        /// <summary>
        /// 버퍼에서 액션이 준비됐을 때
        /// </summary>
        private void HandleBufferedActionReady(InputBuffer.BufferedInput buffered)
        {
            if (CanExecuteByState(buffered.Action))
            {
                ExecuteAction(buffered.Action);
            }
        }

        #region Legacy API (하위 호환)

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

        #endregion

        private int GetPriority(Type stateType)
        {
            return ActionPriority.TryGetValue(stateType, out int priority) ? priority : 0;
        }

        public void Cleanup()
        {
            if (_combatHandler != null)
            {
                _combatHandler.OnCancelWindowEnter -= HandleCancelWindowEnter;
                _combatHandler.OnBufferedActionReady -= HandleBufferedActionReady;
            }
        }
    }
}