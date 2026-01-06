using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_Gauge : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image _gaugeFillImage;
    [SerializeField] private Image _gaugeEffectImage;
    [SerializeField] private Image _gaugeOutlineImage;
    [SerializeField] private Image _overDriveIcon;
    [SerializeField] private TextMeshProUGUI _gaugePercentText;
    
    [Space(10)]
    [Header("Test Settings")]
    [SerializeField] private float _chargingAmount;
    
    private void Start()
    {
        ShowChargingPercent();
    }

    public void ImproveGauge()
    {
        float fillAmount = _gaugeFillImage.fillAmount;
        if (fillAmount >= 1.0f) return;
        fillAmount = Mathf.Clamp01(fillAmount + _chargingAmount);
        _gaugePercentText.text = $"{(fillAmount * 100):F0}%";
        _gaugeFillImage.fillAmount = fillAmount;
        if (fillAmount >= 1.0f)
        {
            ShowOverDrive();
        }
    }

    public void ResetGauge()
    {
        ShowChargingPercent();
    }

    private void ShowChargingPercent()
    {
        _gaugeEffectImage.gameObject.SetActive(false);
        _overDriveIcon.gameObject.SetActive(false);
        
        _gaugePercentText.gameObject.SetActive(true);
        _gaugePercentText.text = "0%";
        
        _gaugeFillImage.gameObject.SetActive(true);
        _gaugeFillImage.fillAmount = 0.0f;
    }

    private void ShowOverDrive()
    {
        _gaugeEffectImage.gameObject.SetActive(true);
        _overDriveIcon.gameObject.SetActive(true);
        _gaugePercentText.gameObject.SetActive(false);
        _gaugeFillImage.gameObject.SetActive(false);
    }
}
