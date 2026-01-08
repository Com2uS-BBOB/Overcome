using UnityEngine;

public class AttackWaitAction : IEnemyAction
{
    private readonly Transform _enemy;
    private readonly Transform _player;
    private readonly EnemyMovement _movement;

    private readonly float _minWait;
    private readonly float _maxWait;
    private readonly float _baseDistance;      // 플레이어와 기본 거리
    private readonly float _strafeRange;       // 좌우 이동 폭
    private readonly float _strafeChangeMin;
    private readonly float _strafeChangeMax;
    private readonly float _waitSpeedMultiplier;

    private float _timer;
    private float _duration;
    private float _playerFarDistance = 3f;

    private float _strafeTimer;
    private float _strafeDuration;
    private int _strafeDirection;

    private float _half = 0.5f;

    private float _toEnemyMinSqrDistance = 0.01f;

    private bool _isFinished;

    public bool IsFinished => _isFinished;

    public AttackWaitAction(
        Transform enemy,
        Transform player,
        EnemyMovement movement,
        float minWait,
        float maxWait,
        float baseDistance,
        float strafeRange,
        float strafeChangeMin,
        float strafeChangeMax,
        float waitSpeedMultiplier
    )
    {
        _enemy = enemy;
        _player = player;
        _movement = movement;

        _minWait = minWait;
        _maxWait = maxWait;
        _baseDistance = baseDistance;
        _strafeRange = strafeRange;
        _strafeChangeMin = strafeChangeMin;
        _strafeChangeMax = strafeChangeMax;
        _waitSpeedMultiplier = waitSpeedMultiplier;
    }

    public void Enter()
    {
        _isFinished = false;
        _timer = 0f;
        _duration = Random.Range(_minWait, _maxWait);

        _movement.EnablePlayerSeparation(_player);
        _movement.SetRotationToLookAt(_player);

        PickNewStrafe();
    }

    public void Update()
    {
        _timer += Time.deltaTime;
        _strafeTimer += Time.deltaTime;

        Vector3 difference = _enemy.position - _player.position;
        difference.y = 0f; // 수평 거리만 확인

        if (difference.sqrMagnitude >= _playerFarDistance * _playerFarDistance)
        {
            _movement.ResetSpeedMultiplier();
            _timer = 0f;
            _strafeTimer = 0f;
        }
        else
        {
            _movement.SetSpeedMultiplier(_waitSpeedMultiplier);
        }

        StrafeAroundPlayer();

        if (_strafeTimer >= _strafeDuration)
        {
            PickNewStrafe();
        }

        if (_timer >= _duration)
        {
            _isFinished = true;
        }
    }

    // 플레이어 근처 서성임 로직
    private void PickNewStrafe()
    {
        _strafeTimer = 0f;
        _strafeDuration = Random.Range(_strafeChangeMin, _strafeChangeMax);
        _strafeDirection = Random.value < _half ? -1 : 1;
    }

    private void StrafeAroundPlayer()
    {
        Vector3 toEnemy = _enemy.position - _player.position;
        toEnemy.y = 0f;

        if (toEnemy.sqrMagnitude < _toEnemyMinSqrDistance) return;

        toEnemy.Normalize();

        // 좌우 방향 (플레이어 기준)
        Vector3 lateral = Vector3.Cross(Vector3.up, toEnemy) * _strafeDirection;

        // 목표 위치 계산
        Vector3 targetPosition =_player.position + toEnemy * _baseDistance + lateral * _strafeRange;

        _movement.MoveTo(targetPosition);
    }

    public void Exit()
    {
        _movement.DisablePlayerSeparation();
        _movement.ResetSpeedMultiplier();
    }
}
