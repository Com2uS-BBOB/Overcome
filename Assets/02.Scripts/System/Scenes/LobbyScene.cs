using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class LobbyScene : MonoBehaviour
{
    private struct PlayerRewardInfo
    {
        public string PlayerID;
        public ERewardType UnlockedRewardType;
    }
    public static Action CloseStageSelect;
    [SerializeField] private CanvasGroup _lobbyCanvasGroup;
    
    private static PlayerRewardInfo _rewardInfo;
    private static readonly int _rewardTypeCount = System.Enum.GetValues(typeof(ERewardType)).Length;

    
    private void Awake()
    {
        CloseStageSelect = OnCloseStageSelect;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        CheckNewRewards();
    }
    
    private async void CheckNewRewards()
    {
        // 새로운 유저인지 확인
        // 이전 유저와 다르면 보상 정보 갱신
        string currentPlayerID = PlayerDataManager.Instance.GetPlayerID();
        if (_rewardInfo.PlayerID != currentPlayerID)
        {
            // ERewardType을 순차 탐색하며 획득할 수 있는 가장 나중 Reward를 저장
            _rewardInfo.PlayerID = currentPlayerID;
            _rewardInfo.UnlockedRewardType = GetHighestUnlockedReward(ERewardType.None);
            return;
        }
        
        // 이전 유저와 같으면 이전 리워드와 비교해서 새로운 리워드를 얻었으면 popup 표시
        
        ERewardType prevUnlockedReward = _rewardInfo.UnlockedRewardType;
        _rewardInfo.UnlockedRewardType  = GetHighestUnlockedReward(prevUnlockedReward);
        if (_rewardInfo.UnlockedRewardType  == prevUnlockedReward) return;

        UI_RewardUnlockPopup popup = await UIController.Instance.OpenUI<UI_RewardUnlockPopup>();
        popup.SetRewardInfo(_rewardInfo.UnlockedRewardType);
    }

    private ERewardType GetHighestUnlockedReward(ERewardType prevRewardType)
    {
        RewardManager rewardManager = RewardManager.Instance;
        int startIndex = (int)prevRewardType + 1;
        ERewardType highRewardType = prevRewardType;
        for (var i = startIndex; i < _rewardTypeCount; i++)
        {
            ERewardType rewardType = (ERewardType)i;
            if (!rewardManager.IsRewardUnlocked(rewardType)) break;
            highRewardType = rewardType;
        }
        return highRewardType;
    }

    public void HandleGameStart(int chapter, int level)
    {
        StageManager.Instance.SetCurrentStage(chapter, level);
        LoadGameScene(chapter, level);
    }

    private void LoadGameScene(int chapter, int level)
    {
        ESceneType stageScene = GetStageEnum(chapter, level);
        SceneController.Instance.LoadSceneAsync(stageScene);
    }

    private ESceneType GetStageEnum(int chapter, int level)
    {
        string enumName = $"Stage{chapter}_{level}";
        if (Enum.TryParse(enumName, out ESceneType stageType))
        {
            return stageType;
        }
        return ESceneType.LobbyScene;
    }

    public void LoadTutorialScene()
    {
        SceneController.Instance.LoadSceneAsync(ESceneType.TutorialMap);
    }
    
    private void OnCloseStageSelect()
    {
        if (_lobbyCanvasGroup == null) return;
        _lobbyCanvasGroup.alpha = 1;
        _lobbyCanvasGroup.interactable = true;
    }

    public void OpenSettingUI()
    {
        _= UIController.Instance.OpenUI<UI_Settings>();
    }

    public void OpenExitUI()
    {
        _= UIController.Instance.OpenUI<UI_Exit>();
    }
}
