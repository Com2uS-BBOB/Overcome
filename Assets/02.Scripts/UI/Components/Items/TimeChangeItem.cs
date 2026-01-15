using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class TimeChangeItem : MonoBehaviour
{
    [SerializeField] private float _moveDistance = 50f;

    private TextMeshProUGUI _valueText;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;

    private Action<TimeChangeItem> _onComplete;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _valueText = GetComponent<TextMeshProUGUI>();
    }

    public void Init(float value, float duration, Vector2 anchoredPosition, Action<TimeChangeItem> onComplete)
    {
        _rectTransform.anchoredPosition = anchoredPosition;
        _canvasGroup.alpha = 1f;
        _onComplete = onComplete;

        SetText(value);
        PlayAnimation(value > 0, duration);
    }

    private void SetText(float value)
    {
        if (value > 0)
        {
            _valueText.text = $"+{value:F1}s";
            _valueText.color = Color.green;
        }
        else
        {
            _valueText.text = $"{value:F1}s";
            _valueText.color = Color.red;
        }
    }

    private void PlayAnimation(bool moveUp, float duration)
    {
        float targetY = moveUp ? _moveDistance : -_moveDistance;

        _rectTransform.
            DOAnchorPosY(_rectTransform.anchoredPosition.y + targetY, duration)
            .SetEase(Ease.OutQuad);

        _canvasGroup.DOFade(0f, duration)
            .SetEase(Ease.InQuad)
            .OnComplete(() => _onComplete?.Invoke(this));
    }
}
