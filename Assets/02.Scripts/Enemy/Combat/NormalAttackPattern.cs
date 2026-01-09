using UnityEngine;
using UnityEngine.AI;

public class NormalAttackPattern : IEnemyAttackPattern
{
    // 공용 컨텍스트
    private readonly Transform _player;
    private readonly Transform _enemy;
    private readonly float _damage;
    private readonly EnemyMovement _movement;
    private readonly EnemyKnockbackHitbox _knockbackHitbox;
    private readonly Animator _animator;
    private readonly NavMeshAgent _agent;
    private readonly EnemySlotCoordinator _slotCoordinator;
    private readonly EnemyAttackDirector _attackDirector;

    private AttackWaitAction _pressure;
    private BiteAction _bite;

    private bool _reserved;
    private float _nextBiteTime;

    // 압박 유지 관련
    private readonly float _pressureWaitTime = 999f;
    private readonly float _pressureSpeed = 0.15f;

    // Bite 관련
    private readonly float _biteCooldownMin = 3f;
    private readonly float _biteCooldownMax = 5f;
    private readonly float _biteTouchDelay = 0.2f;

    private IEnemyAction _currentAction;

    public bool IsFinished => false;

    private readonly int _slotIndex;

    public NormalAttackPattern(
        Transform player,
        Transform enemy,
        float damage,
        EnemyMovement movement,
        EnemyKnockbackHitbox knockbackHitbox,
        Animator animator,
        EnemySlotCoordinator slotCoordinator,
        EnemyAttackDirector attackDirector
    )
    {
        _player = player;
        _enemy = enemy;
        _damage = damage;
        _movement = movement;
        _knockbackHitbox = knockbackHitbox;
        _animator = animator;
        _agent = enemy.GetComponent<NavMeshAgent>();
        _slotCoordinator = slotCoordinator;
        _attackDirector = attackDirector;
    }

    public void Start()
    {
        _reserved = false;
        _nextBiteTime = Time.time + Random.Range(_biteCooldownMin, _biteCooldownMax);

        // 압박 액션 생성 (슬롯 점유 포함)
        _pressure = new AttackWaitAction(
            _enemy,
            _player,
            _movement,
            _slotCoordinator,
            _agent,
            _pressureWaitTime, _pressureWaitTime,
            _pressureSpeed,
            releaseSlotOnExit: true,  // 패턴이 Stop될 때만 Exit 호출되고 슬롯 해제
            fixedSlotIndex: -1
        );
        _pressure.Enter();
    }

    public void Update()
    {
        if (_player == null) return;

        // 1) Bite 진행 중이면 Bite만 처리 (압박은 잠시 정지)
        if (_bite != null)
        {
            _bite.Update();

            if (_bite.IsFinished)
            {
                _bite.Exit();
                _bite = null;

                if (_reserved)
                {
                    _attackDirector?.Release(_enemy);
                    _reserved = false;
                }

                _nextBiteTime = Time.time + Random.Range(_biteCooldownMin, _biteCooldownMax);
            }
            return;
        }

        // 2) Bite 중이 아니면 압박은 항상 유지
        _pressure?.Update();

        // 3) Bite 시도 (공격권 필요)
        if (Time.time < _nextBiteTime) return;

        bool granted = (_attackDirector == null) || _attackDirector.TryReserve(_enemy);
        if (!granted)
        {
            _nextBiteTime = Time.time + _biteTouchDelay; // 너무 자주 두드리지 않게 살짝 딜레이
            return;
        }

        _reserved = (_attackDirector != null);

        _bite = new BiteAction(
            _enemy,
            _player,
            _knockbackHitbox,
            _animator,
            _agent,
            _damage
        );
        _bite.Enter();
    }

    public void Stop()
    {
        // Bite 정리
        if (_bite != null)
        {
            _bite.Exit();
            _bite = null;
        }

        // 공격권 반납 (Attack 상태에서 빠질 때)
        if (_reserved)
        {
            _attackDirector?.Release(_enemy);
            _reserved = false;
        }

        // 압박 정리 + 슬롯 해제
        _pressure?.Exit();
        _pressure = null;
    }

    // 애니메이션 이벤트 포워딩
    public void OnAnimEvent(EAttackAnimEvent animEvent)
    {
        // BiteAction이 없으면 이벤트 무시
        if (_bite == null) return;

        switch (animEvent)
        {
            case EAttackAnimEvent.BiteStart:
                _bite.OnAnimStart();
                break;

            case EAttackAnimEvent.BiteHitStart:
                _bite.OnHitStart();
                break;

            case EAttackAnimEvent.BiteHitEnd:
                _bite.OnHitEnd();
                break;

            case EAttackAnimEvent.BiteEnd:
                _bite.OnAnimEnd();
                break;
        }
    }

    public void ForwardBiteStart() => _bite?.OnAnimStart();
    public void ForwardBiteHitStart() => _bite?.OnHitStart();
    public void ForwardBiteHitEnd() => _bite?.OnHitEnd();
    public void ForwardBiteEnd() => _bite?.OnAnimEnd();
}
