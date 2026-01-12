using UnityEngine;
using UnityEngine.AI;

public class HowlAction : IEnemyAction
{
    private readonly EnemyAnimatorController _anim;
    private readonly NavMeshAgent _agent;
    private readonly float _duration;

    private float _timer;
    private bool _finished;

    public bool IsFinished => _finished;

    public HowlAction(EnemyAnimatorController anim, NavMeshAgent agent, float duration)
    {
        _anim = anim;
        _agent = agent;
        _duration = duration;
    }

    public void Enter()
    {
        _finished = false;
        _timer = 0f;
        _agent.isStopped = true;
        _agent.ResetPath();

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

    public void Exit() { }
}
