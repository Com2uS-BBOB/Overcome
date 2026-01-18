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
    
    private readonly Vector2[] _candidatePositions = new Vector2[4];
    private Action _onNextCallback;

    private void Awake()
    {
        _nextButton.onClick.AddListener(OnNextButtonClicked);
        gameObject.SetActive(false);
    }

    public void ShowNearTarget(string description, RectTransform target, float offset, Action onNext)
    {
        _onNextCallback = onNext;
        _descriptionText.text = description;

        gameObject.SetActive(true);
        PositionNearTarget(target, offset);

        _canvasGroup.alpha = 0;
        _canvasGroup.DOFade(1f, _fadeinTime);

        transform.localScale = Vector3.zero;
        transform.DOScale(1f, _fadeinTime).SetEase(Ease.OutBack);
    }

    private void PositionNearTarget(RectTransform target, float offset)
    {
        Vector2 targetScreenPos = target.position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle
        (
            _canvasRect,
            targetScreenPos,
            null,
            out Vector2 targetLocalPos
        );

        Vector2 targetSize = target.sizeDelta;
        Vector2 descriptionSize = _rectTransform.sizeDelta;
        Vector2 spawnPosition = CalculatePositionNearestToOrigin(targetLocalPos, targetSize, descriptionSize, offset);

        _rectTransform.anchoredPosition = spawnPosition;
        _rectTransform.pivot = target.pivot;
    }

    private Vector2 CalculatePositionNearestToOrigin(Vector2 targetPos, Vector2 targetSize, Vector2 descSize, float offset)
    {
        _candidatePositions[0] = new Vector2(targetPos.x + targetSize.x / 2 + descSize.x / 2 + offset, targetPos.y);
        _candidatePositions[1] = new Vector2(targetPos.x - targetSize.x / 2 - descSize.x / 2 - offset, targetPos.y);
        _candidatePositions[2] = new Vector2(targetPos.x, targetPos.y + targetSize.y / 2 + descSize.y / 2 + offset);
        _candidatePositions[3] = new Vector2(targetPos.x, targetPos.y - targetSize.y / 2 - descSize.y / 2 - offset);

        float minDistance = float.MaxValue;
        Vector2 spawnPosition = _candidatePositions[0];

        foreach (Vector2 v in _candidatePositions)
        {
            float distance = v.magnitude;
            if (distance >= minDistance) continue;
            minDistance = distance;
            spawnPosition = v;
        }

        return spawnPosition;
    }

    public void Hide()
    {
        _canvasGroup.DOFade(0f, 0.2f).OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    private void OnNextButtonClicked()
    {
        _onNextCallback?.Invoke();
    }
}