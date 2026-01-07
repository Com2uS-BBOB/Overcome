using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_CrescentGauge : MonoBehaviour
{
    [SerializeField] private Image _gaugeFillImage;

    [Header("Test Code")]
    [SerializeField] private float _remainValue;

    [Header("Animation")]
    [SerializeField] private float _duration = 1f;
    [SerializeField] private Ease _ease = Ease.Linear;
    private Tweener _tweener;
    
    private void Awake()
    {
        _tweener = _gaugeFillImage
                   .DOFillAmount(_remainValue, _duration)
                   .SetEase(_ease)
                   .SetAutoKill(false);
    }

    private void OnDestroy()
    {
        _tweener?.Kill();
    }

    public void SetGaugeValue(float value)
    {
        _remainValue = Mathf.Clamp01(value);
        UpdateGaugeUI();
    }
    
    private void UpdateGaugeUI()
    {
        if (_gaugeFillImage == null) return;
        if (Mathf.Approximately(_gaugeFillImage.fillAmount, _remainValue)) return;

        if (_tweener == null) return;
        _tweener.ChangeStartValue(_gaugeFillImage.fillAmount);
        _tweener.ChangeEndValue(_remainValue);
    }
}