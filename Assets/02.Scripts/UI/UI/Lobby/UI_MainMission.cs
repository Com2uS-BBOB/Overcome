using DG.Tweening;
using UnityEngine;

public class UI_MainMission : MonoBehaviour
{
    [SerializeField] private CanvasGroup _lobbyCanvas;
    [SerializeField] private CanvasGroup _stageSelectCanvas;
    [SerializeField] private float _changeDuration;
    
    public void LoadSelectScene()
    {
        _lobbyCanvas.alpha = 0f;
        _stageSelectCanvas.DOFade(1f, _changeDuration);
        _stageSelectCanvas.blocksRaycasts = true;
    }
}
