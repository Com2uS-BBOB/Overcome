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
    }
    
    public void HandleGameStart(int chapter, int level)
    {
        StageManager.Instance.SetCurrentStage(chapter, level);
        LoadGameScene(chapter, level);
    }

    private void LoadGameScene(int chapter, int level)
    {
        SceneController.Instance.LoadSceneAsync(ESceneType.GameScene);
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
