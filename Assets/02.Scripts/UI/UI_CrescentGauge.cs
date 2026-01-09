using UnityEngine;
using UnityEngine.UI;

public class UI_CrescentGauge : MonoBehaviour
{
    [SerializeField] private Image _gaugeFillImage;

    [Header("Test Code")]
    [SerializeField] private float _remainValue;

    public void SetGaugeValue(float value)
    {
        _remainValue = Mathf.Clamp01(value);
        UpdateGaugeUI();
    }

    private void UpdateGaugeUI()
    {
        if (_gaugeFillImage == null) return;
        if (Mathf.Approximately(_gaugeFillImage.fillAmount, _remainValue)) return;
        _gaugeFillImage.fillAmount = _remainValue;
    }
}
