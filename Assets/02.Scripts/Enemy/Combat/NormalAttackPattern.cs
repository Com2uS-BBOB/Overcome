using UnityEngine;

public class NormalAttackPattern : IEnemyAttackPattern
{
    private readonly Transform _player;
    private readonly Transform _enemy;
    private readonly float _damage;
    private readonly EnemyMovement _movement;
    private readonly DashKnockbackHitbox _dashHitbox;
    private readonly SwingAttackHitbox _swingHitbox;
    private readonly EnemyAttack _attack;

    private ENormalAttackPhase _phase;

    [Header("돌진 공격 옵션")]
    private float _dashSpeed = 14f;
    private float _dashDuration = 0.8f;
    private float _dashTimer;
    private Vector3 _dashDirection;

    [Header("공격 대기 옵션")]
    private float _minWaitDuration = 1f;
    private float _maxWaitDuration = 3f;
    private float _waitTimer;
    private float _waitDuration;

    [Header("휘두르기 공격 옵션")]
    private float _swingDuration = 1.5f;
    private float _swingTimer;
    private int _swingCount;
    private int _maxSwingCount = 50;

    private bool _isFinished;

    public bool IsFinished => _isFinished;

    public NormalAttackPattern(
        Transform player,
        Transform enemy,
        float damage,
        EnemyMovement movement,
        DashKnockbackHitbox dashHitbox,
        SwingAttackHitbox swingHitbox,
        EnemyAttack attack
        )
    {
        _player = player;
        _enemy = enemy;
        _damage = damage;
        _movement = movement;
        _dashHitbox = dashHitbox;
        _swingHitbox = swingHitbox;
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

            case ENormalAttackPhase.Swing:
                UpdateSwing();
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
    }

    private void UpdateWait()
    {
        _waitTimer += Time.deltaTime;

        if (_waitTimer >= _waitDuration)
        {
            EnterSwing();
        }
    }
    private void EnterSwing()
    {
        _phase = ENormalAttackPhase.Swing;
        _swingTimer = 0f;

        // animator.SetTrigger("Swing");
        OnSwingStart();
    }

    // 애니메이션 시작 프레임 때 호출
    private void OnSwingStart()
    {
        // 공격 판정 활성화
    }

    private void OnSwingHitStart()
    {
        _swingHitbox.Enable(_damage);
    }

    private void OnSwingHitEnd()
    {
        _swingHitbox.Disable();
    }

    private void OnSwingEnd()
    {
        _swingHitbox.Disable();
    }

    private void UpdateSwing()
    {
        _swingTimer += Time.deltaTime;

        // 공격 판정은 애니메이션 이벤트로 처리. 일단 임시로 타격 프레임
        if (_swingTimer >= 0.3f && _swingTimer <= 0.6f)
        {
            OnSwingHitStart();
        }
        else
        {
            OnSwingHitEnd();
        }

        if (_swingTimer >= _swingDuration)
        {
            OnSwingEnd();
            _swingCount++;

            if (_swingCount >= _maxSwingCount)
            {
                _isFinished = true;
            }
            else
            {
                EnterWait();
            }
        }
    }

    public void Stop()
    {
        _isFinished = true;

        _dashHitbox.DisableKnockback();
        _swingHitbox.Disable();
    }
}
