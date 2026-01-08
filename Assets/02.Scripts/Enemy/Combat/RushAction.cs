using UnityEngine;

public class RushAction : IEnemyAction
{
    private readonly Transform _enemy;
    private readonly Transform _player;
    private readonly EnemyMovement _movement;
    private readonly RushAttackHitbox _hitbox;

    private readonly float _speed;
    private readonly float _duration;

    private float _timer;
    private Vector3 _direction;
    private bool _isFinished;

    public bool IsFinished => _isFinished;

    public RushAction(
        Transform enemy,
        Transform player,
        EnemyMovement movement,
        RushAttackHitbox hitbox,
        float speed,
        float duration
    )
    {
        _enemy = enemy;
        _player = player;
        _movement = movement;
        _hitbox = hitbox;
        _speed = speed;
        _duration = duration;
    }

    public void Enter()
    {
        _timer = 0f;
        _isFinished = false;

        _direction = (_player.position - _enemy.position).normalized;
        _direction.y = 0f;

        _movement.Stop();
        _enemy.rotation = Quaternion.LookRotation(_direction);

        _hitbox.EnableKnockback();
    }

    public void Update()
    {
        if (_isFinished) return;

        _timer += Time.deltaTime;
        _enemy.position += _direction * _speed * Time.deltaTime;

        if (_timer >= _duration)
        {
            _isFinished = true;
        }
    }

    public void Exit()
    {
        _hitbox.DisableKnockback();
    }
}
