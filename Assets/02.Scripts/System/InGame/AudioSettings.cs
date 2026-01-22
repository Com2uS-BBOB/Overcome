using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    [Serializable]
    private struct AudioSlider
    {
        public TextMeshProUGUI ValueText;
        public Slider Slider;

        public void SetAudioVolume(float value)
        {
            ValueText.text = value.ToString("F3");
            Slider.value = value;
        }
    }
    
    [Header("Audio Sliders")]
    [SerializeField] private AudioSlider _masterSlider;
    [SerializeField] private AudioSlider _musicSlider;
    [SerializeField] private AudioSlider _sfxSlider;
    private const string Format = "F3";

    private void OnEnable()
    {
        RegisterSliderListeners();
        InitializeSliders();
    }

    private void OnDisable()
    {
        UnregisterSliderListeners();
    }
    
    private void InitializeSliders()
    {
        float masterVolume = SoundManager.Instance.GetVolume(EAudioType.Master);
        _masterSlider.SetAudioVolume(masterVolume);
        float musicVolume = SoundManager.Instance.GetVolume(EAudioType.Music);
        _musicSlider.SetAudioVolume(musicVolume);
        float sfxVolume = SoundManager.Instance.GetVolume(EAudioType.Effect);
        _sfxSlider.SetAudioVolume(sfxVolume);
    }

    private void RegisterSliderListeners()
    {
        if (_masterSlider.Slider != null)
        {
            _masterSlider.Slider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            _masterSlider.Slider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (_musicSlider.Slider != null)
        {
            _musicSlider.Slider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            _musicSlider.Slider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (_sfxSlider.Slider != null)
        {
            _sfxSlider.Slider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            _sfxSlider.Slider.onValueChanged.AddListener(OnSfxVolumeChanged);
        }
    }

    private void UnregisterSliderListeners()
    {
        if (_masterSlider.Slider != null)
        {
            _masterSlider.Slider.onValueChanged.RemoveListener(OnMasterVolumeChanged);
        }

        if (_musicSlider.Slider != null)
        {
            _musicSlider.Slider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        }

        if (_sfxSlider.Slider != null)
        {
            _sfxSlider.Slider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        _masterSlider.ValueText.text = value.ToString(Format);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetAudioVolume(EAudioType.Master, value);
        }
    }

    private void OnMusicVolumeChanged(float value)
    {
        _musicSlider.ValueText.text = value.ToString(Format);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetAudioVolume(EAudioType.Music, value);
        }
    }

    private void OnSfxVolumeChanged(float value)
    {
        _sfxSlider.ValueText.text = value.ToString(Format);

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.SetAudioVolume(EAudioType.Effect, value);
        }
    }

}
