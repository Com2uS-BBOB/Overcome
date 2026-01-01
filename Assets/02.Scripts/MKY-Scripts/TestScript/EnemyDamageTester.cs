using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyDamageTester : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    [SerializeField] private float maxDistance = 100f;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            ShootRay();
        }
    }

    void ShootRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            EnemyBase enemy = hit.collider.GetComponentInParent<EnemyBase>();

            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"{enemy.name} 에게 {damage} 데미지!");
            }
        }
    }
}