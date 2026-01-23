using DG.Tweening;
using UnityEngine;

public class UI_MainMission : MonoBehaviour
{
    [SerializeField] private GameObject _Root;

    public void LoadSelectScene()
    {
        _= UIController.Instance.OpenUI<UI_StageSelect>();
        _Root.SetActive(false);
    }
}
