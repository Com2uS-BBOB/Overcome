using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_RewardInfo : MonoBehaviour
{
    [SerializeField] private ERewardType _rewardType;
    [SerializeField] private Image _rewardIIcon;
    [SerializeField] private Image _blockImage;
    [SerializeField] private TextMeshProUGUI _requiredStarText;

    private RewardManager _rewardManager;
    private RewardUnlockConfig _rewardUnlockConfig;

    private void Start()
    {
        _rewardManager = RewardManager.Instance;
        _rewardUnlockConfig = _rewardManager.GetRewardUnlockInfo(_rewardType);
        if (_rewardUnlockConfig == null)
        {
            enabled = false;
            return;
        }
        
        ResetInfo();
    }

    private void OnEnable()
    {
        ResetInfo();
    }
    
    private void ResetInfo()
    {
        SetRewardInfo();
        ShowClearImage();
    }
    
    private void SetRewardInfo()
    {
        if (_rewardUnlockConfig == null) return;
        _rewardIIcon.sprite = _rewardUnlockConfig.RewardSprite;
        _requiredStarText.SetText($"x {_rewardUnlockConfig.RequiredStars}");
    }
    private void ShowClearImage()
    {
        if (_rewardManager == null) return;
        _blockImage.gameObject.SetActive(!_rewardManager.IsRewardUnlocked(_rewardType));
    }
}
