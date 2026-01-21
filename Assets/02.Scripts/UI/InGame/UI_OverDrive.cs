using _02.Scripts.Player.Gauge;
using UnityEngine;
using UnityEngine.UI;

public class UI_OverDrive : MonoBehaviour
{
    [SerializeField] private GaugeManager _gaugeManager;
    
    [Header("UI References")]
    [SerializeField] private Image _gaugeEffectImage;
    [SerializeField] private Image _overDriveIcon;

    [Header("Components")]
    [SerializeField] private Gauge _gauge;

    private void Awake()
    {
        _gauge.Init();
        ResetGauge();
        SubscribeOverDriveEvents();
    }

    private void SubscribeOverDriveEvents()
    {
        _gaugeManager.OnOverDriveDeactivated += ShowChargingPercent;
        _gaugeManager.OnOverDriveGaugeChanged += SetGauge;
    }

    private void UnsubscribeOverDriveEvents()
    {
        _gaugeManager.OnOverDriveDeactivated -= ShowChargingPercent;
        _gaugeManager.OnOverDriveGaugeChanged -= SetGauge;
    }

    private void Start()
    {
        ShowChargingPercent();
    }

    private void OnDestroy()
    {
        _gauge.Clear();
        UnsubscribeOverDriveEvents();
    }

    private void SetGauge(float value, float maxValue)
    {
        float nextValue = value / maxValue;
        float gaugeValue = Mathf.Clamp01(nextValue);
        _gauge.SetValue(gaugeValue);
        
        if (Mathf.Approximately(value, maxValue))
        {
            ShowOverDrive();
        }
    }

    private void ResetGauge()
    {
        _gauge.SetValue(0f, immediate: true);
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
    }
}
