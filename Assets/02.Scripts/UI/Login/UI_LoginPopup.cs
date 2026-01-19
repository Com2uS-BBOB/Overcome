using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class UI_LoginPopup : BaseUI
{
    [Header("Popup Duration")]
    [SerializeField] private float _popupOpen = 0.2f;
    [SerializeField] private float _popupMaintain = 1.0f;
    [SerializeField] private float _popupClose = 0.2f;
    [SerializeField] private TextMeshProUGUI _welcomeText;

    protected override void Init()
    {
        transform.localScale = Vector3.zero;
    }

    public void SetTitle(string title)
    {
        _welcomeText.text = title;
    }

    protected override void PlayOpenAnimation()
    {
        transform.DOScale(Vector3.one, _popupOpen)
                 .SetEase(Ease.Linear)
                 .OnComplete(() =>
                 {
                     StartCoroutine(ShowWelcomeTextCoroutine());
                 });
    }

    protected override void PlayCloseAnimation()
    {
        transform.DOScale(Vector3.zero, _popupClose)
                 .SetEase(Ease.Linear)
                 .OnComplete(() =>
                 {
                     gameObject.SetActive(false);
                     SceneController.Instance.LoadSceneAsync(ESceneType.LobbyScene);
                 });
    }

    private IEnumerator ShowWelcomeTextCoroutine()
    {
        yield return new WaitForSeconds(_popupMaintain);
        UIController.Instance.CloseUI(this);
    }
}
