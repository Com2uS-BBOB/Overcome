using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private Transform _player;
    private EnemyBase _enemy;
    private float _damage;
    private EnemyMovement _movement;
    private EnemyKnockbackHitbox _knockbackHitbox;
    private Animator _animator;
    private EnemySlotCoordinator _slotCoordinator;
    private EnemyAttackDirector _attackDirector;

    private bool _hasRushedOnce;
    public bool HasRushedOnce => _hasRushedOnce;
    public void MarkRushed() => _hasRushedOnce = true;

    private IEnemyAttackPattern _currentPattern;

    private void Awake()
    {
        _enemy = GetComponent<EnemyBase>();
        _movement = GetComponent<EnemyMovement>();
        _knockbackHitbox = GetComponentInChildren<EnemyKnockbackHitbox>();
        _animator = GetComponent<Animator>();
    }

    public void Initialize(Transform player)
    {
        _player = player;
        _damage = _enemy.EnemyStatData.Damage;

        // ✅ “공격에 필요한” player 컴포넌트는 공격쪽에서 관리
        _slotCoordinator = player.GetComponent<EnemySlotCoordinator>();
        _attackDirector = player.GetComponent<EnemyAttackDirector>();
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
                    _knockbackHitbox,
                    _animator,
                    _slotCoordinator,
                    _attackDirector
                );
                break;

                // TODO: Elite, Small 확장
        }
    }

    // 애니메이션 이벤트 포워딩
    public void OnBiteStart() => (_currentPattern as NormalAttackPattern)?.ForwardBiteStart();
    public void OnBiteHitStart() => (_currentPattern as NormalAttackPattern)?.ForwardBiteHitStart();
    public void OnBiteHitEnd() => (_currentPattern as NormalAttackPattern)?.ForwardBiteHitEnd();
    public void OnBiteEnd() => (_currentPattern as NormalAttackPattern)?.ForwardBiteEnd();

    public void ResetRush()
    {
        _hasRushedOnce = false;
    }
}