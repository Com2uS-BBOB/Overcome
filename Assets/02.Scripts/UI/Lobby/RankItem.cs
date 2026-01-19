using TMPro;
using UnityEngine;

public class RankItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _userName;
    [SerializeField] private TextMeshProUGUI _score;

    public void SetRankInfo(RankConfig config)
    {
        _userName.text = config.UserID;
        _score.text = config.Score.ToString("N0");
    }
}
