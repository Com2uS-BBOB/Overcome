using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using _02.Scripts.Player.Common;
using _02.Scripts.Player.Core;
using _02.Scripts.Player.Interfaces;

public class EnemyKnockbackHitbox : HitboxBase
{
    [Header("넉백 옵션")]
    [SerializeField] private bool _useKnockback = true;
    [SerializeField] private float _knockbackDuration = 0.2f;
    [SerializeField] private float _defaultKnockbackDistance = 2f;  // 기본값
    private float _currentKnockbackDistance;

    [Header("레이어 마스크")]
    [SerializeField] private LayerMask _enemyMask = 0;
    [SerializeField] private LayerMask _playerMask = 0;

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
        // 자기 자신 무시
        if (other.transform.root == transform.root)
        {
            return true;
        }

        // Enemy 레이어 무시
        if (((1 << other.gameObject.layer) & _enemyMask) != 0)
        {
            return true;
        }

        return false;
    }

    protected override void OnHitSuccess(Collider other, IDamageable damageable)
    {
        if (!_useKnockback) return;

        var player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        Vector3 direction = (player.transform.position - transform.position);
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f) return;
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
            _playerMask,
            QueryTriggerInteraction.Collide
        );

        // 플레이어 루트 기준 1회만 맞도록 하기
        var processedPlayers = new HashSet<PlayerController>();

        for (int i = 0; i < overlaps.Length; i++)
        {
            Collider collider = overlaps[i];
            if (collider == null || collider == myCollider)
            {
                continue;
            }
            if (ShouldIgnore(collider))
            {
                continue;
            }

            // 플레이어 계층인지 먼저 확정
            var player = collider.GetComponentInParent<PlayerController>();
            if (player == null)
            {
                continue;
            }

            // 플레이어 루트 기준 중복 방지
            if (!processedPlayers.Add(player))
            {
                continue;
            }

            // 플레이어의 IDamageable 자식에서 찾음
            IDamageable dmg = collider.GetComponent<IDamageable>() ??
                collider.GetComponentInParent<IDamageable>();

            if (dmg == null)
            {
                dmg = player.GetComponentInChildren<IDamageable>(includeInactive: true);
            }
            if (dmg == null)
            {
                continue;
            }

            dmg.TakeDamage(_damage, GetOwner());
            OnHitSuccess(collider, dmg);
        }
    }

    protected override GameObject GetOwner() => transform.root.gameObject;
}
