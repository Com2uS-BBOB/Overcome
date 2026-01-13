using System;
using UnityEngine;
using DG.Tweening;

public class UI_TimeResult : MonoBehaviour
{
    [SerializeField] private ProgressiveScrambleText _timeText;
    private float _targetTime;

    public event Action OnComplete;

    private void Awake()
    {
        _timeText.Init(this);
        _timeText.OnComplete += HandleComplete;
    }

    private void OnDestroy()
    {
        _timeText.OnComplete -= HandleComplete;
    }

    private void OnEnable()
    {
        Show();
    }

    private void OnDisable()
    {
        _timeText.Stop();
    }

    private void HandleComplete()
    {
        OnComplete?.Invoke();
    }

    public void Show()
    {
        _targetTime = TimeSystem.Instance.PlayTime;
        gameObject.SetActive(true);
        _timeText.PlayTime(_targetTime);
    }

    public void Hide()
    {
        _timeText.Stop();
        gameObject.SetActive(false);
    }

    public void Complete()
    {
        _timeText.Stop();
        _timeText.SetTimeImmediate(_targetTime);
        OnComplete?.Invoke();
    }
}
