using System;
using TMPro;
using UnityEngine;
using DG.Tweening;

[Serializable]
public class AnimatedNumber
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _text;

    [Header("Format")]
    [SerializeField] private string _format = "{0:0}";

    [Header("Animation")]
    [SerializeField] private float _duration = 0.5f;
    [SerializeField] private Ease _ease = Ease.OutQuad;

    public Action OnCompleteChanging;
    private float _currentValue;
    private Tweener _tweener;

    public void Init()
    {
        _tweener = DOTween.To(() => _currentValue, x =>
            {
                _currentValue = x;
                UpdateText();
            }, 0f, _duration)
            .SetEase(_ease)
            .SetAutoKill(false)
            .OnComplete(() => { OnCompleteChanging?.Invoke(); });
    }

    public void Clear()
    {
        _tweener?.Kill();
    }

    public void SetValue(float value, bool immediate = false)
    {
        if (Mathf.Approximately(_currentValue, value))
        {
            OnCompleteChanging?.Invoke();
            return;
        }

        if (immediate)
        {
            if (_tweener.IsPlaying())
            {
                _tweener.Complete();
            }
            _currentValue = value;
            UpdateText();
            return;
        }

        _tweener.ChangeStartValue(_currentValue);
        _tweener.ChangeEndValue(value);
        _tweener.Restart();
    }

    private void UpdateText()
    {
        // todo. 시간 기반 Text 표시 기능 필요
        _text.SetText(_format, _currentValue);
    }
    
    public void ActivateTextUI() => _text.gameObject.SetActive(true);
    
    public void DeactivateTextUI()
    {
        if (_tweener.IsPlaying())
        {
            _tweener.Complete();
        }
        _text.gameObject.SetActive(false);
    }
}
