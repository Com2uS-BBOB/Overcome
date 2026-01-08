using UnityEngine;
using UnityEngine.AI;

public class EnemyState : MonoBehaviour
{
    private EnemyBase _enemy;
    private EnemyMovement _movement;
    private EnemyAttack _attack;
    private EnemyAttackDirector _attackDirector;
    private EnemySlotCoordinator _slotCoordinator;
    private NavMeshAgent _agent;

    private EEnemyState _currentState;

    private bool _canMove => _enemy.CanMove;
    private bool _canReturn => _enemy.CanReturn;
    private bool _canAttack => _enemy.CanAttack;

    private Transform _player;

    private bool _didOpeningRush;
    private RushAction _openingRushAction;
    private AttackWaitAction _pressureWaitAction;

    [Header("Trace 관련 옵션")]
    [SerializeField] private float _detectRange = 10f;

    [Header("Return 관련 옵션")]
    [SerializeField] private float _outRange = 18f;
    [SerializeField] private float _returnStopDistance = 0.3f;

    [Header("Attack 관련 옵션")]
    [SerializeField] private float _attackRange = 10f;

    [Header("오프닝 돌진 옵션")]
    [SerializeField] private float _openingRushDistance = 10f;
    [SerializeField] private float _openingRushDuration = 0.54f;

    [Header("압박 Wait 옵션")]
    [SerializeField] private float _pressureSpeedMultiplier = 0.15f;
    private float _pressureWaitTime = 999f; // 사실상 무한대기

    private void Awake()
    {
        _enemy = GetComponent<EnemyBase>();
        _movement = GetComponent<EnemyMovement>();
        _attack = GetComponent<EnemyAttack>();
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        ChangeState(EEnemyState.Idle);
    }

    private void Update()
    {
        switch (_currentState)
        {
            case EEnemyState.Idle:
                UpdateIdle();
                break;

            case EEnemyState.Trace:
                _movement.ResetSpeedMultiplier();
                UpdateTrace();
                break;

            case EEnemyState.Return:
                _attack.ResetRush();
                UpdateReturn();
                break;

            case EEnemyState.Attack:
                UpdateAttack();
                break;
        }
    }

    public void Initialize(Transform player)
    {
        _player = player;
        _attack?.Initialize(player);

        _attackDirector = player.GetComponent<EnemyAttackDirector>();
        _slotCoordinator = player.GetComponent<EnemySlotCoordinator>();
    }

    #region State Updates

    private void UpdateIdle()
    {
        if (!_canMove || _player == null) return;

        if (IsPlayerInDetectRange())
        {
            ChangeState(EEnemyState.Trace);
        }
    }

    private void UpdateTrace()
    {
        if (_player == null)
        {
            CleanupEngagement();
            ChangeState(EEnemyState.Idle);
            return;
        }

        if (IsPlayerOutOfRange() && _canReturn)
        {
            CleanupEngagement();
            ChangeState(EEnemyState.Return);
            return;
        }

        if (!_didOpeningRush)
        {
            StartOrUpdateOpeningRush();
            return; // Rush 하는 동안은 Wait/Bite 판단하지 않음
        }

        // Rush 이후엔 항상 슬롯 압박 Wait 유지
        StartOrUpdatePressureWait();

        if (IsPlayerInAttackRange() && _canAttack)
        {
            bool granted = (_attackDirector == null) || _attackDirector.TryReserve(transform);
            if (granted)
            {
                int slotIndex = _pressureWaitAction != null ? _pressureWaitAction.SlotIndex : -1;
                _attack.SetCurrentSlotIndex(slotIndex);
                ChangeState(EEnemyState.Attack);
                return;
            }
        }
    }

    private void StartOrUpdateOpeningRush()
    {
        if (_openingRushAction == null)
        {
            var hitbox = GetComponentInChildren<EnemyKnockbackHitbox>();
            _openingRushAction = new RushAction(
                transform,
                _player,
                _movement,
                hitbox,
                _agent,
                _openingRushDistance,
                _openingRushDuration,
                _enemy.EnemyStatData.Damage
            );
            _openingRushAction.Enter();
        }

        _openingRushAction.Update();

        if (_openingRushAction.IsFinished)
        {
            _openingRushAction.Exit();
            _openingRushAction = null;
            _didOpeningRush = true;
        }
    }

    private void StartOrUpdatePressureWait()
    {
        if (_pressureWaitAction == null)
        {
            _pressureWaitAction = new AttackWaitAction(
                transform,
                _player,
                _movement,
                _slotCoordinator,
                _agent,
                _pressureWaitTime, _pressureWaitTime,
                _pressureSpeedMultiplier,
                releaseSlotOnExit: false // Bite 하러 잠깐 빠져도 슬롯 유지
            );
            _pressureWaitAction.Enter();
        }

        _pressureWaitAction.Update();
    }

    private void UpdateReturn()
    {
        Vector3 returnPosition = _enemy.GetSpawnBasePosition();

        _movement.SetRotationToMoveDirection();
        _movement.MoveTo(returnPosition);

        if (_movement.IsArrived(_returnStopDistance))
        {
            ChangeState(EEnemyState.Idle);
        }
    }

    private void UpdateAttack()
    {
        int slot = _pressureWaitAction != null ? _pressureWaitAction.SlotIndex : -1;

        if (_player == null)
        {
            _attack.Stop();
            _attackDirector?.Release(transform);
            CleanupEngagement();
            ChangeState(EEnemyState.Idle);
            return;
        }

        _attack.UpdateAttack();

        // Bite가 끝났으면 공격권 반납하고 다시 Trace(Wait 유지)로 복귀
        if (_attack.IsAttackFinished)
        {
            _attack.Stop();
            _attackDirector?.Release(transform);
            ChangeState(EEnemyState.Trace);
            return;
        }

        if (IsPlayerOutOfRange() && _canReturn)
        {
            _attack.Stop();
            _attackDirector?.Release(transform);
            CleanupEngagement();
            ChangeState(EEnemyState.Return);
        }
    }

    #endregion

    #region Cleanup

    private void CleanupEngagement()
    {
        // Rush 정리
        _openingRushAction?.Exit();
        _openingRushAction = null;

        // 슬롯 점유 해제
        if (_pressureWaitAction != null)
        {
            _pressureWaitAction.ReleaseSlotNow();
            _pressureWaitAction.Exit();
            _pressureWaitAction = null;
        }

        _didOpeningRush = false;
    }

    #endregion

    #region State Transition

    private void ChangeState(EEnemyState newState)
    {
        if (_currentState == newState) return;

        ExitState(_currentState);
        _currentState = newState;
        EnterState(_currentState);
    }

    private void EnterState(EEnemyState state)
    {
        switch (state)
        {
            case EEnemyState.Idle:
                _movement.Stop();
                break;

            case EEnemyState.Trace:
                _movement.SetRotationToLookAt(_player);
                break;

            case EEnemyState.Attack:
                _movement.Stop();
                _attack?.StartAttack();
                break;

            case EEnemyState.Return:
                _movement.SetRotationToMoveDirection();
                break;
        }
    }

    private void ExitState(EEnemyState state)
    {
        switch (state)
        {
            case EEnemyState.Trace:
                _movement.Stop();
                break;

            case EEnemyState.Attack:
                break;
        }
    }

    #endregion

    #region Range Checks

    private bool IsPlayerInDetectRange()
    {
        return _player != null && Vector3.Distance(transform.position, _player.position) <= _detectRange;
    }

    private bool IsPlayerOutOfRange()
    {
        return _player != null && Vector3.Distance(transform.position, _player.position) > _outRange;
    }

    private bool IsPlayerInAttackRange()
    {
        return _player != null && Vector3.Distance(transform.position, _player.position) <= _attackRange;
    }

    #endregion
}