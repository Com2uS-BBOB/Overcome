using System;
using UnityEngine;
using DG.Tweening;

public class UI_ScoreResult : MonoBehaviour
{
    [SerializeField] private ProgressiveScrambleText _scoreText;
    private int _targetScore;

    public event Action OnComplete;

    private void Awake()
    {
        _scoreText.Init(this);
        _scoreText.OnComplete += HandleComplete;
    }

    private void OnDestroy()
    {
        _scoreText.OnComplete -= HandleComplete;
    }

    private void OnDisable()
    {
        _scoreText.Stop();
    }

    private void OnEnable()
    {
        Show();
    }

    private void HandleComplete()
    {
        OnComplete?.Invoke();
    }

    public void Show()
    {
        _targetScore = ScoreSystem.Instance.CurrentScore;
        gameObject.SetActive(true);
        _scoreText.Play(_targetScore);
    }

    public void Hide()
    {
        _scoreText.Stop();
        gameObject.SetActive(false);
    }

    public void Complete()
    {
        _scoreText.Stop();
        _scoreText.SetTextImmediate(_targetScore);
        OnComplete?.Invoke();
    }
}
