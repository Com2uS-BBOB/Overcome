using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_RewardInfo : MonoBehaviour
{
    [SerializeField] private ERewardType _rewardType;
    [SerializeField] private Image _rewardIIcon;
    [SerializeField] private Image _clearImage;
    [SerializeField] private TextMeshProUGUI _requiredStarText;

    private PlayerDataManager _dataManager;
    private RewardUnlockConfig _rewardUnlockConfig;

    private void Start()
    {
        _dataManager = PlayerDataManager.Instance;
        _rewardUnlockConfig = _dataManager.GetRewardUnlockInfo(_rewardType);
        if (_rewardUnlockConfig == null)
        {
            enabled = false;
            return;
        }
        
        ResetInfo();
    }
    
    private void ResetInfo()
    {
        SetRewardInfo();
        ShowClearImage();
    }
    
    private void SetRewardInfo()
    {
        _rewardIIcon.sprite = _rewardUnlockConfig.RewardSprite;
        _requiredStarText.SetText($"x {_rewardUnlockConfig.RequiredStars}");
    }
    private void ShowClearImage()
    {
        if (!_dataManager.IsRewardUnlocked(_rewardType)) return;
        _clearImage.gameObject.SetActive(true);
    }
}
