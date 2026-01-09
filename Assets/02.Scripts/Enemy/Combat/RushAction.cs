using UnityEngine;
using UnityEngine.AI;

public class RushAction : IEnemyAction
{
    private readonly Transform _enemy;
    private readonly Transform _player;
    private readonly EnemyMovement _movement;
    private readonly EnemyKnockbackHitbox _hitbox;
    private readonly NavMeshAgent _agent;

    private readonly float _rushDistance;
    private readonly float _maxDuration;
    private readonly float _damage;

    private float _timer;
    private bool _isFinished;

    private Vector3 _rushDirection;  // Enter 순간에 고정되는 방향
    private Vector3 _startPosition;
    private float _speed;            // distance / duration

    private float _sqrMagnitudeThreshold = 0.01f;
    private bool _savedAgentUpdatePosition;
    private bool _savedAgentUpdateRotation;

    public bool IsFinished => _isFinished;

    public RushAction(
        Transform enemy,
        Transform player,
        EnemyMovement movement,
        EnemyKnockbackHitbox hitbox,
        NavMeshAgent agent,
        float rushDistance,
        float maxDuration,
        float damage
    )
    {
        _enemy = enemy;
        _player = player;
        _movement = movement;
        _hitbox = hitbox;
        _agent = agent;
        _rushDistance = rushDistance;
        _maxDuration = maxDuration;
        _damage = damage;
    }

    public void Enter()
    {
        _isFinished = false;
        _timer = 0f;

        _startPosition = _enemy.position;

        Vector3 snapPlayerPosition = _player.position;
        Vector3 direction = (snapPlayerPosition - _enemy.position);
        direction.y = 0f;

        if (direction.sqrMagnitude < _sqrMagnitudeThreshold)
        {
            _isFinished = true;
            return;
        }

        _rushDirection = direction.normalized;
        _speed = _rushDistance / Mathf.Max(0.01f, _maxDuration);

        _agent.ResetPath();
        _agent.isStopped = true;

        _savedAgentUpdatePosition = _agent.updatePosition;
        _savedAgentUpdateRotation = _agent.updateRotation;
        _agent.updatePosition = false;
        _agent.updateRotation = false;

        // 방향 고정(시작 순간만)
        _enemy.rotation = Quaternion.LookRotation(_rushDirection);

        _hitbox?.Enable(_damage);

        Debug.Log("돌진 공격 시도");
    }

    public void Update()
    {
        if (_isFinished) return;

        _timer += Time.deltaTime;

        Vector3 step = _rushDirection * (_speed * Time.deltaTime);
        _agent.Move(step);

        // 진행 거리로 종료 판단 (remainingDistance 같은 경로 기반 값 쓰지 않음)
        float traveled = Vector3.Distance(new Vector3(_startPosition.x, 0, _startPosition.z), new Vector3(_enemy.position.x, 0, _enemy.position.z));

        if (_timer >= _maxDuration || traveled >= _rushDistance)
        {
            _isFinished = true;
        }
    }

    public void Exit()
    {
        if (_hitbox != null)
        {
            _hitbox.Disable();
        }

        // agent 원상복구
        _agent.updatePosition = _savedAgentUpdatePosition;
        _agent.updateRotation = _savedAgentUpdateRotation;
        _agent.isStopped = false;

        // Transform과 agent 위치 싱크
        _agent.Warp(_enemy.position);
        
        // 이전 경로 정리 - Rush 전 경로가 남아있으면 문제 발생
        _agent.ResetPath();

        _movement.ResetSpeedMultiplier();
    }
}