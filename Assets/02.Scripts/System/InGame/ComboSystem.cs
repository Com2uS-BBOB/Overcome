using System;
using TMPro;
using UnityEngine;

public class ComboSystem : SingletonBehaviour<ComboSystem>
{
    protected override bool DontDestroy => false;

    [Header("Combo Configs")]
    [SerializeField] private ComboConfigData _comboConfigData;

    // Ingame Data
    private ComboConfig _currentComboConfig;
    private int _comboCount;
    private float _comboTimer;

    // Property
    public int ComboCount => _comboCount;
    public string ComboText => _currentComboConfig?.ComboText;
    public float ComboDuration => _comboConfigData.ComboDuration;
    public float DamageMultiplier => _currentComboConfig?.DamageMultiplier ?? 1.0f;
    public VertexGradient ComboColorGradient => _currentComboConfig?.GetComboVertexGradient() ?? new VertexGradient(Color.white);
    public VertexGradient GradeColorGradient => _currentComboConfig?.GetGradeVertexGradient() ?? new VertexGradient(Color.black);
    
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
    
    private void OnEnable()
    {
        EnemyEventController.Enemy.OnHit += AddCombo;
    }

    private void OnDisable()
    {
        EnemyEventController.Enemy.OnHit -= AddCombo;
    }
    
    private void Update()
    {
        if (_comboCount <= 0) return;

        _comboTimer -= Time.deltaTime;
        if (_comboTimer > 0f) return;

        ResetCombo();
    }
 
    private void AddCombo(EnemyHitEvent hitEvent)
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
