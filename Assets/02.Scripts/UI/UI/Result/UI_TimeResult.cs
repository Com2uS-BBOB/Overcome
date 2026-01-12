using System;
using UnityEngine;
using DG.Tweening;

public class UI_TimeResult : MonoBehaviour, ISequentialUI
{
    [SerializeField] private AnimatedNumber _timeNumber;
    [SerializeField] private CanvasGroup _canvasGroup;

    [Header("Animation")]
    [SerializeField] private float _showDuration = 0.3f;
    [SerializeField] private Ease _showEase = Ease.OutBack;

    public event Action OnShowComplete;

    private float _targetTime;
    private bool _isSubscribed;

    private void Awake()
    {
        _timeNumber.Init();
    }

    private void OnDestroy()
    {
        UnsubscribeEvent();
        _timeNumber.Clear();
    }

    public void Show()
    {
        _targetTime = TimeSystem.Instance.PlayTime;
        gameObject.SetActive(true);
        SubscribeEvent();

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
                _timeNumber.SetValue(_targetTime);
            });
    }

    private void SubscribeEvent()
    {
        if (!_isSubscribed)
        {
            _timeNumber.OnCompleteChanging += HandleNumberComplete;
            _isSubscribed = true;
        }
    }

    private void UnsubscribeEvent()
    {
        if (_isSubscribed)
        {
            _timeNumber.OnCompleteChanging -= HandleNumberComplete;
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

    public void SetTime(float time)
    {
        _targetTime = time;
    }
}
