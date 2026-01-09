using UnityEngine;
using UnityEngine.AI;

public sealed class EnemyAttackPatternContext
{
    public Transform Player { get; }
    public Transform Enemy { get; }

    public EnemyMovement Movement { get; }
    public EnemyKnockbackHitbox KnockbackHitbox { get; }
    public Animator Animator { get; }
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
        Animator animator,
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
        Animator = animator;
        Agent = agent;
        SlotCoordinator = slotCoordinator;
        AttackDirector = attackDirector;
    }
}
