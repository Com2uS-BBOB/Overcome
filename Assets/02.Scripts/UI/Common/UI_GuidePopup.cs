using DG.Tweening;
using UnityEngine;

public class UI_GuidePopup : BaseUI
{
    public static bool IsOpened = false;
    [SerializeField] private float _movement = 500.0f;
    [SerializeField] private float _animationDuration = 0.5f;

    private Vector3 _originalPosition;

    protected override void Init()
    {
        _originalPosition = transform.position;
    }

    protected override void PlayOpenAnimation()
    {
        transform.position = _originalPosition + Vector3.left * _movement;
        transform.DOMoveX(_originalPosition.x, _animationDuration)
                 .SetEase(Ease.Linear);
    }

    protected override void PlayCloseAnimation()
    {
        transform.DOMoveX(_originalPosition.x - _movement, _animationDuration)
                 .SetEase(Ease.Linear)
                 .OnComplete(() =>
                 {
                     transform.position = _originalPosition;
                     gameObject.SetActive(false);
                 });
    }

    public override void OnOpen()
    {
        IsOpened = true;
        base.OnOpen();
    }

    public override void OnClose()
    {
        IsOpened = false;
        base.OnClose();
    }
}
