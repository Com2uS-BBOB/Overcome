using DG.Tweening;
using UnityEngine;

public class UI_MainMission : MonoBehaviour
{
    [SerializeField] private CanvasGroup _lobbyCanvas;
    [SerializeField] private CanvasGroup _stageSelectCanvas;
    [SerializeField] private float _changeDuration;
    
    public void LoadSelectScene()
    {
        _= UIController.Instance.OpenUI<UI_StageSelect>();
    }
}
