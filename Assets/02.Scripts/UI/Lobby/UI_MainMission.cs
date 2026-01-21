using DG.Tweening;
using UnityEngine;

public class UI_MainMission : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;

    public void LoadSelectScene()
    {
        _= UIController.Instance.OpenUI<UI_StageSelect>();
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
    }
}
