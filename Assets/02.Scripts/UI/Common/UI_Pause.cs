using UnityEngine;
using UnityEngine.UI;

public class UI_Pause : BaseUI
{
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _controlButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _toLobbyButton;
    [SerializeField] private Button _exitGameButton;
    
    private void OnEnable()
    {
        if (SceneController.Instance.CurrentScene == ESceneType.SampleScene || SceneController.Instance.CurrentScene == ESceneType.TutorialScene)
        {
            _retryButton.gameObject.SetActive(true);
            _toLobbyButton.gameObject.SetActive(true);
            return;
        }
        _retryButton.gameObject.SetActive(false);
        _toLobbyButton.gameObject.SetActive(false);
    }

    public override void OnOpen()
    {

        base.OnOpen();
    }
    
    public override void OnClose()
    {
        base.OnClose();
    }
    
    public void Retry()
    {
        SceneController.Instance.ReloadCurrentScene();
    }
    
    public void OpenControls()
    {
        
    }

    public void OpenSettings()
    {
        _= UIController.Instance.OpenUI<UI_Settings>();
    }

    public void GotoLobby()
    {
        SceneController.Instance.LoadScene(ESceneType.LobbyScene);
    }

    public void ExitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}
