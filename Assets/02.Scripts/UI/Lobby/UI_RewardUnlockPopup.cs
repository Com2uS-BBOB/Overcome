using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_RewardUnlockPopup : BaseUI
{
    [SerializeField] private Image _rewardIcon;
    [SerializeField] private TextMeshProUGUI _rewardNameText;
    [SerializeField] private Button _closeButton;

    [Header("Animation")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _animationDuration = 0.5f;

    private ERewardType _currentReward;
    
    public void SetRewardInfo(ERewardType type)
    {
        _currentReward = type;
        DisplayCurrentReward();
    }
    
    private void DisplayCurrentReward()
    {
        RewardUnlockConfig config = RewardManager.Instance.GetRewardUnlockInfo(_currentReward);
        if (config == null) return;
        
        if (_rewardIcon != null)
        {
            _rewardIcon.sprite = config.RewardSprite;
        }
        
        if (_rewardNameText != null)
        {
            _rewardNameText.text = GetRewardName(_currentReward);
        }
    }

    private string GetRewardName(ERewardType rewardType)
    {
        return rewardType switch
        {
            ERewardType.CrescentSkill => "Crescent Skill",
            ERewardType.OverDriveSkill => "OverDrive Skill",
            ERewardType.MoveSpeedUp => "Move Speed Up",
            _ => rewardType.ToString()
        };
    }

    protected override void PlayOpenAnimation()
    {
        if (_canvasGroup == null) return;

        _canvasGroup.alpha = 0f;
        _canvasGroup.DOFade(1f, _animationDuration).SetEase(Ease.OutQuad);

        transform.localScale = Vector3.one * 0.8f;
        transform.DOScale(Vector3.one, _animationDuration).SetEase(Ease.OutBack);
    }

    protected override void PlayCloseAnimation()
    {
        if (_canvasGroup == null)
        {
            gameObject.SetActive(false);
            return;
        }

        _canvasGroup.DOFade(0f, _animationDuration)
                    .SetEase(Ease.InQuad);
        
        transform.DOScale(Vector3.one * 0.8f, _animationDuration)
                 .SetEase(Ease.InBack)
                 .OnComplete(() => gameObject.SetActive(false));
    }
}