using UnityEngine;

public class EnemyAnimationEventReceiver : MonoBehaviour
{
    private EnemyAttack _attack;

    private void Awake()
    {
        _attack = GetComponent<EnemyAttack>();
    }

    public void Anim_BiteHitStart()
    {
        _attack.OnBiteHitStart();
    }

    public void Anim_BiteHitEnd()
    {
        _attack.OnBiteHitEnd();
    }

    public void Anim_BiteEnd()
    {
        _attack.OnBiteEnd();
    }
}
