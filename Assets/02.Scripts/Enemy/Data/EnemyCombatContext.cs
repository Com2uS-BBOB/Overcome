using UnityEngine;

public sealed class EnemyCombatContext
{
    public Transform Player { get; }
    public EnemyAttackDirector AttackDirector { get; }
    public EnemySlotCoordinator SlotCoordinator { get; }

    public EnemyCombatContext(
        Transform player,
        EnemyAttackDirector attackDirector,
        EnemySlotCoordinator slotCoordinator)
    {
        Player = player;
        AttackDirector = attackDirector;
        SlotCoordinator = slotCoordinator;
    }
}
