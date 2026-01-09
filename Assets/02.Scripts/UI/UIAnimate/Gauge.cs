using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[Serializable]
public class Gauge
{
    [Header("UI References")]
    [SerializeField] private Image _fillImage;
    
    [Header("Animations")]
    [SerializeField] private float _duration = 1f;
    [SerializeField] private Ease _ease = Ease.Linear;
    
    private float _remainValue;
    public float Value => _remainValue;
    private Tweener _tweener;

    public void Init()
    {
        _remainValue = 0;
        _tweener = _fillImage
                   .DOFillAmount(_remainValue, _duration)
                   .SetEase(_ease)
                   .SetAutoKill(false);
    }
    
    public void Clear()
    {
        _tweener?.Kill();
    }

    public void SetValue(float value, bool immediate = false)
    {
        if (_fillImage == null) return;
        if (Mathf.Approximately(_fillImage.fillAmount, value)) return;
        if (immediate)
        {
            if (_tweener.IsPlaying())
            {
                _tweener.Complete();
            }
            _remainValue = value;
            _fillImage.fillAmount = value;
            return;
        }
        _remainValue = Mathf.Clamp01(value);
        UpdateGaugeUI();
    }

    private void UpdateGaugeUI()
    {
        _tweener.ChangeStartValue(_fillImage.fillAmount);
        _tweener.ChangeEndValue(_remainValue);
        _tweener.Restart();
    }

    public void Deactive()
    {
        if (_tweener.IsPlaying())
        {
            _tweener.Complete();
        }
        _fillImage.gameObject.SetActive(false);
    }
}
