using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class TutorialDescriptionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private Button _nextButton;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private RectTransform _canvasRect;

    [Space(10)]
    [Header("Animation")]
    [SerializeField] private float _fadeinTime = 0.3f;
    [SerializeField] private float _fadeoutTime = 0.2f;

    [Space(10)]
    [Header("Dynamic Sizing")]
    [SerializeField] private Vector2 _padding = new Vector2(40f, 40f);
    [SerializeField] private Vector2 _minSize = new Vector2(200f, 100f);
    [SerializeField] private Vector2 _maxSize = new Vector2(600f, 400f);

    private const float OriginThreshold = 0.01f;
    private Action _onNextCallback;

    private void Awake()
    {
        _nextButton.onClick.AddListener(OnNextButtonClicked);
    }

    public void ShowNearTarget(string description, RectTransform target, float offset, Action onNext)
    {
        _onNextCallback = onNext;
        _descriptionText.text = description;

        UpdateSizeBasedOnText();
        PositionNearTarget(target, offset);

        gameObject.SetActive(true);

        _canvasGroup.alpha = 0;
        _canvasGroup.DOFade(1f, _fadeinTime).SetUpdate(true);

        transform.localScale = Vector3.zero;
        transform.DOScale(1f, _fadeinTime).SetEase(Ease.OutBack).SetUpdate(true);
    }

    private void UpdateSizeBasedOnText()
    {
        _descriptionText.ForceMeshUpdate();

        float maxTextWidth = _maxSize.x - _padding.x;
        Vector2 preferredSize = _descriptionText.GetPreferredValues(maxTextWidth, 0);
        Vector2 newSize = preferredSize + _padding;

        newSize.x = Mathf.Clamp(newSize.x, _minSize.x, _maxSize.x);
        newSize.y = Mathf.Clamp(newSize.y, _minSize.y, _maxSize.y);

        _rectTransform.sizeDelta = newSize;
    }

    private void PositionNearTarget(RectTransform target, float offset)
    {
        Vector2 targetCenterWorld = target.TransformPoint(target.rect.center);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect,
            targetCenterWorld,
            null,
            out Vector2 targetCenterLocal
        );

        Vector2 targetSize = new Vector2(
            target.rect.width * target.lossyScale.x,
            target.rect.height * target.lossyScale.y
        );

        Vector2 position = CalculatePositionTowardOrigin(targetCenterLocal, targetSize, offset);
        _rectTransform.anchoredPosition = position;
    }

    private Vector2 CalculatePositionTowardOrigin(Vector2 targetPos, Vector2 targetSize, float offset)
    {
        Vector2 directionToOrigin = -targetPos;
        if (directionToOrigin.sqrMagnitude < OriginThreshold)
        {
            directionToOrigin = Vector2.right;
        }
        else
        {
            directionToOrigin.Normalize();
        }

        Vector2 descSize = _rectTransform.sizeDelta;
        float separationX = (targetSize.x + descSize.x) / 2f + offset;
        float separationY = (targetSize.y + descSize.y) / 2f + offset;

        float absX = Mathf.Abs(directionToOrigin.x);
        float absY = Mathf.Abs(directionToOrigin.y);

        float weightX = 1f;
        float weightY = 1f;

        if (absY > absX)
        {
            weightX = 0.5f;
        }
        else if (absX > absY)
        {
            weightY = 0.5f;
        }

        float moveX = directionToOrigin.x != 0 ? Mathf.Sign(directionToOrigin.x) * separationX * weightX : 0f;
        float moveY = directionToOrigin.y != 0 ? Mathf.Sign(directionToOrigin.y) * separationY * weightY : 0f;

        return targetPos + new Vector2(moveX, moveY);
    }


    public void Hide()
    {
        _canvasGroup.DOFade(0f, _fadeoutTime)
                    .SetUpdate(true)
                    .OnComplete(() => gameObject.SetActive(false));
    }

    private void OnNextButtonClicked()
    {
        _onNextCallback?.Invoke();
    }
}