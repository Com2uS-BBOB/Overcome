using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerDataManager : SingletonBehaviour<PlayerDataManager>
{
    private const string SaveKey = "GameSaveData";
    private PlayerData _currentPlayer;

    [Header("Start Settings")]
    [SerializeField] private string _startPlayerID;
    
    [Header("Debug")]
    [SerializeField] private GameSaveData _saveData;

    protected override void Init()
    {
        LoadData();
    }

    public bool InitializeStartPlayer(string playerName)
    {
        bool existingUser = true;
        if (!HasPlayer(playerName))
        {
            RegistNewPlayer(playerName);
            existingUser = false;
        }

        SwitchPlayer(playerName);
        Debug.Log($"[PlayerDataManager] 현재 플레이어: {playerName}");
        return existingUser;
    }

    private void OnApplicationQuit()
    {
        SaveData();
        Debug.Log("[PlayerDataManager] 종료 시 자동 저장 완료");
    }

    #region Save/Load

    public void SaveData()
    {
        SyncCurrentPlayerToSaveData();
        JsonSaveService.Save(SaveKey, _saveData);
    }

    private void LoadData()
    {
        _saveData = JsonSaveService.Load<GameSaveData>(SaveKey);
        _saveData.Players ??= new List<PlayerData>();

        _currentPlayer = FindPlayer(_saveData.CurrentPlayerID);
    }

    private void SyncCurrentPlayerToSaveData()
    {
        if (_currentPlayer == null) return;

        int index = _saveData.Players.FindIndex(p => p.PlayerID == _currentPlayer.PlayerID);
        if (index >= 0)
        {
            _saveData.Players[index] = _currentPlayer;
        }
    }

    #endregion

    #region Player Management

    public List<PlayerData> GetAllPlayers() => _saveData.Players;
    public PlayerData GetCurrentPlayer() => _currentPlayer;
    public bool HasPlayer(string playerId) => FindPlayer(playerId) != null;

    public bool RegistNewPlayer(string playerId)
    {
        if (HasPlayer(playerId)) return false;

        var newPlayer = new PlayerData
        {
            PlayerID = playerId,
            Settings = new PlayerSettings(),
            StageProgress = new List<StageProgress>()
        };

        _saveData.Players.Add(newPlayer);
        return true;
    }

    public bool SwitchPlayer(string playerId)
    {
        var player = FindPlayer(playerId);
        if (player == null) return false;

        SyncCurrentPlayerToSaveData();
        _currentPlayer = player;
        _saveData.CurrentPlayerID = playerId;

        return true;
    }

    public bool DeletePlayer(string playerId)
    {
        int index = _saveData.Players.FindIndex(p => p.PlayerID == playerId);
        if (index < 0) return false;

        _saveData.Players.RemoveAt(index);

        if (_currentPlayer?.PlayerID == playerId)
        {
            _currentPlayer = _saveData.Players.Count > 0 ? _saveData.Players[0] : null;
            _saveData.CurrentPlayerID = _currentPlayer?.PlayerID;
        }

        return true;
    }

    private PlayerData FindPlayer(string playerId)
    {
        if (string.IsNullOrEmpty(playerId) || _saveData.Players == null) return null;
        return _saveData.Players.Find(p => p.PlayerID == playerId);
    }

    #endregion

    #region Current Player Data

    public string GetPlayerID() => _currentPlayer?.PlayerID;
    public int GetPlayerStarCount() => _currentPlayer?.TotalStarsEarned ?? 0;
    public PlayerSettings GetSettings() => _currentPlayer?.Settings;

    public void SetSettings(float masterVolume, float musicVolume, float sfxVolume)
    {
        if (_currentPlayer == null) return;

        _currentPlayer.Settings ??= new PlayerSettings();
        _currentPlayer.Settings.MasterVolume = masterVolume;
        _currentPlayer.Settings.MusicVolume = musicVolume;
        _currentPlayer.Settings.SfxVolume = sfxVolume;
    }

    #endregion

    #region Stage Data

    public List<StageProgress> GetAllStageProgress()
    {
        return _currentPlayer?.StageProgress ?? new List<StageProgress>();
    }

    public StageProgress GetStageProgress(int stageId)
    {
        return _currentPlayer?.StageProgress?.Find(p => p.StageID == stageId);
    }

    public void SaveStageProgress(StageProgress progress)
    {
        if (_currentPlayer == null)
        {
            Debug.LogError("[PlayerDataManager] SaveStageProgress: _currentPlayer is null");
            return;
        }

        _currentPlayer.StageProgress ??= new List<StageProgress>();

        // 기존 데이터 찾기
        int index = _currentPlayer.StageProgress.FindIndex(p => p.StageID == progress.StageID);
        
        if (index >= 0)
        {
            // 업데이트
            _currentPlayer.StageProgress[index] = progress;
        }
        else
        {
            // 새로 추가
            _currentPlayer.StageProgress.Add(progress);
        }

        SaveData();
    }

    #endregion
}