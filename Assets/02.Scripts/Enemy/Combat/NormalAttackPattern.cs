using UnityEngine;

public class NormalAttackPattern : IEnemyAttackPattern
{
    // 공용 컨텍스트
    private readonly EnemyAttackPatternContext _attackContext;
    private readonly NormalAttackPatternConfig _attackConfig;

    private AttackWaitAction _pressure;
    private BiteAction _bite;

    private bool _reserved;
    private float _nextBiteTime;

    // Bite 관련
    private readonly float _biteCooldownMin = 3f;
    private readonly float _biteCooldownMax = 5f;
    private readonly float _biteTouchDelay = 0.2f;

    private IEnemyAction _currentAction;

    public bool IsFinished => false;

    private readonly int _slotIndex;

    public NormalAttackPattern(
        EnemyAttackPatternContext context,
        NormalAttackPatternConfig config
    )
    {
        _attackContext = context;
        _attackConfig = config;
    }

    public void Start()
    {
        _reserved = false;
        _nextBiteTime = Time.time + Random.Range(_biteCooldownMin, _biteCooldownMax);

        // 압박 액션 생성 (슬롯 점유 포함)
        _pressure = new AttackWaitAction(
            _attackContext.Enemy,
            _attackContext.Player,
            _attackContext.Movement,
            _attackContext.SlotCoordinator,
            _attackContext.Agent,
            _attackConfig.PressureWaitConfig
        );
        _pressure.Enter();
    }

    public void Update()
    {
        if (_attackContext.Player == null) return;

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
                    _attackContext.AttackDirector?.Release(_attackContext.Enemy);
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

        bool granted = (_attackContext.AttackDirector == null) || _attackContext.AttackDirector.TryReserve(_attackContext.Enemy);
        if (!granted)
        {
            _nextBiteTime = Time.time + _biteTouchDelay; // 너무 자주 두드리지 않게 살짝 딜레이
            return;
        }

        _reserved = (_attackContext.AttackDirector != null);

        _bite = new BiteAction(
            _attackContext.Enemy,
            _attackContext.Player,
            _attackContext.KnockbackHitbox,
            _attackContext.Animator,
            _attackContext.Agent,
            _attackContext.Damage
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
            _attackContext.AttackDirector?.Release(_attackContext.Enemy);
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
