using UnityEngine;

public class EnemyLogic : MonoBehaviour
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
    [SerializeField] private float _detectRange = 6f;

    [Header("Return 관련 옵션")]
    [SerializeField] private float _outRange = 12f;
    [SerializeField] private float _stopDistance = 0.1f;

    [Header("Attack 관련 옵션")]
    [SerializeField] private float _attackRange = 3f;

    private void Awake()
    {
        _enemy = GetComponent<EnemyBase>();
        _movement = GetComponent<EnemyMovement>();
        _attack = GetComponent<EnemyAttack>(); // 엘리트 몬스터 전용
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

            case EEnemyState.Trace:
                Trace();
                break;
            case EEnemyState.Return:
                Return();
                break;
            case EEnemyState.Attack:
                Attack();
                break;
        }
    }

    public void Initialize(Transform player)
    {
        _player = player;
    }

    private void Idle()
    {
        if (!_canMove) return;

        if (IsPlayerInRange())
        {
            Debug.Log("Idle -> Trace");
            ChangeState(EEnemyState.Trace);
        }
    }

    private void Trace()
    {
        if (_player == null)
        {
            ChangeState(EEnemyState.Idle);
            return;
        }

        // 플레이어와 일정 간격 두기 활성화
        _movement.EnablePlayerSeparation(_player);

        // 플레이어가 탐지 범위를 벗어나면 Return 전환
        if (IsPlayerOutRange() && _canReturn)
        {
            Debug.Log("Trace -> Return");
            ChangeState(EEnemyState.Return);
            return;
        }

        // 공격 범위에 들어오면 공격 상태로 전환
        if (IsPlayerInAttack() && _canAttack)
        {
            Debug.Log("Trace -> Attack");
            ChangeState(EEnemyState.Attack);
            return;
        }

        _movement.MoveTo(_player.position);
    }

    private void Return()
    {
        if (_player == null)
        {
            ChangeState(EEnemyState.Idle);
            return;
        }

        // 플레이어와 일정 간격 두기 비활성화
        _movement.DisablePlayerSeparation();

        Vector3 returnPosition = _enemy.GetSpawnBasePosition();
        _movement.MoveTo(returnPosition);

        // 스폰 위치에 도착하면 Idle 전환
        if (_movement.IsArrived(returnPosition, _stopDistance))
        {
            Debug.Log("Return -> Idle");
            ChangeState(EEnemyState.Idle);
        }
    }

    private void Attack()
    {
        if (_player == null)
        {
            ChangeState(EEnemyState.Idle);
            return;
        }

        _attack.UpdateAttack();

        if (_attack.IsAttackFinished)
        {
            ChangeState(EEnemyState.Idle);
        }
    }

    private void ChangeState(EEnemyState newState)
    {
        if (_currentState == newState) return;

        OnExitState(_currentState);
        _currentState = newState;
        OnEnterState(_currentState);
    }

    // 상태 진입 시 처리
    private void OnEnterState(EEnemyState state)
    {
        if (state == EEnemyState.Attack)
        {
            _attack?.StartAttack();
        }

        if (state == EEnemyState.Idle)
        {
            _movement?.Stop();
        }
    }

    // 상태 종료 시 처리
    private void OnExitState(EEnemyState state)
    {
        if (state == EEnemyState.Trace)
        {
            _movement.Stop();
        }
    }

    private bool IsPlayerInRange()
    {
        if (_player == null) return false;

        float distance = Vector3.Distance(transform.position, _player.position);
        return distance <= _detectRange;
    }

    private bool IsPlayerOutRange()
    {
        if (_player == null) return false;

        float distance = Vector3.Distance(transform.position, _player.position);
        return distance > _outRange;
    }

    private bool IsPlayerInAttack()
    {
        if (_player == null) return false;

        float distance = Vector3.Distance(transform.position, _player.position);
        return distance <= _attackRange;
    }
}