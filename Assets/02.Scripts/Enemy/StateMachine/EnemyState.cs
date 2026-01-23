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

    private bool _isElite => _enemy != null && _enemy.EnemyType == EEnemyType.Elite;

    private Transform _player;

    [Header("Trace 관련 옵션")]
    [SerializeField] private float _detectRange = 22f;
    [SerializeField] private float _standOffDistance = 8.5f;  // 비압박자 유지 거리(attackRange보다 크게)
    [SerializeField] private float _standOffRepathInterval = 0.25f;  // 목적지 자주 바뀜 방지
    private float _maxSamplePositionDistance = 1.5f;

    private float _standOffRepathTimer;
    private bool _pressureReserved;

    [Header("Return 관련 옵션")]
    [SerializeField] private float _outRange = 32f;
    [SerializeField] private float _returnStopDistance = 0.3f;

    [Header("Attack 관련 옵션")]
    [SerializeField] private float _attackRange = 22f;

    // 게임 오버 정지 플러그 (중복 호출 방지용)
    private bool _stoppedByGameOver;

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

    private void OnEnable()
    {
        // 리스폰 시 상태 초기화 (Idle로 리셋)
        if (_currentState != EEnemyState.Idle)
        {
            _currentState = EEnemyState.Idle;
            _pressureReserved = false;
            _standOffRepathTimer = 0f;
        }

        // 리스폰되면 게임오버 정지 플래그 초기화
        _stoppedByGameOver = false;

        // GameOver 구독
        GameEventHandler.OnGameEnd += HandleGameOver;
    }

    private void OnDisable()
    {
        // GameOver 해제
        GameEventHandler.OnGameEnd -= HandleGameOver;
    }

    private void HandleGameOver()
    {
        ForceStopByGameOver();
    }

    public void ForceStopByGameOver()
    {
        if (_stoppedByGameOver) return;
        _stoppedByGameOver = true;

        // 공격 중단 (히트박스/코루틴 포함)
        _attack?.Stop();

        // 공격 대기 위협 해제
        CleanupEngagement();

        // 이동 완전 정지
        _movement?.FullStop();

        // 상태 고정
        _currentState = EEnemyState.Idle;

        // Update 루프 자체를 끊음
        enabled = false;
    }

    private void Update()
    {
        // enabled=false 전에 들어올 수 있는 경우 방어
        if (_stoppedByGameOver) return;

        if (_enemy != null && _enemy.IsDead)
        {
            // 죽었으면 전투 / 공격 루프를 끊어버림
            _attack?.Stop();
            CleanupEngagement();

            // 죽은 뒤 상태 고정
            _currentState = EEnemyState.Idle;
            return;
        }

        if (_enemy != null && _enemy.IsSpawnGap)
        {
            // 스폰 텀 동안은 AI 로직 자체를 돌리지 않음
            _movement?.Stop();
            return;
        }

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

        _movement.SetRotationToLookAt(_player);

        // 엘리트는 거리두고 위협 연출 시스템 완전 무시하고 플레이어 추적
        if (_isElite)
        {
            // 혹시 남아있을 수 있는 pressure 예약 흔적 정리(안전장치)
            if (_pressureReserved)
            {
                _attackDirector?.ReleasePressure(transform);
                _pressureReserved = false;
            }

            _movement.MoveTo(_player.position);
            return;
        }

        // 플레이어 가까이 갈 압박자 여부 결정
        bool isPressurer = (_attackDirector == null) || _attackDirector.TryReservePressure(transform);
        _pressureReserved = isPressurer && (_attackDirector != null);

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
                if (NavMesh.SamplePosition(desired, out var hit, _maxSamplePositionDistance, NavMesh.AllAreas))
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

        // Return 범위 밖이면 복귀
        if (IsPlayerOutOfRange() && _canReturn)
        {
            _attack.Stop();
            CleanupEngagement();
            ChangeState(EEnemyState.Return);
            return;
        }

        // AttackRange 밖이면 추적으로 복귀 + 돌진 공격 초기화
        if (!IsPlayerInAttackRange())
        {
            _attack.Stop();
            _attack.ResetRush();
            ChangeState(EEnemyState.Trace);
            return;
        }

        _attack.UpdateAttack();
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
                _attack.Stop();
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