using System;
using System.Collections.Generic;
using UnityEngine;

public class KillLogSystem : SingletonBehaviour<KillLogSystem>
{
    protected override bool DontDestroy => false;

    public event Action<KillLogConfig> OnKillLogged;
    private readonly Dictionary<EEnemyType, int> _killLogs = new Dictionary<EEnemyType, int>();

    protected override void Init()
    {
        foreach (EEnemyType enemyType in Enum.GetValues(typeof(EEnemyType)))
        {
            _killLogs.TryAdd(enemyType, 0);
        }
    }
    
    private void OnEnable()
    {
        EnemyEventController.Enemy.OnKilled += LogKill;
    }

    private void OnDisable()
    {
        EnemyEventController.Enemy.OnKilled -= LogKill;
    }
    
    private void LogKill(EnemyKilledEvent killedEvent)
    {
        // todo. EnemyData를 받아 출력할 수 있게 수정 필요
        // todo. KillLog 기록 로직 추가
        _killLogs[killedEvent.Enemy.EnemyType]++;
        var killLogConfig = new KillLogConfig
        {
            SkillName = "",
            DeathEnemy = killedEvent.Enemy.EnemyType
        };

        OnKillLogged?.Invoke(killLogConfig);
    }
    public int GetKillCount(EEnemyType type)
    {
        return _killLogs.GetValueOrDefault(type, 0);
    }
}
