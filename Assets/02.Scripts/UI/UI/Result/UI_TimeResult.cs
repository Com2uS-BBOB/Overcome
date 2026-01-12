using System;
using UnityEngine;
using DG.Tweening;

public class UI_TimeResult : MonoBehaviour
{
    [SerializeField] private ProgressiveScrambleText _timeText;
    private float _targetTime;

    private void Awake()
    {
        _timeText.Init(this);
    }

    private void OnEnable()
    {
        Show();
    }
    
    private void OnDisable()
    {
        _timeText.Stop();
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
}
