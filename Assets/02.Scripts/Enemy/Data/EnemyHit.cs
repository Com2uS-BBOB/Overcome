using UnityEngine;

public class EnemyHit : MonoBehaviour
{
    private void OnEnable()
    {
        EnemyEventController.Enemy.OnHit += HandleHit;
    }

    private void OnDisable()
    {
        EnemyEventController.Enemy.OnHit -= HandleHit;
    }

    private void HandleHit(EnemyHitEvent e)
    {
        Debug.Log($"적 피격! {e.Enemy.name}, Damage: {e.Damage}");
    }
}
