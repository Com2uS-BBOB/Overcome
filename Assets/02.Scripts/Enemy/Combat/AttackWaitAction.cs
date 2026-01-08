using UnityEngine;

public class AttackWaitAction : IEnemyAction
{
    private readonly Transform _enemy;
    private readonly Transform _player;
    private readonly EnemyMovement _movement;

    private readonly float _minWait;
    private readonly float _maxWait;
    private readonly float _orbitRadius;
    private readonly float _orbitSpeed;
    private readonly float _aroundChance;
    private readonly float _aroundSpeedMultiplier;

    private float _timer;
    private float _duration;
    private float _orbitAngle;

    private bool _isAround;
    private bool _isFinished;

    public bool IsFinished => _isFinished;

    public AttackWaitAction(
        Transform enemy,
        Transform player,
        EnemyMovement movement,
        float minWait,
        float maxWait,
        float orbitRadius,
        float orbitSpeed,
        float aroundChance,
        float aroundSpeedMultiplier
    )
    {
        _enemy = enemy;
        _player = player;
        _movement = movement;
        _minWait = minWait;
        _maxWait = maxWait;
        _orbitRadius = orbitRadius;
        _orbitSpeed = orbitSpeed;
        _aroundChance = aroundChance;
        _aroundSpeedMultiplier = aroundSpeedMultiplier;
    }

    public void Enter()
    {
        _isFinished = false;
        _timer = 0f;
        _duration = Random.Range(_minWait, _maxWait);

        _movement.EnablePlayerSeparation(_player);
        _movement.SetRotationToLookAt(_player);

        Vector3 direction = _enemy.position - _player.position;
        direction.y = 0f;
        _orbitAngle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
    }

    public void Update()
    {
        _timer += Time.deltaTime;
        AroundPlayer();

        if (_timer >= _duration)
        {
            _isFinished = true;
        }
    }

    private void AroundPlayer()
    {
        _orbitAngle += _orbitSpeed * Time.deltaTime;
        float rad = _orbitAngle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * _orbitRadius;

        _movement.MoveTo(_player.position + offset);
    }

    public void Exit()
    {
        _movement.DisablePlayerSeparation();
        _movement.ResetSpeedMultiplier();
    }
}
