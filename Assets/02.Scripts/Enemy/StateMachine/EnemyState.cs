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

    [Header("Trace 관련 옵션")]
    [SerializeField] private float _detectRange = 10f;
    [SerializeField] private float _standOffDistance = 8.5f;  // 비압박자 유지 거리(attackRange보다 크게)
    [SerializeField] private float _standOffRepathInterval = 0.4f;  // 목적지 자주 바뀜 방지

    private float _standOffRepathTimer;
    private bool _pressureReserved;

    [Header("Return 관련 옵션")]
    [SerializeField] private float _outRange = 18f;
    [SerializeField] private float _returnStopDistance = 0.3f;

    [Header("Attack 관련 옵션")]
    [SerializeField] private float _attackRange = 5f;

    [Header("오프닝 돌진 옵션")]
    [SerializeField] private float _openingRushDistance = 10f;
    [SerializeField] private float _openingRushDuration = 0.54f;

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

    public void Initialize(EnemyCombatContext context)
    {
        _player = context.Player;
        _attackDirector = context.AttackDirector;

        _attack?.Initialize(context);
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
            _attack.Stop();
            CleanupEngagement();
            ChangeState(EEnemyState.Return);
            return;
        }

        if (!_didOpeningRush)
        {
            StartOrUpdateOpeningRush();
            return; // Rush 하는 동안은 Wait/Bite 판단하지 않음
        }

        if (_canAttack && IsPlayerInAttackRange())
        {
            if (_pressureReserved)
            {
                _attackDirector?.ReleasePressure(transform);
                _pressureReserved = false;
            }

            ChangeState(EEnemyState.Attack);
            return;
        }

        // 플레이어 가까이 갈 압박자 여부 결정
        bool isPressurer = (_attackDirector == null) || _attackDirector.TryReservePressure(transform);
        _pressureReserved = isPressurer && (_attackDirector != null);

        _movement.SetRotationToLookAt(_player);

        if (isPressurer)
        {
            // 압박자는 플레이어 쪽으로 접근
            _movement.MoveTo(_player.position);
        }
        else
        {
            // 비압박자는 떨어져서 이동
            _standOffRepathTimer -= Time.deltaTime;

            if (_standOffRepathTimer <= 0f)
            {
                _standOffRepathTimer = _standOffRepathInterval;

                Vector3 toEnemy = (transform.position - _player.position);
                toEnemy.y = 0f;

                Vector3 direction = toEnemy.sqrMagnitude > 0.01f ? toEnemy.normalized : Vector3.forward;
                Vector3 desired = _player.position + direction * _standOffDistance;

                // NavMesh 위로 보정
                if (NavMesh.SamplePosition(desired, out var hit, 1.5f, NavMesh.AllAreas))
                {
                    desired = hit.position;

                }
                _movement.MoveTo(desired);
            }
        }
    }

    private void UpdateAttack()
    {
        if (_player == null)
        {
            _attack.Stop();
            CleanupEngagement();
            ChangeState(EEnemyState.Idle);
            return;
        }

        // OutOfRange 체크를 최우선으로 (공격 중이어도 즉시 중단하고 Return)
        if (IsPlayerOutOfRange() && _canReturn)
        {
            _attack.Stop();
            CleanupEngagement();
            ChangeState(EEnemyState.Return);
            return;
        }

        // AttackRange 밖이면 추적으로 복귀
        if (!IsPlayerInAttackRange())
        {
            _attack.Stop();
            // Trace로 돌아갈 때도 OpeningRush 상태 유지를 위해 CleanupEngagement 호출하지 않음
            ChangeState(EEnemyState.Trace);
            return;
        }

        _attack.UpdateAttack();
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

    #endregion

    #region Cleanup

    private void CleanupEngagement()
    {
        // Rush 정리
        _openingRushAction?.Exit();
        _openingRushAction = null;
        _didOpeningRush = false;

        if (_pressureReserved)
        {
            _attackDirector?.ReleasePressure(transform);
            _pressureReserved = false;
        }
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