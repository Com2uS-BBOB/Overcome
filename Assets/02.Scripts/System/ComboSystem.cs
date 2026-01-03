using System;
using UnityEngine;

public class ComboSystem : SingletonBehaviour<ComboSystem>
{
    [Header("Combo Configs")]
    [SerializeField] private ComboConfigData _comboConfigData;

    // InGame Combo Data
    private ComboConfig _currentComboConfig;
    private int _comboCount;
    private float _comboTimer;

    // Property
    public int ComboCount => _comboCount;
    public string ComboText => _currentComboConfig?.ComboText;
    public float ComboDuration => _comboConfigData.ComboDuration;
    public float DamageMultiplier => _currentComboConfig.DamageMultiplier;

    public event Action OnComboChanged;
    
    protected override void Init()
    {
        if (_comboConfigData == null)
        {
            Debug.LogError("[ComboSystem] ComboConfigData Doesn't Exist");
            enabled = false;
            return;
        }
        _currentComboConfig = _comboConfigData.GetConfig(_comboCount);
    }
    
    private void Update()
    {
        if (_comboCount <= 0) return;

        _comboTimer -= Time.deltaTime;
        if (_comboTimer > 0f) return;

        ResetCombo();
    }
 
    public void AddCombo()
    {
        _comboCount = Mathf.Min(_comboCount + 1, _comboConfigData.MaxCombo);
        _comboTimer = _comboConfigData.ComboDuration;

        if (_comboCount > _currentComboConfig.MaxComboInclusive)
        {
            _currentComboConfig = _comboConfigData.GetConfig(_comboCount);
        }
        
        OnComboChanged?.Invoke();
    }

    private void ResetCombo()
    {
        _comboCount = 0;
        _comboTimer = 0.0f;
        _currentComboConfig = _comboConfigData.GetConfig(_comboCount);
        OnComboChanged?.Invoke();
    }
}
