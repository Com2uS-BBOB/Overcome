using UnityEngine;

public class SmallGroundEnemy : EnemyBase
{
    public override bool CanMove => false;
    public override bool CanAttack => false;
}
