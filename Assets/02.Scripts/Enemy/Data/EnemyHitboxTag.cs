using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHitboxTag : MonoBehaviour
{
    [SerializeField] private EEnemyHitboxType _type;
    public EEnemyHitboxType Type => _type;
}
