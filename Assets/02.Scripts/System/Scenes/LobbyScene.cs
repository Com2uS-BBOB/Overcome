using UnityEngine;

public class LobbyScene : MonoBehaviour
{
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void HandleGameStart(int chapter, int level)
    {
        StageManager.Instance.SetCurrentStage(chapter, level);
        LoadGameScene();
    }

    public void LoadGameScene()
    {
        SceneController.Instance.LoadSceneAsync(ESceneType.GameScene);
    }

    public void LoadTutorialScene()
    {
        SceneController.Instance.LoadSceneAsync(ESceneType.TutorialScene);
    }
}
