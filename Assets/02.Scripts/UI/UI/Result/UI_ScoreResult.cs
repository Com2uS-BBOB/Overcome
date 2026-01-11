using System;
using UnityEngine;
using DG.Tweening;

public class UI_ScoreResult : MonoBehaviour, ISequentialUI
{
    [SerializeField] private AnimatedNumber _scoreNumber;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Animation")]
    [SerializeField] private float _showDuration = 0.3f;
    [SerializeField] private Ease _showEase = Ease.OutBack;


    private int _targetScore;
    private bool _isSubscribed;
    
    public event Action OnShowComplete;

    private void Awake()
    {
        _scoreNumber.Init();
    }

    private void OnDestroy()
    {
        UnsubscribeEvent();
        _scoreNumber.Clear();
    }

    public void Show()
    {
        _targetScore = ScoreSystem.Instance.CurrentScore;
        gameObject.SetActive(true);
        SubscribeEvent();

        // 페이드 + 스케일 애니메이션
        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.DOFade(1f, _showDuration);
        }

        transform.localScale = Vector3.zero;
        transform
            .DOScale(1f, _showDuration)
            .SetEase(_showEase)
            .OnComplete(() =>
            {
                _scoreNumber.SetValue(_targetScore);
            });
    }

    private void SubscribeEvent()
    {
        if (!_isSubscribed)
        {
            _scoreNumber.OnCompleteChanging += HandleNumberComplete;
            _isSubscribed = true;
        }
    }

    private void UnsubscribeEvent()
    {
        if (_isSubscribed)
        {
            _scoreNumber.OnCompleteChanging -= HandleNumberComplete;
            _isSubscribed = false;
        }
    }

    private void HandleNumberComplete()
    {
        OnShowComplete?.Invoke();
    }

    public void Hide()
    {
        UnsubscribeEvent();
        gameObject.SetActive(false);
    }
}
