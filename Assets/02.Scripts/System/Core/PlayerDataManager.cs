using UnityEngine;

public class PlayerDataManager : SingletonBehaviour<PlayerDataManager>
{
    [Header("Test Data")]
    [SerializeField] private PlayerData _playerData;

    protected override void Init()
    {
        LoadData();
    }

    private void SaveData()
    {
        
    }

    private void LoadData()
    {
        // todo. PC에 저장된 Data 기반 Player Data 세팅
        
    }
    
    public int GetPlayerStarCount() => _playerData.TotalStarsEarned;
    public string GetPlayerID() => _playerData.PlayerID;
}
