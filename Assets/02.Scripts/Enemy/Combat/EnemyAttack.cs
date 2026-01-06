using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private EnemyBase _enemy;
    private EnemyMovement _movement;
    private DashKnockbackHitbox _dashHitbox;
    private Transform _player;

    private IEnemyAttackPattern _currentPattern;

    private void Awake()
    {
        _enemy = GetComponent<EnemyBase>();
        _movement = GetComponent<EnemyMovement>();
        _dashHitbox = GetComponentInChildren<DashKnockbackHitbox>();
    }

    public void Initialize(Transform player)
    {
        _player = player;
    }

    public void StartAttack()
    {
        SelectPattern();
        _currentPattern?.Start();
    }

    public void UpdateAttack()
    {
        _currentPattern?.Update();
    }

    public void Stop()
    {
        _currentPattern?.Stop();
        _currentPattern = null;
    }

    private void SelectPattern()
    {
        switch (_enemy.EnemyStatData.EnemyType)
        {
            case EEnemyType.Normal:
                _currentPattern = new NormalAttackPattern(
                    transform,
                    _movement,
                    _dashHitbox,
                    _player
                );
                break;
        }
    }

    public bool IsAttackFinished => _currentPattern == null || _currentPattern.IsFinished;
}