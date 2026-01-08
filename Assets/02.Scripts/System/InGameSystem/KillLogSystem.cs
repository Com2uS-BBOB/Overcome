using System;
using System.Collections.Generic;
using UnityEngine;

public class KillLogSystem : SingletonBehaviour<KillLogSystem>
{
    protected override bool DontDestroy => false;

    public event Action<KillLogConfig> OnKillLogged;
    private Dictionary<EEnemyType, int> _killLogs;
    
    public void LogKill()
    {
        // todo. EnemyData를 받아 출력할 수 있게 수정 필요
        // todo. _killLogs에 죽인 몬스터 수를 기록하는 코드 추가
        var killLogConfig = new KillLogConfig
        {
            SkillName = "",
            DeathEnemy = EEnemyType.Normal
        };

        OnKillLogged?.Invoke(killLogConfig);
    }
}
