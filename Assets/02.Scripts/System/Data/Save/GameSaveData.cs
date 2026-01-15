using System;
using System.Collections.Generic;

[Serializable]
public class GameSaveData
{
    public string CurrentPlayerID;
    public List<PlayerData> Players;
}
