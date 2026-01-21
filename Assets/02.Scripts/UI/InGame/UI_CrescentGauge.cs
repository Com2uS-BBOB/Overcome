using _02.Scripts.Player.Gauge;
using UnityEngine;

public class UI_CrescentGauge : MonoBehaviour
{
    [SerializeField] private Gauge _gauge;
    [SerializeField] private GaugeManager _gaugeManager;
    [SerializeField] private GameObject[] _gaugeBorders;
    
    private void Awake()
    {
        _gauge.Init(1f);
        _gaugeManager.OnCrescentGaugeChanged += SetGauge;
    }

    private void OnDestroy()
    {
        _gauge.Clear();        
    }
    
    private void SetGauge(float value, float max)
    {
        float nextValue = value / max;
        SetBorder(nextValue);
        _gauge.SetValue(nextValue);
    }

    private void SetBorder(float nextValue)
    {
        int maxValue = _gaugeBorders.Length;
        int activeCount = Mathf.Clamp(Mathf.FloorToInt(nextValue * maxValue), 0, maxValue);
        for (var i = 0; i < _gaugeBorders.Length; i++)
        {
            _gaugeBorders[i].SetActive(i < activeCount);
        }
    }
}