using System.Collections;
using _02.Scripts.Player.Combat;
using UnityEngine;
using UnityEngine.UI;

public class UI_CrescentSkill : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CrescentSkill _crescentSkill;
    [SerializeField] private Image _blockImage;
    [SerializeField] private Image _skillImage;
    
    [Header("스킬 색상")]
    [SerializeField] private Color _skillUnuseColor;
    [SerializeField] private Color _skillUseColor;

    [SerializeField] private float _resetDuration;
    Coroutine _resetImageCoroutine;
    
    private void Start()
    {
        bool canUse = RewardManager.Instance.IsCrescentUnlocked();
        if (!canUse)
        {
            _blockImage.gameObject.SetActive(true);
            return;
        }
        _crescentSkill.OnCrescentEndedUI += ActivateSkillUsingImage;
    }
    
    private void OnDestroy()
    {
        _crescentSkill.OnCrescentEndedUI -= ActivateSkillUsingImage;
    }

    private void ActivateSkillUsingImage()
    {
        _skillImage.color = _skillUseColor;
        if (_resetImageCoroutine != null)
        {
            StopCoroutine(_resetImageCoroutine);
        }
        _resetImageCoroutine = StartCoroutine(ResetImageCoroutine());
    }

    private IEnumerator ResetImageCoroutine()
    {
        yield return new WaitForSeconds(_resetDuration);
        _skillImage.color = _skillUnuseColor;
    }
}
