using Unity.Cinemachine;
using UnityEngine;

public class CameraSensitivityManager : MonoBehaviour
{
    [SerializeField] private CinemachineInputAxisController _inputAxisController;

    private float _sensitivity = 1.0f;
    private const float DefaultGainX = 1.0f;
    private const float DefaultGainY = -1.0f;

    public static CameraSensitivityManager Instance { get; private set; }

    public float Sensitivity => _sensitivity;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        LoadSensitivity();
    }

    public void SetSensitivity(float value)
    {
        _sensitivity = Mathf.Clamp(value, 0.1f, 3.0f);
        UpdateControllerGain();
        SaveSensitivity();
    }

    private void UpdateControllerGain()
    {
        if (_inputAxisController == null) return;

        var controllers = _inputAxisController.Controllers;
        foreach (var controller in controllers)
        {
            if (controller.Input == null) continue;

            if (controller.Name.Contains("X"))
            {
                controller.Input.Gain = DefaultGainX * _sensitivity;
            }
            else if (controller.Name.Contains("Y"))
            {
                controller.Input.Gain = DefaultGainY * _sensitivity;
            }
        }
    }

    private void LoadSensitivity()
    {
        if (PlayerDataManager.Instance != null)
        {
            _sensitivity = PlayerDataManager.Instance.GetMouseSensitivity();
        }
        UpdateControllerGain();
    }

    private void SaveSensitivity()
    {
        PlayerDataManager.Instance?.SetMouseSensitivity(_sensitivity);
    }
}