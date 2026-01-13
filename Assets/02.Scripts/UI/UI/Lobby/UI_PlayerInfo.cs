using TMPro;
using UnityEngine;

public class UI_PlayerInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TextMeshProUGUI _playerStarText;

    private void Start()
    {
        _playerNameText.SetText(PlayerDataManager.Instance.GetPlayerID());
        _playerStarText.SetText(PlayerDataManager.Instance.GetPlayerStarCount().ToString());
    }
}
