using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_CrescentGauge : MonoBehaviour
{
    [SerializeField] private Gauge _gauge;

    private void Awake()
    {
        _gauge.Init();
    }

    private void OnDestroy()
    {
        _gauge.Clear();        
    }
    
    public void SetGauge(float value)
    {
        _gauge.SetValue(value);
    }
    
    #region Test Code
    public void IncreaseGauge(float value)
    {
        
    }
    #endregion
}