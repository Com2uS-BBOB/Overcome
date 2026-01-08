using UnityEngine;
using UnityEngine.AI;

public class NormalAttackPattern : IEnemyAttackPattern
{
    // 공용 컨텍스트
    private readonly Transform _player;
    private readonly Transform _enemy;
    private readonly float _damage;
    private readonly EnemyMovement _movement;
    private readonly RushAttackHitbox _rushHitbox;
    private readonly BiteAttackHitbox _biteHitbox;
    private readonly Animator _animator;
    private readonly EnemySlotCoordinator _slotCoordinator;
    private readonly NavMeshAgent _agent;

    private IEnemyAction _currentAction;

    // 수치 데이터
    private readonly float _rushDuration = 0.54f;
    private readonly float _rushDistance = 4f;

    private readonly float _minWait = 1f;
    private readonly float _maxWait = 3f;
    private readonly float _waitSpeedMurtiplier = 0.1f;

    public NormalAttackPattern(
        Transform player,
        Transform enemy,
        float damage,
        EnemyMovement movement,
        RushAttackHitbox rushHitbox,
        BiteAttackHitbox biteHitbox,
        EnemyAttack attack,
        Animator animator,
        EnemySlotCoordinator slotCoordinator
    )
    {
        _player = player;
        _enemy = enemy;
        _damage = damage;
        _movement = movement;
        _rushHitbox = rushHitbox;
        _biteHitbox = biteHitbox;
        _animator = animator;
        _slotCoordinator = slotCoordinator;
        _agent = enemy.GetComponent<NavMeshAgent>();
    }

    public void Start()
    {
        StartRush();
    }

    public void Update()
    {
        _currentAction?.Update();

        if (_currentAction != null && _currentAction.IsFinished)
        {
            ChangeAction();
        }
    }

    private void ChangeAction()
    {
        _currentAction.Exit();

        if (_currentAction is RushAction)
        {
            StartWait();
        }

        else if (_currentAction is AttackWaitAction)
        {
            StartBite();
        }

        else if (_currentAction is BiteAction)
        {
            StartWait();
        }
    }

    private void StartRush()
    {
        _currentAction = new RushAction(
            _enemy,
            _player,
            _movement,
            _rushHitbox,
            _enemy.GetComponent<NavMeshAgent>(),
            _rushDuration,
            _rushDistance
        );
        _currentAction.Enter();
    }

    private void StartWait()
    {
        _currentAction = new AttackWaitAction(
            _enemy,
            _player,
            _movement,
            _slotCoordinator,
            _agent,
            _minWait,
            _maxWait,
            _waitSpeedMurtiplier
        );
        _currentAction.Enter();
    }

    private void StartBite()
    {
        _currentAction = new BiteAction(
            _enemy,
            _player,
            _biteHitbox,
            _animator,
            _enemy.GetComponent<NavMeshAgent>(),
            _damage
        );
        _currentAction.Enter();
    }

    public void Stop()
    {
        _currentAction?.Exit();
        _currentAction = null;
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

    public bool IsFinished => false; // 반복 패턴
}
