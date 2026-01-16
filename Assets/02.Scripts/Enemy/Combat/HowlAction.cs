using UnityEngine;
using UnityEngine.AI;

public class HowlAction : IEnemyAction
{
    private readonly EnemyAnimatorController _anim;
    private readonly NavMeshAgent _agent;
    private readonly EnemyMovement _movement;
    private readonly float _duration;
    private readonly string _howlSfxKey;

    private float _timer;
    private bool _finished;

    private bool _sfxPlayed;

    public bool IsFinished => _finished;

    public HowlAction(
        EnemyAnimatorController anim, 
        NavMeshAgent agent, 
        EnemyMovement movement, 
        float duration, 
        string howlSfxKey)
    {
        _anim = anim;
        _agent = agent;
        _movement = movement;
        _duration = duration;
        _howlSfxKey = howlSfxKey;
    }

    public void Enter()
    {
        _finished = false;
        _timer = 0f;

        _sfxPlayed = false;

        _agent.isStopped = true;
        _agent.ResetPath();

        _movement.LockMovement(true, _duration);

        _anim.TryPlayHowl();
#if UNITY_EDITOR
        Debug.Log("포효 시작");
#endif
    }

    public void Update()
    {
        if (_finished) return;
        _timer += Time.deltaTime;
        if (_timer >= _duration)
        {
            _finished = true;
        }
    }

    public void Exit()
    {
        _movement.LockMovement(false);
    }

    public void OnSfxStart()
    {
        if (_sfxPlayed) return;
        _sfxPlayed = true;

        if (string.IsNullOrEmpty(_howlSfxKey)) return;

        // todo. 사운드 재생
        // SoundManager.Instance.PlaySfx(_howlSfxKey, _enemy.position);
    }
}
