using System;
using UnityEngine;
using UnityEngine.AI;

public sealed class EnemyAttackPatternContext
{
    public Transform Player { get; }
    public Transform Enemy { get; }
    public float Damage { get; }

    public EnemyMovement Movement { get; }
    public EnemyAnimatorController Anim { get; }
    public NavMeshAgent Agent { get; }

    public EnemySlotCoordinator SlotCoordinator { get; }
    public EnemyAttackDirector AttackDirector { get; }

    public EnemyStatData StatData { get; }
    public EnemySfxSet SfxSet => StatData != null ? StatData.EnemySfxSet : null;

    public Func<EEnemyHitboxType, EnemyKnockbackHitbox> GetHitbox { get; }

    public EnemyAttackPatternContext(
        Transform player,
        Transform enemy,
        float damage,
        EnemyMovement movement,
        EnemyAnimatorController anim,
        NavMeshAgent agent,
        EnemySlotCoordinator slotCoordinator,
        EnemyAttackDirector attackDirector,
        EnemyStatData statData
    )
    {
        Player = player;
        Enemy = enemy;
        Damage = damage;
        Movement = movement;
        Anim = anim;
        Agent = agent;
        SlotCoordinator = slotCoordinator;
        AttackDirector = attackDirector;
        StatData = statData;
    }
}
