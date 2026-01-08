using UnityEngine;

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

    private IEnemyAction _currentAction;

    // 수치 데이터
    private readonly float _rushSpeed = 20f;
    private readonly float _rushDuration = 0.54f;

    private readonly float _minWait = 1f;
    private readonly float _maxWait = 3f;
    private readonly float _orbitRadius = 5f;
    private readonly float _orbitSpeed = 120f;
    private readonly float _aroundChance = 0.01f;
    private readonly float _aroundSpeedMurtiplier = 0.6f;

    public NormalAttackPattern(
        Transform player,
        Transform enemy,
        float damage,
        EnemyMovement movement,
        RushAttackHitbox rushHitbox,
        BiteAttackHitbox biteHitbox,
        EnemyAttack attack,
        Animator animator
    )
    {
        _player = player;
        _enemy = enemy;
        _damage = damage;
        _movement = movement;
        _rushHitbox = rushHitbox;
        _biteHitbox = biteHitbox;
        _animator = animator;
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
            _rushSpeed,
            _rushDuration
        );
        _currentAction.Enter();
    }

    private void StartWait()
    {
        _currentAction = new AttackWaitAction(
            _enemy,
            _player,
            _movement,
            _minWait,
            _maxWait,
            _orbitRadius,
            _orbitSpeed,
            _aroundChance,
            _aroundSpeedMurtiplier
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
            _damage
        );
        _currentAction.Enter();
    }

    public void Stop()
    {
        _currentAction?.Exit();
        _currentAction = null;
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
