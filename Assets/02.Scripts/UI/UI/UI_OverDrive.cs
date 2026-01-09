using UnityEngine;
using UnityEngine.UI;

public class UI_OverDrive : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image _gaugeEffectImage;
    [SerializeField] private Image _overDriveIcon;

    [Header("Components")]
    [SerializeField] private Gauge _gauge;
    [SerializeField] private AnimatedNumber _percentText;

    private void Awake()
    {
        _gauge.Init();
        _percentText.Init();
        ResetGauge();
    }

    private void Start()
    {
        ShowChargingPercent();
    }

    private void OnDestroy()
    {
        _gauge.Clear();
        _percentText.Clear();
    }

    public void SetGauge(float value)
    {
        float gaugeValue = Mathf.Clamp01(value);
        _gauge.SetValue(gaugeValue);
        _percentText.SetValue(gaugeValue * 100f);

        if (gaugeValue >= 1.0f)
        {
            ShowOverDrive();
        }
    }

    public void ResetGauge()
    {
        _gauge.SetValue(0f, immediate: true);
        _percentText.SetValue(0f, immediate: true);
        ShowChargingPercent();
    }

    private void ShowChargingPercent()
    {
        _gaugeEffectImage.gameObject.SetActive(false);
        _overDriveIcon.gameObject.SetActive(false);
    }

    private void ShowOverDrive()
    {
        _gaugeEffectImage.gameObject.SetActive(true);
        _overDriveIcon.gameObject.SetActive(true);
        _percentText.Deactive();
    }
    
    #region Test Code
    public void IncreaseGauge(float value)
    {
        float gaugeValue = Mathf.Clamp01(_gauge.Value + value);
        SetGauge(gaugeValue);
    }
    #endregion
}
