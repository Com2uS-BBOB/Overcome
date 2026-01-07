using UnityEngine;

public class NormalAttackPattern : IEnemyAttackPattern
{
    private readonly Transform _player;
    private readonly Transform _enemy;
    private readonly float _damage;
    private readonly EnemyMovement _movement;
    private readonly DashAttackHitbox _dashHitbox;
    private readonly BiteAttackHitbox _biteHitbox;
    private readonly EnemyAttack _attack;

    private ENormalAttackPhase _phase;

    [Header("돌진 공격 옵션")]
    [SerializeField] private float _dashSpeed = 20f;
    [SerializeField] private float _dashDuration = 0.44f;
    private float _dashTimer;
    private Vector3 _dashDirection;

    [Header("공격 대기 옵션")]
    [SerializeField] private float _minWaitDuration = 1f;
    [SerializeField] private float _maxWaitDuration = 3f;
    private float _waitTimer;
    private float _waitDuration;

    [Header("대기 중 맴돌기 옵션")]
    [SerializeField] private float _orbitRadius = 5f;
    [SerializeField] private float _orbitSpeed = 120f;
    private float _currentOrbitAngle;

    [Header("서성임 연출")]
    private float _aroundTimer;
    private float _aroundDuration;
    [SerializeField] private float _minAroundDuration = 0.3f;
    [SerializeField] private float _maxAroundDuration = 0.6f;
    [SerializeField] private float _aroundSpeedMultiplier = 0.6f;
    private float _aroundChance = 0.01f; // 프레임당 확률
    private bool _isAround;


    private bool _isFinished;

    public bool IsFinished => _isFinished;

    public NormalAttackPattern(
        Transform player,
        Transform enemy,
        float damage,
        EnemyMovement movement,
        DashAttackHitbox dashHitbox,
        BiteAttackHitbox biteHitbox,
        EnemyAttack attack
        )
    {
        _player = player;
        _enemy = enemy;
        _damage = damage;
        _movement = movement;
        _dashHitbox = dashHitbox;
        _biteHitbox = biteHitbox;
        _attack = attack;
    }

    public void Start()
    {
        _isFinished = false;
        if (_attack.HasDashedOnce)
        {
            EnterWait();
        }
        else
        {
            _attack.MarkDashed();
            EnterDash();
        }
    }

    public void Update()
    {
        if (_isFinished) return;

        switch (_phase)
        {
            case ENormalAttackPhase.Dash:
                UpdateDash();
                break;

            case ENormalAttackPhase.Wait:
                UpdateWait();
                break;

            case ENormalAttackPhase.Bite:
                UpdateBite();
                break;
        }
    }

    private void EnterDash()
    {
        _phase = ENormalAttackPhase.Dash;
        _dashTimer = 0f;

        _dashDirection = (_player.position - _enemy.position).normalized;
        _dashDirection.y = 0f;

        _movement.Stop();
        _enemy.rotation = Quaternion.LookRotation(_dashDirection);

        _dashHitbox.EnableKnockback();
    }

    private void UpdateDash()
    {
        // 플레이어가 공중이라면 돌진 실패
        // if (IsPlayerInDoubleJump())
        // {
        //     EnterWait();
        //     return;
        // }

        _dashTimer += Time.deltaTime;
        _enemy.position += _dashDirection * _dashSpeed * Time.deltaTime;

        if (_dashTimer >= _dashDuration)
        {
            EnterWait();
        }
    }

    // private bool IsPlayerInDoubleJump()
    // {
    //     var controller = _player.GetComponent<PlayerController>();
    //     return controller != null && controller.IsJumpingTwice;
    // }

    private void EnterWait()
    {
        _dashHitbox.DisableKnockback();

        _phase = ENormalAttackPhase.Wait;
        _waitTimer = 0f;
        _waitDuration = Random.Range(_minWaitDuration, _maxWaitDuration);

        // 플레이어 기준 거리 유지 ON
        _movement.EnablePlayerSeparation(_player);
        _movement.SetRotationToLookAt(_player);

        Vector3 direction = _enemy.position - _player.position;
        direction.y = 0f;
        _currentOrbitAngle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
    }

    private void UpdateWait()
    {
        _waitTimer += Time.deltaTime;

        if (_isAround)
        {
            _aroundTimer += Time.deltaTime;
            AroundPlayer();

            if (_aroundTimer >= _aroundDuration)
            {
                ExitAround();
            }
        }
        else
        {
            AroundPlayer();

            if (Random.value < _aroundChance)
            {
                EnterAround();
            }
        }

        if (_waitTimer >= _waitDuration)
        {
            _movement.DisablePlayerSeparation();
            _movement.ResetSpeedMultiplier();
            EnterBite();
        }
    }

    private void EnterAround()
    {
        _isAround = true;
        _aroundTimer = 0f;
        _aroundDuration = Random.Range(_minAroundDuration, _maxAroundDuration);

        _movement.SetSpeedMultiplier(_aroundSpeedMultiplier);
    }

    private void ExitAround()
    {
        _isAround = false;
        _movement.ResetSpeedMultiplier();
    }

    private void AroundPlayer()
    {
        _currentOrbitAngle += _orbitSpeed * Time.deltaTime;

        float rad = _currentOrbitAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * _orbitRadius;

        _movement.MoveTo(_player.position + offset);
    }

    private void EnterBite()
    {
        _phase = ENormalAttackPhase.Bite;

        _movement.Stop();
        _movement.DisablePlayerSeparation();

        Vector3 direction = _player.position - _enemy.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.01f)
        {
            _enemy.rotation = Quaternion.LookRotation(direction);
        }

        // animator.SetTrigger("Bite");
    }

    // 애니메이션 시작 프레임 때 호출
    private void OnBiteStart()
    {
        // 공격 판정 활성화
    }

    // 애니메이션 타격 시작 때 호출
    private void OnBiteHitStart()
    {
        _biteHitbox.Enable(_damage);
    }

    // 애니메이션 타격 끝날 때 호출
    private void OnBiteHitEnd()
    {
        _biteHitbox.Disable();
    }

    // 애니메이션 마지막 프레임 때 호출
    private void OnBiteEnd()
    {
        _biteHitbox.Disable();
        EnterWait();
    }

    private void UpdateBite()
    {
        // 대기 상태로 전환은 애니메이션 이벤트에서 처리
    }

    public void Stop()
    {
        _isFinished = true;

        _dashHitbox.DisableKnockback();
        _biteHitbox.Disable();
    }
}
