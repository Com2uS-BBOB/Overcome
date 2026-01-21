using UnityEngine;

public class LoginScene : MonoBehaviour
{
    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
        Time.timeScale = 1f;
    }
    
    public void HandleLoginCompleted()
    {
        LoadLobbyScene();
    }

    public void LoadLobbyScene()
    {
        SceneController.Instance.LoadSceneAsync(ESceneType.LobbyScene);
    }
}
