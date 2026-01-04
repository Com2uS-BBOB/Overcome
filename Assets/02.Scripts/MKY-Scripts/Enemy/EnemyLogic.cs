using UnityEngine;

public class EnemyLogic : MonoBehaviour
{
    private EnemyBase _enemy;
    private EnemyMovement _movement;
    // private EnemyAttack _attack;

    private EEnemyState _currentState;

    private bool _canMove => _enemy.EnemyType != EEnemyType.Small;
    // private bool _canAttack => _enemy.EnemyType == EEnemyType.Elite;

    private Transform player => PlayerLocator.Player;

    [SerializeField] private float _detectRange = 6f;

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
        }
    }

    private void Idle()
    {
        if (!_canMove) return;

        if (IsPlayerInRange())
        {
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

        _movement.MoveTo(player.position);
    }

    private bool IsPlayerInRange()
    {
        if (player == null) return false;

        float distance = Vector3.Distance(transform.position, player.position);
        return distance <= _detectRange;
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