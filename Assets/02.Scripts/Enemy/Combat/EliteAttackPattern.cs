using UnityEngine;

public class EliteAttackPattern : MonoBehaviour
{
    private EEliteAttackPhase _phase;

    private bool _isFinished;

    public bool IsFinished => _isFinished;

    public void Update()
    {
        if (_isFinished) return;

        switch (_phase)
        {
            case EEliteAttackPhase.Rush:
                break;

            case EEliteAttackPhase.Wait:
                break;

            case EEliteAttackPhase.Rip:
                break;
        }
    }
}
