using _02.Scripts.Player.Gauge;
using UnityEngine;

public class UI_CrescentGauge : MonoBehaviour
{
    [SerializeField] private Gauge _gauge;
    [SerializeField] private GaugeManager _gaugeManager;
    
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
        _gauge.SetValue(nextValue);
    }
}