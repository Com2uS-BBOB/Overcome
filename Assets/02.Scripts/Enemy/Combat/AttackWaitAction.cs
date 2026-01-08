using UnityEngine;
using UnityEngine.AI;

public class AttackWaitAction : IEnemyAction
{
    private readonly Transform _enemy;
    private readonly Transform _player;
    private readonly EnemyMovement _movement;
    private readonly EnemySlotCoordinator _slotCoordinator;
    private readonly NavMeshAgent _agent;

    private readonly float _minWait;
    private readonly float _maxWait;
    private readonly float _waitSpeedMultiplier;

    private float _timer;
    private float _duration;
    private bool _isFinished;

    private int _mySlotIndex = -1;

    public bool IsFinished => _isFinished;

    public AttackWaitAction(
        Transform enemy,
        Transform player,
        EnemyMovement movement,
        EnemySlotCoordinator slotCoordinator,
        NavMeshAgent agent,
        float minWait,
        float maxWait,
        float waitSpeedMultiplier
    )
    {
        _enemy = enemy;
        _player = player;
        _movement = movement;
        _slotCoordinator = slotCoordinator;
        _agent = agent;

        _minWait = minWait;
        _maxWait = maxWait;
        _waitSpeedMultiplier = waitSpeedMultiplier;
    }

    public void Enter()
    {
        _isFinished = false;
        _timer = 0f;
        _duration = Random.Range(_minWait, _maxWait);

        // 슬롯 점유
        _mySlotIndex = _slotCoordinator.ClaimSlot(_enemy);

        _agent.isStopped = false;
        _movement.SetRotationToLookAt(_player);
    }

    public void Update()
    {
        if (_mySlotIndex < 0) return;

        Vector3 slotPosition = _slotCoordinator.GetSlotPosition(_mySlotIndex);
        _agent.SetDestination(slotPosition);

        // 아직 슬롯에 도착하지 못함
        if (_agent.remainingDistance > _agent.stoppingDistance)
        {
            _movement.ResetSpeedMultiplier();
            _timer = 0f;
            return;
        }

        // 슬롯에 도착한 상태
        _movement.SetSpeedMultiplier(_waitSpeedMultiplier);
        _timer += Time.deltaTime;

        if (_timer >= _duration)
        {
            _isFinished = true;
        }
    }

    public void Exit()
    {
        if (_mySlotIndex >= 0)
        {
            _slotCoordinator.ReleaseSlot(_mySlotIndex);
            _mySlotIndex = -1;
        }

        _movement.ResetSpeedMultiplier();
    }
}