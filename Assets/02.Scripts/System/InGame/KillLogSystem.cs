using System;
using System.Collections.Generic;
using UnityEngine;

public class KillLogSystem : SingletonBehaviour<KillLogSystem>, IGameSystem
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
    
    public void GameStart()
    {
        EnemyEventController.Enemy.OnKilled += LogKill;
    }

    public void GameEnd()
    {
        EnemyEventController.Enemy.OnKilled -= LogKill;
    }
    
    private void LogKill(EnemyKilledEvent killedEvent)
    {
        EEnemyType type = killedEvent.Enemy.EnemyType;
        if (type == EEnemyType.FloatSmall)
        {
            type = EEnemyType.Small;
        }
        _killLogs[type]++;
        var killLogConfig = new KillLogConfig
        {
            SkillName = "",
            DeathEnemy = type
        };

        OnKillLogged?.Invoke(killLogConfig);
    }
    public int GetKillCount(EEnemyType type)
    {
        return _killLogs.GetValueOrDefault(type, 0);
    }
}
