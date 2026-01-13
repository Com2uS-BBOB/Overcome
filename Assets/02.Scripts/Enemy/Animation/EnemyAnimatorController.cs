using UnityEngine;

public class EnemyAnimatorController : MonoBehaviour
{
    [Header("애니메이터")]
    [SerializeField] private Animator _animator;

    [Header("없는 동작 호출 방지")]
    [SerializeField] private bool _supportsHowl = true;
    [SerializeField] private bool _supportsRush = true;
    [SerializeField] private bool _supportsAttack = true;
    [SerializeField] private bool _supportsHit = true;

    [Header("트리거 난입 방지")]
    [SerializeField] private bool _exclusiveCombat = true;  // Combat(Howl/Rush/Attack) 시작 시 기존 Combat 트리거를 리셋하고 하나만 발동
    [SerializeField] private bool _useCombatLock = true;  // 락 중에는 다른 Combat 트리거를 막기 (Hit/Death는 제외)
    [SerializeField] private bool _blockAttackRushWhileHowling = true;  // Howl 중에는 Rush/Attack 금지

    [SerializeField] private float _howlLockSeconds = 0.6f;  // Howl 시작 후 다른 Combat 막는 시간 (애니 이벤트로 해제할 거면 0)
    [SerializeField] private float _rushLockSeconds = 0.15f;  // Rush 시작 후 다른 Combat을 막는 시간
    [SerializeField] private float _attackLockSeconds = 0.15f;  // Attack 시작 후 다른 Combat을 막는 시간

    [Header("Hit / Death 속성")]
    [SerializeField] private bool _hitInterruptsCombat = true;  // Hit 우선 옵션

    [Header("Death Mode")]
    [SerializeField] private EEnemyDeathMode _deathMode = EEnemyDeathMode.Single;

    [Header("애니메이터 매개변수 명칭 (최대한 통일)")]
    [SerializeField] private string _moveBoolName = "Move";
    [SerializeField] private string _howlTriggerName = "Howl";
    [SerializeField] private string _rushTriggerName = "Rush";
    [SerializeField] private string _attackTriggerName = "Attack";
    [SerializeField] private string _hitTriggerName = "Hit";
    [SerializeField] private string _deathTriggerName = "Death";

    [Header("엘리트 적 난도질 공격 매개변수 명칭")]
    [SerializeField] private string _ripBoolName = "Rip";

    [Header("공중 적 Death 관련 매개변수 명칭")]
    [SerializeField] private string _fallingTriggerName = "Falling";
    [SerializeField] private string _onGroundTriggerName = "OnGround";

    // ---- animator hashes ----
    private int _moveHash;
    private int _howlHash;
    private int _rushHash;
    private int _attackHash;
    private int _hitHash;
    private int _deathHash;

    private int _ripHash;

    private int _fallingHash;
    private int _onGroundHash;

    // ---- state ----
    private bool _isDead;
    private EEnemyCombatAnim _currentCombat = EEnemyCombatAnim.None;
    private float _combatLockTimer;

    private bool IsCombatLocked => _useCombatLock && _combatLockTimer > 0f;

    private void Awake()
    {
        if (_animator == null) _animator = GetComponent<Animator>();

        _moveHash = Animator.StringToHash(_moveBoolName);
        _howlHash = Animator.StringToHash(_howlTriggerName);
        _rushHash = Animator.StringToHash(_rushTriggerName);
        _attackHash = Animator.StringToHash(_attackTriggerName);
        _hitHash = Animator.StringToHash(_hitTriggerName);
        _deathHash = Animator.StringToHash(_deathTriggerName);

        _ripHash = Animator.StringToHash(_ripBoolName);

        _fallingHash = Animator.StringToHash(_fallingTriggerName);
        _onGroundHash = Animator.StringToHash(_onGroundTriggerName);
    }

    private void Update()
    {
        if (_combatLockTimer > 0f)
        {
            _combatLockTimer -= Time.deltaTime;
        }
    }

    // 외부 호출용
    // 스폰/리스폰/풀에서 꺼낼 때 호출 추천

    // Death Mode 설정
    public void ConfigureDeathMode(EEnemyDeathMode mode)
    {
        _deathMode = mode;
    }

    public void ReviveReset()
    {
        _isDead = false;
        ResetAll();
    }

    public void SetMove(bool isMoving)
    {
        if (_isDead || _animator == null) return;
        _animator.SetBool(_moveHash, isMoving);
    }

    public void SetRip(bool isRipping)
    {
        if (!_isDead || _animator == null) return;

        if (isRipping && _exclusiveCombat)
        {
            ClearCombatTriggersOnly();
        }

        _animator.SetBool(_ripHash, isRipping);
    }

    public bool TryPlayHowl()
    {
        if (!_supportsHowl) return false;
        if (!CanEnterCombat(EEnemyCombatAnim.Howl)) return false;

        EnterCombat(EEnemyCombatAnim.Howl, _howlHash, _howlLockSeconds);
        return true;
    }

    public bool TryPlayRush()
    {
        if (!_supportsRush) return false;
        if (!CanEnterCombat(EEnemyCombatAnim.Rush)) return false;

        // howl 중 rush 금지 옵션
        if (_blockAttackRushWhileHowling && _currentCombat == EEnemyCombatAnim.Howl && IsCombatLocked)
        {
            return false;
        }

        EnterCombat(EEnemyCombatAnim.Rush, _rushHash, _rushLockSeconds);
        return true;
    }

    public bool TryPlayAttack()
    {
        if (!_supportsAttack) return false;
        if (!CanEnterCombat(EEnemyCombatAnim.Attack)) return false;

        // howl 중 attack 금지 옵션
        if (_blockAttackRushWhileHowling && _currentCombat == EEnemyCombatAnim.Howl && IsCombatLocked)
        {
            return false;
        }

        EnterCombat(EEnemyCombatAnim.Attack, _attackHash, _attackLockSeconds);
        return true;
    }

    // TakeDamage에서 호출
    public void PlayHit()
    {
        if (!_supportsHit) return;
        if (_isDead || _animator == null) return;

        if (_hitInterruptsCombat)
        {
            ClearCombat();
            _combatLockTimer = 0f;
        }

        _animator.SetTrigger(_hitHash);
    }

    // Die에서 호출 (무조건 우선)
    public void PlayDeath()
    {
        if (_animator == null) return;
        if (_isDead) return;

        _isDead = true;
        ResetAll(); // 트리거 또는 이동 정리

        if (_deathMode == EEnemyDeathMode.Single)
        {
            _animator.SetTrigger(_deathHash);
        }
        else
        {
            // 공중 소형 적은 첫 단계 Falling 시작
            _animator.SetTrigger(_fallingHash);
        }
    }

    // FlyingTwoStage에서 "땅에 닿았을 때" 호출
    public void PlayOnGroundDeath()
    {
        if (_animator == null) return;
        if (!_isDead) return;
        if (_deathMode != EEnemyDeathMode.FlyingTwoStage) return;

        // Falling에서 OnGround로 전환
        _animator.ResetTrigger(_fallingHash);
        _animator.SetTrigger(_onGroundHash);
    }

    // 애니메이션 이벤트로 락 해제하고 싶으면 클립 끝에 호출
    public void AnimEvent_CombatUnlock()
    {
        _combatLockTimer = 0f;
        _currentCombat = EEnemyCombatAnim.None;
    }


    // 내부 호출용
    private bool CanEnterCombat(EEnemyCombatAnim next)
    {
        if (_isDead || _animator == null) return false;
        if (IsCombatLocked) return false;
        return true;
    }

    private void EnterCombat(EEnemyCombatAnim combat, int triggerHash, float lockSeconds)
    {
        if (_exclusiveCombat)
            ClearCombatTriggersOnly();

        _currentCombat = combat;

        if (_useCombatLock && lockSeconds > 0f)
            _combatLockTimer = Mathf.Max(_combatLockTimer, lockSeconds);

        _animator.SetTrigger(triggerHash);
    }

    private void ClearCombat()
    {
        ClearCombatTriggersOnly();
        _currentCombat = EEnemyCombatAnim.None;
    }

    private void ClearCombatTriggersOnly()
    {
        if (_animator == null) return;

        if (_supportsHowl) _animator.ResetTrigger(_howlHash);
        if (_supportsRush) _animator.ResetTrigger(_rushHash);
        if (_supportsAttack) _animator.ResetTrigger(_attackHash);
    }

    public void ResetAll()
    {
        if (_animator == null) return;

        // Combat
        if (_supportsHowl) _animator.ResetTrigger(_howlHash);
        if (_supportsRush) _animator.ResetTrigger(_rushHash);
        if (_supportsAttack) _animator.ResetTrigger(_attackHash);

        // Hit / Death
        if (_supportsHit) _animator.ResetTrigger(_hitHash);
        _animator.ResetTrigger(_deathHash);

        // 공중 적 Death
        _animator.ResetTrigger(_fallingHash);
        _animator.ResetTrigger(_onGroundHash);

        // Move
        _animator.SetBool(_moveHash, false);

        // Rip
        _animator.SetBool(_ripHash, false);

        _currentCombat = EEnemyCombatAnim.None;
        _combatLockTimer = 0f;
    }
}