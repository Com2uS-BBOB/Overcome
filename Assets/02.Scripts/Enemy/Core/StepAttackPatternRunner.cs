using UnityEngine;
using System.Collections.Generic;

public class StepAttackPatternRunner : IEnemyAttackPattern
{
    private readonly List<IEnemyAttackStep> _steps = new();
    private readonly List<IEnemyAttackAlwaysStep> _always = new();

    private IEnemyAttackStep _current;

    public bool IsFinished => false;

    public StepAttackPatternRunner(IEnumerable<IEnemyAttackStep> steps, IEnumerable<IEnemyAttackAlwaysStep> alwaysSteps)
    {
        if (steps != null)
        {
            _steps.AddRange(steps);
        }
        if (alwaysSteps != null)
        {
            _always.AddRange(alwaysSteps);
        }
    }

    public void Start()
    {
        _current = null;

        for (int i = 0; i < _always.Count; i++)
        {
            _always[i].StartAlways();
        }

        // 시작 시점에 항상 실행되는 스텝이 필요하면 해당 스텝 내부에서 TryStart() 때 생성/Enter하도록 구성
    }

    public void Update()
    {
        // 항상 실행되는 스텝들
        for (int i = 0; i < _always.Count; i++)
        {
            if (_always[i].ShouldTickWhile(_current))
            {
                _always[i].TickAlways();
            }
        }

        // 현재 실행 중인 스텝
        if (_current != null)
        {
            _current.Tick();

            if (_current.IsFinished)
            {
                _current.Stop();
                _current = null;
            }
            return;
        }

        // 실행 중 스텝이 없으면 우선순위대로 시작 시도
        for (int i = 0; i < _steps.Count; i++)
        {
            if (_steps[i].TryStart())
            {
                _current = _steps[i];
                break;
            }
        }
        // 아무도 시작 못했으면 다음 프레임까지 대기
    }

    public void Stop()
    {
        if (_current != null)
        {
            _current.Stop();
            _current = null;
        }

        // 모든 스텝 정리
        for (int i = 0; i < _steps.Count; i++)
        {
            _steps[i].Stop();
        }
        for (int i = 0; i < _always.Count; i++)
        {
            _always[i].StopAlways();
        }
    }

    public void OnAnimEvent(EAttackAnimEvent animEvent)
    {
        _current?.OnAnimEvent(animEvent);
    }
}
