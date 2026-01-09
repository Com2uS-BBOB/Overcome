using System.Collections;
using TMPro;
using UnityEngine;

public class UI_Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playTimeText;
    [SerializeField] private TextMeshProUGUI _changedValueText;
    [SerializeField] private TextMeshProUGUI _remainTimeText;

    private TimeSystem _timeSystem;
    private Coroutine _changeValueCoroutine;
    [SerializeField] private float _changeValueDuration;
    private void Start()
    {
        _timeSystem = TimeSystem.Instance;
        _timeSystem.OnRemainTimeDelta += UpdateChangeValueUI;
        ResetChangeValue();
    }

    private void Update()
    {
        UpdateRemainTimeUI();
        UpdatePlayerTimeUI();
    }
    
    private void OnDestroy()
    {
       _timeSystem.OnRemainTimeDelta -= UpdateChangeValueUI;
    }

    private void UpdateRemainTimeUI()
    {
        _remainTimeText.text = $"{_timeSystem.RemainTime:F2}s";
    }

    private void UpdatePlayerTimeUI()
    {
        _playTimeText.text = $"{_timeSystem.PlayTime:F2}s";
    }

    private void UpdateChangeValueUI(float value)
    {
        if (_changeValueCoroutine != null)
        {
            StopCoroutine(_changeValueCoroutine);
            _changeValueCoroutine = null;
        }
        _changeValueCoroutine = StartCoroutine(ChangeValueCoroutine(value));
    }

    private void SetChangeValue(float value)
    {
        if (value > 0)
        {
            _changedValueText.text = $"+{value:F1}s";
            _changedValueText.color = Color.lawnGreen;
        }
        else if (value < 0)
        {
            _changedValueText.text = $"{value:F1}s";
            _changedValueText.color = Color.red;
        }
        else
        {
            ResetChangeValue();
        }
    }

    private void ResetChangeValue()
    {
        _changedValueText.text = "";
    }

    private IEnumerator ChangeValueCoroutine(float value)
    {
        SetChangeValue(value);
        yield return new WaitForSeconds(_changeValueDuration);
        ResetChangeValue();
    }
}
