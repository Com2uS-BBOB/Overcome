using UnityEngine;

public class SmallEnemy : EnemyBase
{
    public override bool CanMove => false;
    public override bool CanAttack => false;
}
