using UnityEngine;
using UnityEngine.AI;

public class RushAction : IEnemyAction
{
    private readonly Transform _enemy;
    private readonly Transform _player;
    private readonly EnemyMovement _movement;
    private readonly RushAttackHitbox _hitbox;
    private readonly NavMeshAgent _agent;

    private readonly float _rushDistance;
    private readonly float _maxDuration;

    private float _timer;
    private bool _isFinished;
    private Vector3 _rushTarget;

    public bool IsFinished => _isFinished;

    public RushAction(
        Transform enemy,
        Transform player,
        EnemyMovement movement,
        RushAttackHitbox hitbox,
        NavMeshAgent agent,
        float rushDistance,
        float maxDuration
    )
    {
        _enemy = enemy;
        _player = player;
        _movement = movement;
        _hitbox = hitbox;
        _agent = agent;
        _rushDistance = rushDistance;
        _maxDuration = maxDuration;
    }

    public void Enter()
    {
        _isFinished = false;
        _timer = 0f;

        Vector3 direction = (_player.position - _enemy.position);
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
        {
            _isFinished = true;
            return;
        }

        direction.Normalize();

        _rushTarget = _enemy.position + direction * _rushDistance;

        // NavMesh 보정
        if (NavMesh.SamplePosition(_rushTarget, out var hit, 1.5f, NavMesh.AllAreas))
        {
            _rushTarget = hit.position;
        }

        _movement.MoveTo(_rushTarget);

        _movement.SetRotationToMoveDirection();
        _hitbox.EnableKnockback();
    }

    public void Update()
    {
        if (_isFinished) return;

        _timer += Time.deltaTime;

        if (_agent.remainingDistance <= _agent.stoppingDistance || _timer >= _maxDuration)
        {
            _isFinished = true;
        }
    }

    public void Exit()
    {
        _movement.Stop();
    }
}