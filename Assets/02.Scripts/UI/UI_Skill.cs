using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Skill : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image _coolDownGauge;
    [SerializeField] private TextMeshProUGUI _coolDownCountText;
    [SerializeField] private Image _coolDownBackground;
    [SerializeField] private Image _blockSkillImage;
    private int _lastDisplayTime;
    private bool _processCoolDown;
    
    
    // Test Code
    [Header("Test Settings")]
    [SerializeField] private float _coolDownDuration = 5f;
    private bool _canUse = true;
    private float _elapsedTime = 0f;

    private void Start()
    {
        _blockSkillImage.gameObject.SetActive(!_canUse);
        HideCooldownUI();
    }

    public void SetSkillAvailability(bool canUse)
    {
        _canUse = canUse;
        _blockSkillImage.gameObject.SetActive(!canUse);
    }

    public void SetCoolDown()
    {
        if (!_canUse) return;
        
        _elapsedTime = 0f;
        _processCoolDown = true;
        
        ShowCooldownUI();
    }

    private void Update()
    {
        if (!_processCoolDown) return;
        
        _elapsedTime += Time.deltaTime;
        float progress = Mathf.Clamp01(_elapsedTime / _coolDownDuration);
        UpdateCoolDownUI(progress);

        if (progress >= 1f)
        {
            EndCoolDown();
        }
    }

    private void UpdateCoolDownUI(float progress)
    {
        _coolDownGauge.fillAmount = progress;
        
        float remainTime = _coolDownDuration * (1f - progress);
        int displayTime = Mathf.CeilToInt(remainTime);
        if (displayTime != _lastDisplayTime)
        {
            _lastDisplayTime = displayTime;
            _coolDownCountText.text = displayTime.ToString();
        }
    }

    private void EndCoolDown()
    {
        _processCoolDown = false;
        HideCooldownUI();
    }

    
    private void ShowCooldownUI()
    {
        _coolDownBackground.gameObject.SetActive(true);
        _coolDownGauge.gameObject.SetActive(true);
        _coolDownCountText.gameObject.SetActive(true);
    }

    private void HideCooldownUI()
    {
        _coolDownBackground.gameObject.SetActive(false);
        _coolDownGauge.gameObject.SetActive(false);
        _coolDownCountText.gameObject.SetActive(false);
    }
}