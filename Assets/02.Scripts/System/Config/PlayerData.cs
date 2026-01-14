using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class PlayerData
{
    public string PlayerID;
    public PlayerSettings Settings;
    // public CustomizationData customization;
    public List<StageProgress> StageProgress;

    public int TotalStarsEarned => StageProgress?.Sum(s => s.StarsEarned) ?? 0;
}