using TMPro;
using UnityEngine;

public class UI_PlayerInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TextMeshProUGUI _playerStarText;

    private void Start()
    {
        _playerNameText.SetText(DataManager.Instance.GetPlayerID());
        _playerStarText.SetText(DataManager.Instance.GetPlayerStarCount().ToString());
    }
}
