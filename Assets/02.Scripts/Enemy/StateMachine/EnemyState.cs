using UnityEngine;

public class EnemyState : MonoBehaviour
{
    private EnemyBase _enemy;
    private EnemyMovement _movement;
    private EnemyAttack _attack;

    private EEnemyState _currentState;

    private bool _canMove => _enemy.CanMove;
    private bool _canReturn => _enemy.CanReturn;
    private bool _canAttack => _enemy.CanAttack;

    private Transform _player;

    [Header("Trace 관련 옵션")]
    [SerializeField] private float _detectRange = 10f;

    [Header("Return 관련 옵션")]
    [SerializeField] private float _outRange = 18f;
    [SerializeField] private float _returnStopDistance = 0.3f;

    [Header("Attack 관련 옵션")]
    [SerializeField] private float _attackRange = 10f;

    private void Awake()
    {
        _enemy = GetComponent<EnemyBase>();
        _movement = GetComponent<EnemyMovement>();
        _attack = GetComponent<EnemyAttack>();
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
            ChangeState(EEnemyState.Idle);
            return;
        }

        if (IsPlayerOutOfRange() && _canReturn)
        {
            ChangeState(EEnemyState.Return);
            return;
        }

        if (IsPlayerInAttackRange() && _canAttack)
        {
            ChangeState(EEnemyState.Attack);
            return;
        }

        _movement.SetRotationToLookAt(_player);
        _movement.MoveTo(_player.position);
    }

    private void UpdateReturn()
    {
        Vector3 returnPos = _enemy.GetSpawnBasePosition();

        _movement.SetRotationToMoveDirection();
        _movement.MoveTo(returnPos);

        if (_movement.IsArrived(_returnStopDistance))
        {
            ChangeState(EEnemyState.Idle);
        }
    }

    private void UpdateAttack()
    {
        if (_player == null)
        {
            _attack.Stop();
            ChangeState(EEnemyState.Idle);
            return;
        }

        _attack.UpdateAttack();

        if (IsPlayerOutOfRange() && _canReturn)
        {
            _attack.Stop();
            ChangeState(EEnemyState.Return);
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