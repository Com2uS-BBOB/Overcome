using UnityEngine;
using UnityEngine.UI;

public class TestTimerUI : MonoBehaviour
{
    public Text PlayerTimeText;
    public Text ChangeValueText;
    public Text RemainTimeText;

    private TimeSystem _timeSystem;
    private void Start()
    {
        _timeSystem = TimeSystem.Instance;
        _timeSystem.OnPlayTimeChanged += UpdatePlayerTimeUI;
        _timeSystem.OnRemainTimeChanged += UpdateRemainTimeUI;
        _timeSystem.OnRemainTimeDelta += UpdateChangeValueUI;
        UpdatePlayerTimeUI();
        UpdateRemainTimeUI();
        UpdateChangeValueUI(0);
    }

    private void OnDestroy()
    {
       _timeSystem.OnPlayTimeChanged -= UpdatePlayerTimeUI;
       _timeSystem.OnRemainTimeChanged -= UpdateRemainTimeUI;
       _timeSystem.OnRemainTimeDelta -= UpdateChangeValueUI;
    }

    public void UpdateRemainTimeUI()
    {
        int value = Mathf.RoundToInt(_timeSystem.RemainTime);
        RemainTimeText.text = $"{value}";
    }

    public void UpdatePlayerTimeUI()
    {
        int value = Mathf.RoundToInt(_timeSystem.PlayTime);
        PlayerTimeText.text = $"{value}";
    }

    public void UpdateChangeValueUI(float value)
    {
        if (value > 0)
        {
            ChangeValueText.text = $"+{value}";
            ChangeValueText.color = Color.lawnGreen;
        }
        else if (value < 0)
        {
            ChangeValueText.text = $"{value}";
            ChangeValueText.color = Color.red;
        }
        else
        {
            ChangeValueText.text = "";
        }
    }
}
