using UnityEngine;
using UnityEngine.UI;

public class UI_Pause : BaseUI
{
    [SerializeField] private Button _retryButton;
    [SerializeField] private Button _controlButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _toLobbyButton;
    [SerializeField] private Button _toTitleButton;
    [SerializeField] private Button _exitGameButton;

    private void OnEnable()
    {
        if (SceneController.Instance.CurrentScene == ESceneType.SampleScene || SceneController.Instance.CurrentScene == ESceneType.TutorialScene)
        {
            _retryButton.gameObject.SetActive(true);
            _toLobbyButton.gameObject.SetActive(true);
            _toTitleButton.gameObject.SetActive(false);
            return;
        }
        _retryButton.gameObject.SetActive(false);
        _toLobbyButton.gameObject.SetActive(false);
        _toTitleButton.gameObject.SetActive(true);
    }

    public void Retry()
    {
        SceneController.Instance.ReloadCurrentScene();
    }

    public void OpenControls()
    {
        _= UIController.Instance.OpenUI<UI_ControlGuide>();
        UIController.Instance.CloseUI(this);
    }

    public void OpenSettings()
    {
        _= UIController.Instance.OpenUI<UI_Settings>();
        UIController.Instance.CloseUI(this);
    }

    public void GotoLobby()
    {
        SceneController.Instance.LoadScene(ESceneType.LobbyScene);
    }

    public void GotoTitle()
    {
        SceneController.Instance.LoadScene(ESceneType.LoginScene);
    }

    public void ExitGame()
    {
        _= UIController.Instance.OpenUI<UI_Exit>();
    }
}