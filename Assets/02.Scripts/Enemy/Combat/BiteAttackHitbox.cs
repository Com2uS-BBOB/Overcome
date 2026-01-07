using UnityEngine;
using System.Collections;
using _02.Scripts.Player.Common;
using _02.Scripts.Player.Core;
using _02.Scripts.Player.Interfaces;

public class BiteAttackHitbox : HitboxBase
{
    [Header("넉백 옵션")]
    [SerializeField] private bool _useKnockback = true;
    [SerializeField] private float _knockbackPower = 8f;
    [SerializeField] private float _knockbackDuration = 0.2f;

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

        if (other.GetComponent<PlayerController>() == null) return true;

        return false;
    }

    protected override void OnHitSuccess(Collider other, IDamageable damageable)
    {
        // TODO: 히트 이펙트, 사운드, 경직 등
        Debug.Log($"휘두르기 피격: {other.name}");

        if (!_useKnockback) return;

        var player = other.GetComponent<PlayerController>();
        if (player == null) return;

        Vector3 direction = (other.transform.position - transform.position);
        direction.y = 0f;
        direction.Normalize();

        player.StartCoroutine(ApplyKnockback_Coroutine(player.CharacterController, direction));
    }

    private IEnumerator ApplyKnockback_Coroutine(CharacterController controller,Vector3 direction)
    {
        float timer = 0f;

        while (timer < _knockbackDuration)
        {
            controller.Move(direction * _knockbackPower * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    public override void EnableHitDetection(float damage)
    {
        _damage = damage;
        _isActive = true;
        _hitTargets.Clear();

        Collider myCollider = GetComponent<Collider>();

        Collider[] overlaps = Physics.OverlapBox(
            myCollider.bounds.center,
            myCollider.bounds.extents,
            myCollider.transform.rotation,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        foreach (var collider in overlaps)
        {
            if (collider == myCollider) continue;
            ProcessHit(collider);
        }
    }
}
