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
    [SerializeField] private string _format = "{0:F0}";

    [Header("Animation")]
    [SerializeField] private float _duration = 0.5f;
    [SerializeField] private Ease _ease = Ease.OutQuad;

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
            .SetAutoKill(false);
    }

    public void Clear()
    {
        _tweener?.Kill();
    }

    public void SetValue(float value, bool immediate = false)
    {
        if (_text == null) return;
        if (Mathf.Approximately(_currentValue, value)) return;

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
        _text.text = string.Format(_format, _currentValue);
    }
    
    public void Deactive()
    {
        if (_tweener.IsPlaying())
        {
            _tweener.Complete();
        }
        _text.gameObject.SetActive(false);
    }
}
