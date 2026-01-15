using System.Text;
using TMPro;
using UnityEngine;

public class UI_PlayerInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TextMeshProUGUI _playerStarText;
    
    private readonly StringBuilder _stringBuilder = new StringBuilder();
    [SerializeField] private int _totalStarCount;
    
    
    private void Start()
    {
        _playerNameText.SetText(PlayerDataManager.Instance.GetPlayerID());
        
        UpdatePlayerInfoUI();
    }
    
    private void UpdatePlayerInfoUI()
    {
        UpdateStarInfoUI();
    }
    
    private void UpdateStarInfoUI()
    {
        StageManager stageManager = StageManager.Instance;
        int playerStarCount = stageManager.GetPlayerStarCount();
        int totalStarCount = stageManager.GetTotalStars();
        _playerStarText.text = $"{playerStarCount} / {totalStarCount}";
    }
}
