using UnityEngine;

public class EnemyLogic : MonoBehaviour
{
    private EnemyBase _enemy;
    private EnemyMovement _movement;
    // private EnemyAttack _attack;

    private EEnemyState _currentState;

    private bool _canMove => _enemy.EnemyType != EEnemyType.Small;
    private bool _canReturn => _enemy.EnemyType != EEnemyType.Small;
    // private bool _canAttack => _enemy.EnemyType == EEnemyType.Elite;

    private Transform player => PlayerLocator.Player;

    [Header("Trace 관련 옵션")]
    [SerializeField] private float _detectRange = 6f;

    [Header("Return 관련 옵션")]
    [SerializeField] private float _outRange = 12f;
    [SerializeField] private float _stopDistance = 0.1f;

    private void Awake()
    {
        _enemy = GetComponent<EnemyBase>();
        _movement = GetComponent<EnemyMovement>();
        // _attack = GetComponent<EnemyAttack>(); // 엘리트 몬스터 전용
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
                Idle();
                break;

            case EEnemyState.Chase:
                Chase();
                break;
            case EEnemyState.Return:
                Return();
                break;
        }
    }

    private void Idle()
    {
        if (!_canMove) return;

        if (IsPlayerInRange())
        {
            Debug.Log("Idle -> Trace");
            ChangeState(EEnemyState.Chase);
        }
    }

    private void Chase()
    {
        if (!_canMove || player == null)
        {
            ChangeState(EEnemyState.Idle);
            return;
        }

        if (IsPlayerOutRange())
        {
            Debug.Log("Trace -> Return");
            ChangeState(EEnemyState.Return);
            return;
        }

        _movement.MoveTo(player.position);
    }

    private void Return()
    {
        if (!_canReturn || player == null)
        {
            ChangeState(EEnemyState.Idle);
            return;
        }

        Vector3 returnPosition = _enemy.GetSpawnBasePosition();
        _movement.MoveTo(returnPosition);

        
        float distance = Vector3.Distance(transform.position, returnPosition);
        if (distance < _stopDistance)
        {
            Debug.Log("Return -> Idle");
            ChangeState(EEnemyState.Idle);
        }
    }

    private bool IsPlayerInRange()
    {
        if (player == null) return false;

        float distance = Vector3.Distance(transform.position, player.position);
        return distance <= _detectRange;
    }

    private bool IsPlayerOutRange()
    {
        if (player == null) return false;

        float distance = Vector3.Distance(transform.position, player.position);
        return distance >= _outRange;
    }

    private void ChangeState(EEnemyState newState)
    {
        if (_currentState == newState) return;

        OnExitState(_currentState);
        _currentState = newState;
        OnEnterState(_currentState);
    }

    private void OnEnterState(EEnemyState state)
    {
        if (state == EEnemyState.Idle)
        {
            _movement?.Stop();
        }
    }

    private void OnExitState(EEnemyState state)
    {
        if (state == EEnemyState.Chase)
        {
            _movement.Stop();
        }
    }
}