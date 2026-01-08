using UnityEngine;
using System.Collections;
using _02.Scripts.Player.Common;
using _02.Scripts.Player.Core;
using _02.Scripts.Player.Interfaces;

public class RushAttackHitbox : HitboxBase
{
    [Header("넉백 옵션")]
    [SerializeField] private float _knockbackDuration = 0.2f;
    [SerializeField] private float _totalDistance = 2f;

    private Coroutine _knockbackRoutine;

    protected override bool ShouldIgnore(Collider other)
    {
        if (other.transform.root == transform.root) return true;

        return other.GetComponent<PlayerController>() == null;
    }

    protected override void OnHitSuccess(Collider other, IDamageable damageable)
    {
        var player = other.GetComponent<PlayerController>();
        if (player == null) return;

        Vector3 direction = (player.transform.position - transform.position).normalized;
        direction.y = 0f;

        ApplyKnockback(player, direction);
    }

    private void ApplyKnockback(PlayerController player, Vector3 direction)
    {
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

    // 넉백 공격 데미지는 0으로 설정
    public void EnableKnockback()
    {
        EnableHitDetection(0f);
    }

    public void DisableKnockback()
    {
        DisableHitDetection();
    }
}
