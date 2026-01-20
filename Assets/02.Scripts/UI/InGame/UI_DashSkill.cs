using _02.Scripts.Player.Combat;
using _02.Scripts.Player.Gauge;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_DashSkill : MonoBehaviour
{
    [SerializeField] private DashAttackSkill _dashSkill;
    [SerializeField] private GaugeManager _gaugeManager;
    
    [Header("UI References")]
    [SerializeField] private Image _skillIcon;
    
    [Tooltip("1 -> 0으로 감소")]
    [SerializeField] private Image _coolDownGauge;
    [SerializeField] private TextMeshProUGUI _coolDownCountText;
    [SerializeField] private Image _blockSkillImage;

    [Header("스킬 색상")]
    [SerializeField] private Color _skillUnuseColor;
    [SerializeField] private Color _skillUseColor;

    private bool _onProcessDash;
    private bool _killEnemy;
    private bool _isOverDriveActive;

    private void Start()
    {
        // OnDashStarted : SkillIcon 사용중 이미지 표시
        // OnDashEnded : SkillIcon 사용중 이미지 끄기, CoolDown 시작
        // OnCooldownReset : CoolDown UI 초기화
        
        _dashSkill.OnDashStarted += ActivateSkillUsingImage;
        _dashSkill.OnDashEnded += SetCoolDown;
        _dashSkill.OnCooldownReset += ResetCooldownUI;
        
        _gaugeManager.OnOverDriveActivated += ActivateOverdrive;
        _gaugeManager.OnOverDriveDeactivated += DeactivateOverdrive;

        _blockSkillImage.gameObject.SetActive(!_dashSkill.CanUse);
        HideCooldownUI();
    }

    private void OnDestroy()
    {
        _dashSkill.OnDashStarted -= ActivateSkillUsingImage;
        _dashSkill.OnDashEnded -= SetCoolDown;
        _dashSkill.OnCooldownReset -= ResetCooldownUI;
    
        _gaugeManager.OnOverDriveActivated -= ActivateOverdrive;
        _gaugeManager.OnOverDriveDeactivated -= DeactivateOverdrive;
    
        // Tween 정리
        _blockSkillImage?.DOKill();
    }

    private void ActivateOverdrive()
    {
        _isOverDriveActive = true;
    }
    
    private void DeactivateOverdrive()
    {
        _isOverDriveActive = false;
    }
    
    private void SetCoolDown()
    {
        DeactivateSkillUsingImage();

        if (_killEnemy)
        {
            _killEnemy = false;
            return;
        }
        if (_isOverDriveActive) return;

        ShowCooldownUI();
    
        // 기존 Tween 완전히 제거
        _coolDownGauge.DOKill();
    
        _coolDownGauge.fillAmount = 1f;
        _coolDownGauge.DOFillAmount(0f, _dashSkill.Cooldown - _dashSkill.DashDuration)
                      .SetEase(Ease.Linear)
                      .OnUpdate(() => 
                      {
                          float remaining = _dashSkill.Cooldown * _coolDownGauge.fillAmount;
                          _coolDownCountText.SetText(Mathf.Ceil(remaining).ToString("N0"));
                      })
                      .OnComplete(HideCooldownUI);
    }

    private void ResetCooldownUI()
    {
        _coolDownGauge.DOKill();
        HideCooldownUI();
        if (_onProcessDash)
        {
            _killEnemy = true;
        }
    }

    private void ShowCooldownUI()
    {
        _coolDownGauge?.gameObject.SetActive(true);
        _coolDownCountText?.gameObject.SetActive(true);
    }

    private void HideCooldownUI()
    {
        _coolDownGauge?.gameObject.SetActive(false);
        _coolDownCountText?.gameObject.SetActive(false);
    }
    
    private void DeactivateSkillUsingImage()
    {
        _onProcessDash = false;
        _skillIcon.color = _skillUnuseColor;
    }

    private void ActivateSkillUsingImage()
    {
        _onProcessDash = true;
        _coolDownGauge.DOKill();
        _skillIcon.color = _skillUseColor;
    }
}
