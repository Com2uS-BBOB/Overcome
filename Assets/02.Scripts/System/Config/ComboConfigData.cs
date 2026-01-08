using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ComboConfigData", menuName = "Game/Combo Config")]
public class ComboConfigData : ScriptableObject
{
    [Header("General Settings")]
    public int MaxCombo = 10;
    public float ComboDuration = 5.0f;
    
    [Space(10)]
    [Header("Combo Configs")]
    public ComboConfig[] Configs;
    
    public ComboConfig GetConfig(int currentCombo)
    {
        if (Configs == null || Configs.Length == 0)
        {
            Debug.LogError("[ComboConfigData] Configs array is empty");
            return new ComboConfig { DamageMultiplier = 1.0f };
        }
        
        foreach (var config in Configs)
        {
            if (currentCombo <= config.MaxComboInclusive)
            {
                return config;
            }
        }
        return Configs[^1];
    }
    
    private void OnValidate()
    {
        if (Configs == null || Configs.Length == 0) return;
        Array.Sort(Configs, (a, b) => a.MaxComboInclusive.CompareTo(b.MaxComboInclusive));
    }
}

[Serializable]
public class ComboConfig
{
    [Header("Combo Information")]
    public int MinCombo;
    public int MaxComboInclusive;
    public string ComboText;
    public float DamageMultiplier;
    
    [Header("Combo Effects")]
    public ComboEffectData EffectData;
}

[System.Serializable]
public class ComboEffectData
{
    public float TextPunchScale = 1.2f;
    public float PunchDuration = 0.3f;
    public Color TextColor = Color.white;
    // todo. Ease Data 추가
}