using System.Linq;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public string PlayerID;
    public PlayerSettings Settings;
    // public CustomizationData customization;
    public StageProgress[] StageProgress;

    public int TotalStarsEarned => StageProgress?.Sum(s => s.StarsEarned) ?? 0;
}