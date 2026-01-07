using UnityEngine;
using System;

public static class EnemyEventController
{
    public static class Enemy
    {
        public static event Action<EnemyKilledEvent> OnKilled;
        public static event Action<EnemyHitEvent> OnHit;
        public static event Action<EnemySpawnedEvent> OnSpawned;

        internal static void RaiseKilled(EnemyKilledEvent e)
            => OnKilled?.Invoke(e);

        internal static void RaiseHit(EnemyHitEvent e)
            => OnHit?.Invoke(e);

        internal static void RaiseSpawned(EnemySpawnedEvent e)
            => OnSpawned?.Invoke(e);
    }
}
