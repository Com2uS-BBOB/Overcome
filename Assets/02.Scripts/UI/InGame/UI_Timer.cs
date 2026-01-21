using _02.Scripts.Player.Common;
using TMPro;
using UnityEngine;

public class UI_Timer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI _playTimeText;
    [SerializeField] private TextMeshProUGUI _remainTimeText;
    [SerializeField] private RectTransform _changedValueStartPosition;
    [SerializeField] private TimeChangeItem _timeChangeItemPrefab;
    
    [Header("Change Value Settings")]
    [SerializeField] private float _horizontalOffsetRange = 30f;
    [SerializeField] private float _changeValueDuration = 1f;
    
    [Header("Pool Settings")]
    [SerializeField] private int _poolInitialSize = 20;

    private TimeSystem _timeSystem;
    private ObjectPool<TimeChangeItem> _timeChangeItemPool;

    private void Start()
    {
        _timeSystem = TimeSystem.Instance;
        _timeSystem.OnRemainTimeDelta += UpdateChangeValueUI;
        _timeSystem.OnTimeChanged += UpdateTimeUI;
        _timeChangeItemPool = new ObjectPool<TimeChangeItem>(_timeChangeItemPrefab, _changedValueStartPosition, _poolInitialSize);
        UpdateTimeUI();
    }

    private void UpdateTimeUI()
    {
        UpdateRemainTimeUI();
        UpdatePlayerTimeUI();
    }
    
    private void OnDestroy()
    {
        _timeSystem.OnRemainTimeDelta -= UpdateChangeValueUI;
        _timeChangeItemPool.Clear();
    }

    private void UpdateRemainTimeUI()
    {
        _remainTimeText.text = $"{_timeSystem.RemainTime:F2}s";
    }

    private void UpdatePlayerTimeUI()
    {
        int minutes = Mathf.FloorToInt(_timeSystem.PlayTime / 60f);
        int seconds = Mathf.FloorToInt(_timeSystem.PlayTime % 60f);
        _playTimeText.text = $"{minutes:D2}:{seconds:D2}";
    }

    private void UpdateChangeValueUI(float value)
    {
        if (Mathf.Approximately(value, 0f)) return;

        SpawnTimeChangeItem(value);
    }

    private void SpawnTimeChangeItem(float value)
    {
        TimeChangeItem item = _timeChangeItemPool.Get();

        float randomOffsetX = Random.Range(-_horizontalOffsetRange, _horizontalOffsetRange);
        Vector2 anchoredPosition = new Vector2(randomOffsetX, 0f);

        item.Init(value, _changeValueDuration, anchoredPosition, _timeChangeItemPool.Return);
    }
}
