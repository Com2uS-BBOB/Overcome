using System;
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
        InitializeStartPlayer();
    }

    private void InitializeStartPlayer()
    {
        string targetID = string.IsNullOrEmpty(_startPlayerID)
            ? GenerateTempPlayerID()
            : _startPlayerID;

        if (!HasPlayer(targetID))
        {
            CreatePlayer(targetID);
        }

        SwitchPlayer(targetID);
        Debug.Log($"[PlayerDataManager] 현재 플레이어: {targetID}");
    }

    private string GenerateTempPlayerID()
    {
        return $"Player_{DateTime.Now:yyyyMMdd_HHmmss}";
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
        _saveData.Players ??= Array.Empty<PlayerData>();

        _currentPlayer = FindPlayer(_saveData.CurrentPlayerID);
    }

    private void SyncCurrentPlayerToSaveData()
    {
        if (_currentPlayer == null) return;

        for (int i = 0; i < _saveData.Players.Length; i++)
        {
            if (_saveData.Players[i].PlayerID == _currentPlayer.PlayerID)
            {
                _saveData.Players[i] = _currentPlayer;
                return;
            }
        }
    }

    #endregion

    #region Player Management

    public PlayerData[] GetAllPlayers() => _saveData.Players;
    public PlayerData GetCurrentPlayer() => _currentPlayer;
    public bool HasPlayer(string playerId) => FindPlayer(playerId) != null;

    public bool CreatePlayer(string playerId)
    {
        if (HasPlayer(playerId)) return false;

        var newPlayer = new PlayerData
        {
            PlayerID = playerId,
            Settings = new PlayerSettings(),
            StageProgress = Array.Empty<StageProgress>()
        };

        var newArray = new PlayerData[_saveData.Players.Length + 1];
        _saveData.Players.CopyTo(newArray, 0);
        newArray[^1] = newPlayer;
        _saveData.Players = newArray;

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
        int index = FindPlayerIndex(playerId);
        if (index < 0) return false;

        var newArray = new PlayerData[_saveData.Players.Length - 1];
        for (int i = 0, j = 0; i < _saveData.Players.Length; i++)
        {
            if (i != index)
            {
                newArray[j++] = _saveData.Players[i];
            }
        }
        _saveData.Players = newArray;

        if (_currentPlayer?.PlayerID == playerId)
        {
            _currentPlayer = _saveData.Players.Length > 0 ? _saveData.Players[0] : null;
            _saveData.CurrentPlayerID = _currentPlayer?.PlayerID;
        }

        return true;
    }

    private PlayerData FindPlayer(string playerId)
    {
        if (string.IsNullOrEmpty(playerId) || _saveData.Players == null) return null;

        foreach (PlayerData player in _saveData.Players)
        {
            if (player.PlayerID == playerId)
            {
                return player;
            }
        }

        return null;
    }

    private int FindPlayerIndex(string playerId)
    {
        if (_saveData.Players == null) return -1;

        for (int i = 0; i < _saveData.Players.Length; i++)
        {
            if (_saveData.Players[i].PlayerID == playerId) return i;
        }

        return -1;
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

    #region Stage Data (TODO: StageManager로 이동 예정)

    public StageProgress GetStageProgress(int stageId)
    {
        if (_currentPlayer?.StageProgress == null)
        {
            return null;
        }

        foreach (StageProgress progress in _currentPlayer.StageProgress)
        {
            if (progress.StageID == stageId)
            {
                return progress;
            }
        }

        return null;
    }

    public void UpdateStageProgress(int stageId, bool isCleared, int starsEarned, int score)
    {
        if (_currentPlayer == null) return;

        _currentPlayer.StageProgress ??= Array.Empty<StageProgress>();

        foreach (StageProgress stageProgress in _currentPlayer.StageProgress)
        {
            if (stageProgress.StageID != stageId)
            {
                UpdateExistingProgress(stageProgress, isCleared, starsEarned, score);
                return;
            }
        }

        AddNewStageProgress(stageId, isCleared, starsEarned, score);
    }

    private void UpdateExistingProgress(StageProgress progress, bool isCleared, int starsEarned, int score)
    {
        progress.IsCleared |= isCleared;
        progress.StarsEarned = Mathf.Max(progress.StarsEarned, starsEarned);
        progress.BestScore = Mathf.Max(progress.BestScore, score);
        progress.PlayCount++;
    }

    private void AddNewStageProgress(int stageId, bool isCleared, int starsEarned, int score)
    {
        var newProgress = new StageProgress
        {
            StageID = stageId,
            IsCleared = isCleared,
            StarsEarned = starsEarned,
            BestScore = score,
            PlayCount = 1
        };

        var newArray = new StageProgress[_currentPlayer.StageProgress.Length + 1];
        _currentPlayer.StageProgress.CopyTo(newArray, 0);
        newArray[^1] = newProgress;
        _currentPlayer.StageProgress = newArray;
    }

    #endregion

#if UNITY_EDITOR
    #region Editor Test

    [ContextMenu("Test/Save Data")]
    private void TestSave()
    {
        SaveData();
        Debug.Log($"[PlayerDataManager] 저장 완료: {Application.persistentDataPath}/GameSaveData.json");
    }

    [ContextMenu("Test/Load Data")]
    private void TestLoad()
    {
        LoadData();
        Debug.Log($"[PlayerDataManager] 로드 완료 - 플레이어 수: {_saveData.Players.Length}, 현재: {GetPlayerID()}");
    }
    #endregion
#endif
}
