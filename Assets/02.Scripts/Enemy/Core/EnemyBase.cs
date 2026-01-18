using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;
using _02.Scripts.Player.Interfaces;

public abstract class EnemyBase : MonoBehaviour, IDamageable
{
    public EEnemyType EnemyType { get; private set; }

    public EnemyStatData EnemyStatData { get; private set; }

    private EnemyAnimatorController _anim;
    private Collider _collider;

    protected float _currentHealth;

    [Header("스폰 높이")]
    [SerializeField] private float _spawnHeight = 0f;  // 바닥과 적의 중심 높이 차이

    [Header("스폰 텀")]
    [SerializeField] private float _spawnGapTime = 0.6f;

    private Coroutine _spawnGapRoutine;
    public bool IsSpawnGap { get; private set; }

    private float _hitStopTime = 0.24f;  // Hit 시 적이 멈춰있는 시간
    private Coroutine _hitStopRoutine;

    public virtual bool CanMove => !IsSpawnGap;
    public virtual bool CanReturn => !IsSpawnGap;
    public virtual bool CanAttack => true;

    private EnemyPool _pool;
    private IEnemyDespawnHandler _despawnHandler;

    private EnemyMovement _movement;
    private NavMeshAgent _agent;

    private Vector3 _spawnBasePosition;  // 최초 스폰 위치 저장용 (리스폰 때 사용)

    private bool _despawnRequested;

    public float CurrentHp => _currentHealth;
    public float MaxHp => EnemyStatData.MaxHealth;
    public bool IsDead => _currentHealth <= 0;

    public int Score => EnemyStatData.Score;
    public int Playtime => EnemyStatData.Playtime;

    public event Action<float, float> OnHpChanged;
    public event Action OnDeath;
    public event Action<EnemyBase> OnDespawn;

    protected virtual void Awake()
    {
        _anim = GetComponent<EnemyAnimatorController>();
        _movement = GetComponent<EnemyMovement>();
        _agent = GetComponent<NavMeshAgent>();
        _collider = GetComponent<Collider>();
    }

    public void Initialize(EnemyStatData statData)
    {
        if (statData == null)
        {
#if UNITY_EDITOR
            Debug.LogError($"[EnemyBase] Initialize: statData가 null입니다! EnemyType: {EnemyType}");
#endif
            return;
        }

        EnemyStatData = statData;
        _currentHealth = EnemyStatData.MaxHealth;

        if (_anim == null) _anim = GetComponent<EnemyAnimatorController>();
        _anim?.ConfigureDeathMode(statData.EnemyDeathMode);
    }

    protected virtual void OnEnable()
    {
        if (EnemyStatData == null) return;

        _collider.enabled = true;
        _despawnRequested = false;

        _currentHealth = EnemyStatData.MaxHealth;
        OnHpChanged?.Invoke(_currentHealth, MaxHp);

        _anim?.ReviveReset();
        _movement?.ForceUnlockAll();

        BeginSpawnGap();
    }

    protected virtual void OnDisable()
    {
        // 코루틴 정리
        if (_hitStopRoutine != null)
        {
            StopCoroutine(_hitStopRoutine);
            _hitStopRoutine = null;
        }

        if (_spawnGapRoutine != null)
        {
            StopCoroutine(_spawnGapRoutine);
            _spawnGapRoutine = null;
        }

        IsSpawnGap = false;
    }

    public void SetPool(EnemyPool pool)
    {
        _pool = pool;
    }

    public void SetEnemyType(EEnemyType type)
    {
        EnemyType = type;
    }

    public void SetDespawnHandler(IEnemyDespawnHandler handler)
    {
        _despawnHandler = handler;
    }

    // 최초 스폰 위치 저장 (리스폰용)
    public void SetSpawnBasePosition(Vector3 basePosition)
    {
        _spawnBasePosition = basePosition;
    }

    // 리스폰 위치 반환 + 높이 보정
    public Vector3 GetSpawnBasePosition()
    {
        return _spawnBasePosition;
    }
    public float GetSpawnHeight()
    {
        return _spawnHeight;
    }

    #region Spawn Gap

    private void BeginSpawnGap()
    {
        // 풀링 중복 방지
        if (_spawnGapRoutine != null)
        {
            StopCoroutine(_spawnGapRoutine);
            _spawnGapRoutine = null;
        }

        IsSpawnGap = true;

        // 이동/공격을 확실히 끊기 (안전 장치)
        _movement?.Stop();
        _movement?.LockMovement(true, _spawnGapTime);

        //todo. 스폰 이펙트 자리

        _spawnGapRoutine = StartCoroutine(SpawnGap_Coroutine());
    }

    private IEnumerator SpawnGap_Coroutine()
    {
        yield return new WaitForSeconds(_spawnGapTime);

        // 죽었거나 비활성화면 해제하지 않음
        if (!gameObject.activeInHierarchy) yield break;
        if (IsDead) yield break;

        IsSpawnGap = false;
        _movement?.LockMovement(false);
    }

    #endregion

    #region Hit

    public virtual void TakeDamage(float damage, GameObject attacker = null)
    {
        if (IsDead) return;
        if (IsSpawnGap) return;

        _currentHealth -= damage;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        EnemyEventController.Enemy.RaiseHit(new EnemyHitEvent(this, damage, attacker));
        OnHpChanged?.Invoke(_currentHealth, MaxHp);

        StopOnHit(_hitStopTime);
        _anim?.PlayHit();

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void StopOnHit(float time)
    {
        if (_movement == null) return;

        if (_hitStopRoutine != null)
        {
            StopCoroutine(_hitStopRoutine);
        }
        _hitStopRoutine = StartCoroutine(HitStop_Coroutine(time));
    }

    private IEnumerator HitStop_Coroutine(float time)
    {
        _movement.LockMovement(true, time);
        yield return new WaitForSeconds(time);

        // 죽었다면 풀면 안 됨
        if (!IsDead)
        {
            _movement.LockMovement(false);
        }
    }

    #endregion

    #region Die and Despawn

    protected virtual void Die()
    {
        if (_despawnRequested) return;
        _despawnRequested = true;

        _movement?.FullStop ();
        _collider.enabled = false;

#if UNITY_EDITOR
        Debug.Log($"적이 죽었습니다. EnemyType: {EnemyType}");
#endif

        EnemyEventController.Enemy.RaiseKilled(new EnemyKilledEvent(this));
        OnDeath?.Invoke();

        _anim?.PlayDeath();
    }

    // 애니 이벤트에서 호출 (Death / OnGround 클립 끝에 넣기)
    public void AnimEvent_Despawn()
    {
        DoDespawn();
    }

    private void DoDespawn()
    {
        if (!_despawnRequested) return; // 혹시 모를 중복 방지

        _despawnRequested = false;

        _despawnHandler?.HandleDespawn(this);  // 스포너 / 풀링 처리 요청
        OnDespawn?.Invoke(this);
    }

    #endregion
}