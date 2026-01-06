using UnityEngine;
using _02.Scripts.Player.Common;
using _02.Scripts.Player.Core;
using _02.Scripts.Player.Interfaces;

public class DashKnockbackHitbox : HitboxBase
{
    [Header("넉백 옵션")]
    [SerializeField] private float _knockbackPower = 6f;
    [SerializeField] private float _knockbackDuration = 0.15f;

    protected override bool ShouldIgnore(Collider other)
    {
        if (other.transform.root == transform.root) return true;

        return other.GetComponent<PlayerController>() == null;
    }

    protected override void OnHitSuccess(Collider other, IDamageable damageable)
    {
        var player = other.GetComponent<PlayerController>();
        if (player == null) return;

        Vector3 direction =
            (player.transform.position - transform.position).normalized;
        direction.y = 0f;

        ApplyKnockback(player, direction);
    }

    private void ApplyKnockback(PlayerController player, Vector3 direction)
    {
        var controller = player.CharacterController;
        if (controller == null) return;

        player.StartCoroutine(KnockbackCoroutine(controller, direction));
    }

    private System.Collections.IEnumerator KnockbackCoroutine(CharacterController controller,Vector3 direction)
    {
        float timer = 0f;

        while (timer < _knockbackDuration)
        {
            controller.Move(direction * _knockbackPower * Time.deltaTime);
            timer += Time.deltaTime;
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
