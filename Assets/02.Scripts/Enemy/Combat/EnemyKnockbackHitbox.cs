using _02.Scripts.Player.Common;
using _02.Scripts.Player.Core;
using _02.Scripts.Player.Interfaces;
using System.Collections;
using UnityEngine;

public class EnemyKnockbackHitbox : HitboxBase
{
    [Header("넉백 옵션")]
    [SerializeField] private bool _useKnockback = true;
    [SerializeField] private float _knockbackDuration = 0.2f;
    [SerializeField] private float _defaultKnockbackDistance = 2f;  // 기본값
    private float _currentKnockbackDistance;

    private Coroutine _knockbackRoutine;

    protected override void Awake()
    {
        base.Awake();
        DisableHitDetection();
        _currentKnockbackDistance = _defaultKnockbackDistance;
    }

    // 기본
    public void Enable(float damage)
    {
        Enable(damage, _defaultKnockbackDistance);
    }

    // 공격별 넉백 지정
    public void Enable(float damage, float knockbackDistance)
    {
        _currentKnockbackDistance = Mathf.Max(0f, knockbackDistance);
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
#if UNITY_EDITOR
        Debug.Log($"피격: {other.name}");
#endif

        if (!_useKnockback) return;

        var player = other.GetComponent<PlayerController>();
        if (player == null) return;

        Vector3 direction = (other.transform.position - transform.position);
        direction.y = 0f;
        direction.Normalize();

        if (_knockbackRoutine != null)
        {
            player.StopCoroutine(_knockbackRoutine);
        }

        _knockbackRoutine = player.StartCoroutine(Knockback_Coroutine(player.CharacterController, direction));
    }

    private IEnumerator Knockback_Coroutine(CharacterController controller, Vector3 direction)
    {
        float elapsed = 0f;
        float movedDistance = 0f;

        while (elapsed < _knockbackDuration && movedDistance < _currentKnockbackDistance)
        {
            float step = (_currentKnockbackDistance / _knockbackDuration) * Time.deltaTime;
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
