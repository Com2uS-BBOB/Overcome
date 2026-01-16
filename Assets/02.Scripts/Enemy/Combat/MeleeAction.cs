using UnityEngine;
using UnityEngine.AI;

public class MeleeAction : IEnemyAction
{
    private readonly Transform _enemy;
    private readonly Transform _player;
    private readonly EnemyKnockbackHitbox _hitbox;
    private readonly EnemyAnimatorController _anim;
    private readonly NavMeshAgent _agent;
    private readonly float _damage;
    private readonly float _knockbackDistance;
    private readonly string _meleeSfxKey;

    private bool _sfxPlayed;

    private bool _isFinished;

    public bool IsFinished => _isFinished;

    public MeleeAction(
        Transform enemy,
        Transform player,
        EnemyKnockbackHitbox hitbox,
        EnemyAnimatorController anim,
        NavMeshAgent agent,
        float damage,
        float knockbackDistance,
        string meleeSfxKey
    )
    {
        _enemy = enemy;
        _player = player;
        _hitbox = hitbox;
        _anim = anim;
        _agent = agent;
        _damage = damage;
        _knockbackDistance = knockbackDistance;
        _meleeSfxKey = meleeSfxKey;
    }

    public void Enter()
    {
        _isFinished = false;

        _sfxPlayed = false;

        _agent.isStopped = true;
        _agent.ResetPath();

        Vector3 direction = _player.position - _enemy.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.01f)
        {
            _enemy.rotation = Quaternion.LookRotation(direction);
        }

        _anim.TryPlayAttack();
#if UNITY_EDITOR
        Debug.Log("근접 공격 시도");
#endif
    }

    public void Update() { }

    public void Exit()
    {
        if (_hitbox != null)
        {
            _hitbox.Disable();
        }
    }

    // 애니메이션 이벤트
    public void OnAnimStart() { }
    
    public void OnHitStart()
    {
        if (_hitbox != null)
        {
            _hitbox.Enable(_damage, _knockbackDistance);
        }
    }
    
    public void OnHitEnd()
    {
        if (_hitbox != null)
        {
            _hitbox.Disable();
        }
    }
    
    public void OnAnimEnd() => _isFinished = true;

    public void OnSfxStart()
    {
        if (_sfxPlayed) return;
        _sfxPlayed = true;

        if (string.IsNullOrEmpty(_meleeSfxKey)) return;

        // todo. 사운드 재생
        // SoundManager.Instance.PlaySfx(_meleeSfxKey, _enemy.position);
    }
}