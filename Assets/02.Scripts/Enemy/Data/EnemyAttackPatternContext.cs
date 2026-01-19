using UnityEngine;
using UnityEngine.AI;

public sealed class EnemyAttackPatternContext
{
    public Transform Player { get; }
    public Transform Enemy { get; }

    public EnemyMovement Movement { get; }
    public EnemyKnockbackHitbox KnockbackHitbox { get; }
    public EnemyAnimatorController Anim { get; }
    public NavMeshAgent Agent { get; }

    public EnemySlotCoordinator SlotCoordinator { get; }
    public EnemyAttackDirector AttackDirector { get; }

    public float Damage { get; }

    public EnemyAttackPatternContext(
        Transform player,
        Transform enemy,
        float damage,
        EnemyMovement movement,
        EnemyKnockbackHitbox knockbackHitbox,
        EnemyAnimatorController anim,
        NavMeshAgent agent,
        EnemySlotCoordinator slotCoordinator,
        EnemyAttackDirector attackDirector
    )
    {
        Player = player;
        Enemy = enemy;
        Damage = damage;
        Movement = movement;
        KnockbackHitbox = knockbackHitbox;
        Anim = anim;
        Agent = agent;
        SlotCoordinator = slotCoordinator;
        AttackDirector = attackDirector;
    }
}
