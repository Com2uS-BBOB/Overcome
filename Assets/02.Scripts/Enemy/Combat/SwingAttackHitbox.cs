using UnityEngine;
using _02.Scripts.Player.Common;
using _02.Scripts.Player.Interfaces;

public class SwingAttackHitbox : HitboxBase
{
    [Header("무시 대상")]
    [SerializeField] private LayerMask _ignoreLayers;

    protected override void Awake()
    {
        base.Awake();
        DisableHitDetection();
    }

    // 공격 패턴에서 호출
    public void Enable(float damage)
    {
        EnableHitDetection(damage);
    }

    public void Disable()
    {
        DisableHitDetection();
    }

    protected override bool ShouldIgnore(Collider other)
    {
        // 자기 자신
        if (other.transform.root == transform.root) return true;

        // 레이어 무시
        if ((_ignoreLayers.value & (1 << other.gameObject.layer)) != 0) return true;

        return false;
    }

    protected override void OnHitSuccess(Collider other, IDamageable damageable)
    {
        // TODO: 히트 이펙트, 사운드, 경직 등
        Debug.Log($"휘두르기 피격: {other.name}");
    }
}
