using System;

public class HitEventManager : SingletonBehaviour<HitEventManager>
{
    protected override bool DontDestroy => false;

    /// <summary>
    /// 플레이어가 적에게 데미지를 입힐 때 발생
    /// </summary>
    public event Action<float> OnPlayerDealtDamage;

    public void NotifyDamageDealt(float damage)
    {
        OnPlayerDealtDamage?.Invoke(damage);
    }
}