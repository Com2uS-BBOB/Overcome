using System;
using UnityEngine;
using TMPro;

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

public enum ColorMode
{
    Solid,
    Gradient
}

[Serializable]
public class ComboConfig
{
    [Header("Combo Information")]
    public int MinCombo;
    public int MaxComboInclusive;
    public string ComboText;
    public float DamageMultiplier;

    [Header("Combo Color Effects")]
    public ColorMode ComboColorMode;
    public Color ComboColor;
    public VertexGradient ComboGradient;

    [Header("Grade Color Effects")]
    public ColorMode GradeColorMode;
    public Color GradeColor;
    public VertexGradient GradeGradient;

    public VertexGradient GetComboVertexGradient()
    {
        return GetVertexGradient(ComboColorMode, ComboColor, ComboGradient);
    }

    public VertexGradient GetGradeVertexGradient()
    {
        return GetVertexGradient(GradeColorMode, GradeColor, GradeGradient);
    }

    private VertexGradient GetVertexGradient(ColorMode mode, Color solidColor, VertexGradient gradient)
    {
        return mode == ColorMode.Solid ? new VertexGradient(solidColor) : gradient;
    }
}
