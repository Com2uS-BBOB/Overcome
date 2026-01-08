using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private Transform _player;
    private EnemyBase _enemy;
    private float _damage;
    private EnemyMovement _movement;
    private RushAttackHitbox _rushHitbox;
    private BiteAttackHitbox _swingHitbox;
    private Animator _animator;
    private EnemySlotCoordinator _slotCoordinator;

    private bool _hasRushedOnce;

    public bool HasRushedOnce => _hasRushedOnce;
    public void MarkRushed() => _hasRushedOnce = true;


    private IEnemyAttackPattern _currentPattern;

    private void Awake()
    {
        _enemy = GetComponent<EnemyBase>();
        _movement = GetComponent<EnemyMovement>();
        _rushHitbox = GetComponentInChildren<RushAttackHitbox>();
        _swingHitbox = GetComponentInChildren<BiteAttackHitbox>();
        _animator = GetComponent<Animator>();
    }

    public void Initialize(Transform player)
    {
        _player = player;
        _damage = _enemy.EnemyStatData.Damage;
        _slotCoordinator = player.GetComponent<EnemySlotCoordinator>();
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
        _hasRushedOnce = false;
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
                    _rushHitbox,
                    _swingHitbox,
                    this,
                    _animator,
                    _slotCoordinator
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

    // 이벤트 호출용 메서드
    public void OnBiteStart()
    {
        (_currentPattern as NormalAttackPattern)?.ForwardBiteStart();
    }

    public void OnBiteHitStart()
    {
        (_currentPattern as NormalAttackPattern)?.ForwardBiteHitStart();
    }

    public void OnBiteHitEnd()
    {
        (_currentPattern as NormalAttackPattern)?.ForwardBiteHitEnd();
    }

    public void OnBiteEnd()
    {
        (_currentPattern as NormalAttackPattern)?.ForwardBiteEnd();
    }
}