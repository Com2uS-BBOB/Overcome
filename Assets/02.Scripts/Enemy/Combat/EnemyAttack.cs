using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private Transform _player;
    private EnemyBase _enemy;
    private float _damage;
    private EnemyMovement _movement;
    private DashKnockbackHitbox _dashHitbox;
    private SwingAttackHitbox _swingHitbox;

    private bool _hasDashedOnce;

    public bool HasDashedOnce => _hasDashedOnce;
    public void MarkDashed() => _hasDashedOnce = true;


    private IEnemyAttackPattern _currentPattern;

    private void Awake()
    {
        _enemy = GetComponent<EnemyBase>();
        _damage = _enemy.EnemyStatData.Damage;
        _movement = GetComponent<EnemyMovement>();
        _dashHitbox = GetComponentInChildren<DashKnockbackHitbox>();
        _swingHitbox = GetComponentInChildren<SwingAttackHitbox>();
    }

    public void Initialize(Transform player)
    {
        _player = player;
    }

    public void StartAttack()
    {
        if (_currentPattern != null) return;

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
        _hasDashedOnce = false;
    }

    private void SelectPattern()
    {
        switch (_enemy.EnemyStatData.EnemyType)
        {
            case EEnemyType.Normal:
                _currentPattern = new NormalAttackPattern(
                    _player,
                    transform,
                    _damage,
                    _movement,
                    _dashHitbox,
                    _swingHitbox,
                    this
                );
                break;
        }
    }

    public bool IsAttackFinished => _currentPattern == null || _currentPattern.IsFinished;
}