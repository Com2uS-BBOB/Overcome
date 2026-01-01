using UnityEngine;
using UnityEngine.InputSystem;

public class EnemyDamageTester : MonoBehaviour
{
    [SerializeField] private float _damage = 50f;
    [SerializeField] private float _maxDistance = 100f;

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

        if (Physics.Raycast(ray, out hit, _maxDistance))
        {
            EnemyBase enemy = hit.collider.GetComponentInParent<EnemyBase>();

            if (enemy != null)
            {
                enemy.TakeDamage(_damage);
                Debug.Log($"{enemy.name} 에게 {_damage} 데미지!");
            }
        }
    }
}