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
        int playerStarCount = PlayerDataManager.Instance.GetPlayerStarCount();
        // todo. TotalStarCount 변경(Stage 개수 * 3개로 변경)
        _playerStarText.text = $"{playerStarCount} / {_totalStarCount}";
    }
}
