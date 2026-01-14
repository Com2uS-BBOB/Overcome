using UnityEngine;

public class UI_MainMission : MonoBehaviour
{
    public void LoadSelectScene()
    {
        SceneController.Instance.LoadSceneAsync(ESceneType.StageSelectScene);
    }
}
