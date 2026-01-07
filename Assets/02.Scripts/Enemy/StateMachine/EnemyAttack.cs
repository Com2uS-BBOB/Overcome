using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private Transform _player;
    private EnemyBase _enemy;
    private float _damage;
    private EnemyMovement _movement;
    private DashAttackHitbox _dashHitbox;
    private BiteAttackHitbox _swingHitbox;
    private  Animator _animator;

    private bool _hasDashedOnce;

    public bool HasDashedOnce => _hasDashedOnce;
    public void MarkDashed() => _hasDashedOnce = true;


    private IEnemyAttackPattern _currentPattern;

    private void Awake()
    {
        _enemy = GetComponent<EnemyBase>();
        _damage = _enemy.EnemyStatData.Damage;
        _movement = GetComponent<EnemyMovement>();
        _dashHitbox = GetComponentInChildren<DashAttackHitbox>();
        _swingHitbox = GetComponentInChildren<BiteAttackHitbox>();
        _animator = GetComponent<Animator>();
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
                    this,
                    _animator
                );
                break;
        }
    }

    public bool IsAttackFinished => _currentPattern == null || _currentPattern.IsFinished;

    public void RestartAttackIfNeeded()
    {
        if (_currentPattern == null || _currentPattern.IsFinished)
        {
            SelectPattern();
            _currentPattern?.Start();
        }
    }

    public void Anim_OnBiteStart()
    {
        (_currentPattern as NormalAttackPattern)?.OnBiteStart();
    }

    public void Anim_OnBiteHitStart()
    {
        (_currentPattern as NormalAttackPattern)?.OnBiteHitStart();
    }

    public void Anim_OnBiteHitEnd()
    {
        (_currentPattern as NormalAttackPattern)?.OnBiteHitEnd();
    }

    public void Anim_OnBiteEnd()
    {
        (_currentPattern as NormalAttackPattern)?.OnBiteEnd();
    }
}