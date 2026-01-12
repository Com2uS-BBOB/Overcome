using UnityEngine;

public class EnemyAnimatorController : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    static readonly int MoveHash = Animator.StringToHash("Move");
    static readonly int HowlHash = Animator.StringToHash("Howl");
    static readonly int AttackHash = Animator.StringToHash("Attack");
    static readonly int HitHash = Animator.StringToHash("Hit");
    static readonly int DeadHash = Animator.StringToHash("Dead");

    public void SetMove(bool isMoving) => _animator.SetBool(MoveHash, isMoving);

    public void PlayHowl()
    {
        ResetCombatTriggers();
        _animator.SetTrigger(HowlHash);
    }

    public void PlayAttack()
    {
        // Howl 중이면 공격 못하게 막고 싶으면 여기서 가드 가능
        _animator.SetTrigger(AttackHash);
    }

    public void PlayHit()
    {
        _animator.SetTrigger(HitHash);
    }

    public void PlayDead()
    {
        ResetAll();
        _animator.SetTrigger(DeadHash);
    }

    public void ResetCombatTriggers()
    {
        _animator.ResetTrigger(AttackHash);
        _animator.ResetTrigger(HowlHash);
    }

    public void ResetAll()
    {
        ResetCombatTriggers();
        _animator.ResetTrigger(HitHash);
        _animator.ResetTrigger(DeadHash);
        _animator.SetBool(MoveHash, false);
    }
}
