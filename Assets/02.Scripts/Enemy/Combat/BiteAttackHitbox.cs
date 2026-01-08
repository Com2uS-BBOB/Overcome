using UnityEngine;
using System.Collections;
using _02.Scripts.Player.Common;
using _02.Scripts.Player.Core;
using _02.Scripts.Player.Interfaces;

public class BiteAttackHitbox : HitboxBase
{
    [Header("넉백 옵션")]
    [SerializeField] private bool _useKnockback = true;
    [SerializeField] private float _knockbackDuration = 0.2f;
    [SerializeField] private float _totalDistance = 2f;

    private Coroutine _knockbackRoutine;

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

        // 적
        if (other.transform.root.GetComponent<EnemyBase>()) return true;

        return false;
    }

    protected override void OnHitSuccess(Collider other, IDamageable damageable)
    {
        // TODO: 히트 이펙트, 사운드, 경직 등
        Debug.Log($"휘두르기 피격: {other.name}");

        if (!_useKnockback) return;

        var player = other.GetComponent<PlayerController>();
        if (player == null) return;

        Vector3 direction = (transform.position - other.transform.position);
        direction.y = 0f;
        direction.Normalize();

        if (_knockbackRoutine != null)
        {
            player.StopCoroutine(_knockbackRoutine);
        }

        _knockbackRoutine = player.StartCoroutine(Knockback_Coroutine(player.CharacterController, direction));
    }

    private IEnumerator Knockback_Coroutine(CharacterController controller,Vector3 direction)
    {
        float elapsed = 0f;
        float movedDistance = 0f;

        while (elapsed < _knockbackDuration && movedDistance < _totalDistance)
        {
            float step = (_totalDistance / _knockbackDuration) * Time.deltaTime;
            Vector3 move = direction * step;

            Vector3 before = controller.transform.position;
            controller.Move(move);
            Vector3 after = controller.transform.position;

            movedDistance += Vector3.Distance(before, after);

            elapsed += Time.deltaTime;
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
