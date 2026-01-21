using System;
using DG.Tweening;
using UnityEngine;

public class LobbyScene : MonoBehaviour
{
    public static Action CloseStageSelect;
    [SerializeField] private CanvasGroup _lobbyCanvasGroup;
    
    private void Awake()
    {
        CloseStageSelect = OnCloseStageSelect;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Time.timeScale = 1f;
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
        return ESceneType.GameScene;
    }

    public void LoadTutorialScene()
    {
        SceneController.Instance.LoadSceneAsync(ESceneType.TutorialScene);
    }
    
    private void OnCloseStageSelect()
    {
        if (_lobbyCanvasGroup == null) return;
        _lobbyCanvasGroup.alpha = 1;
        _lobbyCanvasGroup.interactable = true;
    }
}
