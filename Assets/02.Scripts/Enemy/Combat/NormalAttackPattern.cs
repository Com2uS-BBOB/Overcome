using UnityEngine;
using UnityEngine.AI;

public class NormalAttackPattern : IEnemyAttackPattern
{
    // 공용 컨텍스트
    private readonly Transform _player;
    private readonly Transform _enemy;
    private readonly float _damage;
    private readonly EnemyMovement _movement;
    private readonly EnemyKnockbackHitbox _knockbackHitbox;
    private readonly Animator _animator;
    private readonly NavMeshAgent _agent;
    private readonly EnemySlotCoordinator _slotCoordinator;

    private IEnemyAction _currentAction;
    private bool _isFinished;

    public bool IsFinished => _isFinished;

    // 수치 데이터
    private readonly float _minWait = 1f;
    private readonly float _maxWait = 3f;
    private readonly float _windupSpeed = 0.15f;
    private readonly int _slotIndex;

    public NormalAttackPattern(
        Transform player,
        Transform enemy,
        float damage,
        EnemyMovement movement,
        EnemyKnockbackHitbox knockbackHitbox,
        EnemyAttack attack,
        Animator animator,
        EnemySlotCoordinator slotCoordinator,
        int slotIndex
    )
    {
        _player = player;
        _enemy = enemy;
        _damage = damage;
        _movement = movement;
        _knockbackHitbox = knockbackHitbox;
        _animator = animator;
        _agent = enemy.GetComponent<NavMeshAgent>();
        _slotCoordinator = slotCoordinator;
        _slotIndex = slotIndex;
    }

    public void Start()
    {
        _isFinished = false;

        _currentAction = new AttackWaitAction(
            _enemy,
            _player,
            _movement,
            _slotCoordinator,
            _agent,
            _minWait,
            _maxWait,
            _windupSpeed,
            fixedSlotIndex: _slotIndex,
            releaseSlotOnExit: false
        );

        _currentAction.Enter();
    }

    public void Update()
    {
        if (_isFinished) return;

        _currentAction?.Update();

        if (_currentAction != null && _currentAction.IsFinished)
        {
            _currentAction.Exit();

            if (_currentAction is AttackWaitAction)
            {
                _currentAction = new BiteAction(
                    _enemy,
                    _player,
                    _knockbackHitbox,
                    _animator,
                    _enemy.GetComponent<NavMeshAgent>(),
                    _damage
                    );
                _currentAction.Enter();
                return;
            }

            if (_currentAction is BiteAction)
            {
                _currentAction.Exit();
                _isFinished = true;
            }
        }
    }

    public void Stop()
    {
        _currentAction?.Exit();
        _currentAction = null;
        _isFinished = true;
    }

    public void ForwardBiteStart()
    {
        (_currentAction as BiteAction)?.OnAnimStart();
    }

    public void ForwardBiteHitStart()
    {
        (_currentAction as BiteAction)?.OnHitStart();
    }

    public void ForwardBiteHitEnd()
    {
        (_currentAction as BiteAction)?.OnHitEnd();
    }

    public void ForwardBiteEnd()
    {
        (_currentAction as BiteAction)?.OnAnimEnd();
    }
}
