using UnityEngine;
using _02.Scripts.Player.Interfaces;

public class EnemyLogic : MonoBehaviour
{
    private EnemyBase _enemy;
    private EnemyMovement _movement;
    private EnemyAttack _attack;

    private EEnemyState _currentState;

    private bool _canMove => _enemy.EnemyType != EEnemyType.Small;
    private bool _canReturn => _enemy.EnemyType != EEnemyType.Small;
    private bool _canAttack => _enemy.EnemyType == EEnemyType.Elite;

    private Transform _player => PlayerLocator.Player;  // PlayerLocator를 플레이어에게 부착시켜 플레이어 정보를 가져옴

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
        if (!_canMove || _player == null)
        {
            ChangeState(EEnemyState.Idle);
            return;
        }

        if (IsPlayerOutRange())  // 플레이어가 탐지 범위를 벗어나면 Return 전환
        {
            Debug.Log("Trace -> Return");
            ChangeState(EEnemyState.Return);
            return;
        }

        if (IsPlayerInAttack() && _canAttack)  // 공격 범위에 들어오면 공격 상태로 전환 (엘리트 전용)
        {
            Debug.Log("Trace -> Attack");
            ChangeState(EEnemyState.Attack);
            return;
        }

        _movement.MoveTo(_player.position);
    }

    private void Return()
    {
        if (!_canReturn || _player == null)
        {
            ChangeState(EEnemyState.Idle);
            return;
        }

        Vector3 returnPosition = _enemy.GetSpawnBasePosition();
        _movement.MoveTo(returnPosition);


        if (_movement.IsArrived(returnPosition, _stopDistance))  // 스폰 위치에 도착하면 Idle 전환
        {
            Debug.Log("Return -> Idle");
            ChangeState(EEnemyState.Idle);
        }
    }

    private void Attack()
    {
        if (!_canAttack || _player == null)
        {
            ChangeState(EEnemyState.Idle);
            return;
        }

        if (IsPlayerOutAttack())  // 플레이어가 공격 범위를 벗어나면 Trace 전환
        {
            _attack.Stop();
            Debug.Log("Attack -> Trace");
            ChangeState(EEnemyState.Trace);
            return;
        }

        var damageable = _player.GetComponent<IDamageable>();
        if (damageable != null)
        {
            _attack.TryAttack(damageable);
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

    private bool IsPlayerOutAttack()
    {
        if (_player == null) return false;

        float distance = Vector3.Distance(transform.position, _player.position);
        return distance > _attackRange;
    }
}