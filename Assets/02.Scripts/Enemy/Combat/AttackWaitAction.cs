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
    private readonly bool _releaseSlotOnExit;
    private readonly int _fixedSlotIndex;

    private float _timer;
    private float _duration;
    private bool _isFinished;

    private int _mySlotIndex = -1;

    private EEnemyProbeMode _probeMode;
    private float _modeTimer;
    private float _modeDuration;

    private float _minModeTime = 0.4f;
    private float _maxModeTime = 1.0f;
    private float _shuffleAngle = 35f;     // 원호 이동 각도
    private float _feintDistance = 0.8f;
    private float _navSampleRadius = 1.5f;

    private float _arrivedThreshold = 0.2f;
    private float _minStoppingDistance = 0.1f;

    private float _shuffleModeValue = 0.40f;
    private float _feintModeValue = 0.70f;
    private float _repositionModeValue = 0.85f;

    private float _magnitudeThreshold = 0.01f;
    private float _randomValue = 0.5f;

    public bool IsFinished => _isFinished;
    public int SlotIndex => _mySlotIndex;

    public AttackWaitAction(
        Transform enemy,
        Transform player,
        EnemyMovement movement,
        EnemySlotCoordinator slotCoordinator,
        NavMeshAgent agent,
        float minWait,
        float maxWait,
        float waitSpeedMultiplier,
        bool releaseSlotOnExit = true,
        int fixedSlotIndex = -1
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
        _releaseSlotOnExit = releaseSlotOnExit;
        _fixedSlotIndex = fixedSlotIndex;
    }

    public void Enter()
    {
        _isFinished = false;
        _timer = 0f;
        _duration = Random.Range(_minWait, _maxWait);

        // 슬롯 점유 (이미 슬롯이 있으면 그 슬롯 사용)
        if (_fixedSlotIndex >= 0)
        {
            _mySlotIndex = _fixedSlotIndex;
        }
        else
        {
            _mySlotIndex = _slotCoordinator.ClaimSlot(_enemy);
        }

        _agent.isStopped = false;
        _movement.SetRotationToLookAt(_player);

        PickNewMode();
    }

    public void Update()
    {
        if (_mySlotIndex < 0) return;

        _modeTimer += Time.deltaTime;
        if (_modeTimer >= _modeDuration)
        {
            PickNewMode();
        }

        Vector3 destination = GetModeDestination();
        _agent.SetDestination(destination);

        // slotPosition과의 평면 거리 확인
        float flat = Vector3.Distance(new Vector3(_enemy.position.x, 0, _enemy.position.z),new Vector3(destination.x, 0, destination.z));
        
        // 슬롯 근처 또는 모드 목적지 근처면 대기 타이머 진행
        bool arrived = flat <= _arrivedThreshold || (!_agent.pathPending && _agent.remainingDistance <= Mathf.Max(_agent.stoppingDistance, _minStoppingDistance));

        if (!arrived)
        {
            _movement.ResetSpeedMultiplier();
            _timer = 0f;
            return;
        }

        _movement.SetSpeedMultiplier(_waitSpeedMultiplier);
        _timer += Time.deltaTime;

        if (_timer >= _duration)
        {
            _isFinished = true;
        }
    }

    private void PickNewMode()
    {
        _modeTimer = 0f;
        _modeDuration = Random.Range(_minModeTime, _maxModeTime);

        // Hold 빈도는 가중치로 조절 가능
        float r = Random.value;
        if (r < _shuffleModeValue) _probeMode = EEnemyProbeMode.Shuffle;
        else if (r < _feintModeValue) _probeMode = EEnemyProbeMode.Feint;
        else if (r < _repositionModeValue) _probeMode = EEnemyProbeMode.Reposition;
        else _probeMode = EEnemyProbeMode.Hold;
    }

    private Vector3 GetModeDestination()
    {
        Vector3 slotPosition = _slotCoordinator.GetSlotPosition(_mySlotIndex);

        switch (_probeMode)
        {
            case EEnemyProbeMode.Reposition:
                return slotPosition;

            case EEnemyProbeMode.Hold:
                // 제자리 유지
                return slotPosition;

            case EEnemyProbeMode.Shuffle:
                {
                    // 플레이어 중심 원호 이동
                    Vector3 toEnemy = (_enemy.position - _player.position);
                    toEnemy.y = 0f;
                    if (toEnemy.sqrMagnitude < _magnitudeThreshold) return slotPosition;

                    float sign = Random.value < _randomValue ? -1f : 1f;
                    Quaternion rot = Quaternion.Euler(0f, sign * _shuffleAngle, 0f);
                    Vector3 shuffledDir = rot * toEnemy.normalized;

                    Vector3 raw = _player.position + shuffledDir * toEnemy.magnitude;

                    if (NavMesh.SamplePosition(raw, out var hit, _navSampleRadius, NavMesh.AllAreas))
                    {
                        return hit.position;
                    }

                    return slotPosition;
                }

            case EEnemyProbeMode.Feint:
                {
                    // 플레이어 방향으로 살짝 전진 혹은 후퇴
                    Vector3 direction = (_player.position - _enemy.position);
                    direction.y = 0f;
                    if (direction.sqrMagnitude < _magnitudeThreshold) return slotPosition;
                    direction.Normalize();

                    float sign = Random.value < _randomValue ? 1f : -1f;
                    Vector3 raw = _enemy.position + direction * (_feintDistance * sign);

                    if (NavMesh.SamplePosition(raw, out var hit, _navSampleRadius, NavMesh.AllAreas))
                    {
                        return hit.position;
                    }

                    return slotPosition;
                }
        }

        return slotPosition;
    }

    public void Exit()
    {
        if (_releaseSlotOnExit)
        {
            ReleaseSlotNow();
        }

        _movement.ResetSpeedMultiplier();
    }

    public void ReleaseSlotNow()
    {
        if (_mySlotIndex >= 0)
        {
            _slotCoordinator.ReleaseSlot(_mySlotIndex);
            _mySlotIndex = -1;
        }
    }
}