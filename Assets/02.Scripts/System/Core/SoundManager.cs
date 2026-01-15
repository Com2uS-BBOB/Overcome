using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using DG.Tweening;

public enum AudioType
{
    Master,
    Music,
    Effect
}

public class SoundManager : SingletonBehaviour<SoundManager>
{
    [Header("Audio Settings")]
    [SerializeField] private AudioMixer _audioMixer;
    [SerializeField] private AudioMixerGroup _musicGroup;
    [SerializeField] private AudioMixerGroup _effectGroup;

    [Header("AudioSource Settings")]
    [SerializeField] private int _sfxPoolSize = 15;

    [Header("Common Settings")]
    [Tooltip("게임 전체 동안 유지할 공통 사운드 라벨 (예: UISound_Common)")]
    [SerializeField] private string _commonSoundLabel = "Common";

    private AudioSource _bgmSource;
    private readonly List<AudioSource> _sfxPool = new List<AudioSource>();

    // clipName(Address 또는 이름) -> AudioClip 캐시
    private readonly Dictionary<string, AudioClip> _sceneClips = new Dictionary<string, AudioClip>();
    private readonly Dictionary<string, AudioClip> _commonClips = new Dictionary<string, AudioClip>();

    // Common 라벨 로드 핸들 (게임 끝날 때 한 번에 Release)
    private readonly List<AsyncOperationHandle> _commonHandles = new List<AsyncOperationHandle>();

    private float _masterVolume = 1f;
    private float _musicVolume = 1f;
    private float _effectVolume = 1f;
    private Tween _bgmFadeTween;

    public AudioMixer Mixer => _audioMixer;

    protected override void Init()
    {
        if (_audioMixer == null)
        {
            Debug.LogError("[SoundManager] AudioMixer가 할당되지 않았습니다!");
            return;
        }

        InitializeAudioSources();
        _ = PreloadCommonSoundsAsync();
    }

    private void Start()
    {
        LoadVolumeSettings();
    }
    
    private void InitializeAudioSources()
    {
        // BGM Source
        GameObject bgmObject = new GameObject("BGM_Source");
        bgmObject.transform.SetParent(transform);
        _bgmSource = bgmObject.AddComponent<AudioSource>();
        _bgmSource.outputAudioMixerGroup = _musicGroup;
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;
        _bgmSource.spatialBlend = 0f;

        // SFX Pool
        for (int i = 0; i < _sfxPoolSize; i++)
        {
            CreateSfxAudioSource(i);
        }
    }

    #region Volume Setting
    private void LoadVolumeSettings()
    {
        var settings = PlayerDataManager.Instance?.GetSettings();
        if (settings != null)
        {
            SetAudioVolume(AudioType.Master, settings.MasterVolume);
            SetAudioVolume(AudioType.Music, settings.MusicVolume);
            SetAudioVolume(AudioType.Effect, settings.SfxVolume);
        }
        else
        {
            SetAudioVolume(AudioType.Master, 1f);
            SetAudioVolume(AudioType.Music, 1f);
            SetAudioVolume(AudioType.Effect, 1f);
        }
    }

    public void SetAudioVolume(AudioType audioType, float volume)
    {
        volume = Mathf.Clamp01(volume);
        float value = Mathf.Max(0.0001f, volume);

        switch (audioType)
        {
            case AudioType.Master:
                _masterVolume =  volume;
                break;
            case AudioType.Music:
                _musicVolume =  volume;
                break;
            case AudioType.Effect:
                _effectVolume =  volume;
                break;
        }

        _audioMixer?.SetFloat(audioType.ToString(), Mathf.Log10(value) * 20);
        SaveVolumeSettings();
    }

    public float GetVolume(AudioType audioType)
    {
        if (audioType == AudioType.Master) return _masterVolume;
        return audioType == AudioType.Music ? _musicVolume : _effectVolume;
    }

    public void PauseAudio()
    {
        _audioMixer?.SetFloat(nameof(AudioType.Effect), Mathf.Log10(0.0001f) * 20);
    }

    public void ResumeAudio()
    {
        SetAudioVolume(AudioType.Effect, _effectVolume);
    }

    private void SaveVolumeSettings()
    {
        PlayerDataManager playerDataManager = PlayerDataManager.Instance;
        if (playerDataManager == null) return;
        PlayerSettings settings = playerDataManager.GetSettings();
        if (settings != null)
        {
            playerDataManager.SetSettings(
                _masterVolume,
                _musicVolume,
                _effectVolume
            );
        }
    }
    #endregion

    #region BGM Control
    public async void PlayBGM(string clipName, float fadeTime = 0f)
    {
        if (_bgmSource.isPlaying && fadeTime > 0f)
        {
            await FadeBGMAsync(0f, fadeTime);
        }
        else
        {
            _bgmSource.Stop();
        }

        var clip = await LoadAudioClipAsync(clipName);
        if (clip == null) return;

        _bgmSource.clip = clip;
        _bgmSource.Play();

        if (fadeTime > 0f)
        {
            _bgmSource.volume = 0f;
            await FadeBGMAsync(1f, fadeTime);
        }
        else
        {
            _bgmSource.volume = 1f;
        }
    }

    public async void StopBGM(float fadeTime = 0f)
    {
        if (fadeTime > 0f) await FadeBGMAsync(0f, fadeTime);

        _bgmSource.Stop();
        _bgmSource.clip = null;
    }

    public void PauseBGM() => _bgmSource.Pause();
    public void ResumeBGM() => _bgmSource.UnPause();

    private async Task FadeBGMAsync(float targetVolume, float duration)
    {
        _bgmFadeTween?.Kill();
        _bgmFadeTween = _bgmSource.DOFade(targetVolume, duration);
        await _bgmFadeTween.AsyncWaitForCompletion();
    }
    #endregion

    #region SFX Control
    
    /// <summary>
    /// 평면적인 SFX 실행 (UI SFX 등)
    /// </summary>
    public void PlaySfx(string clipName)
    {
        _= PlaySfxInternalAsync(clipName, Vector3.zero, null, true);
    }

    /// <summary>
    /// Position에 SFX 실행 (피격, 타격 등)
    /// </summary>
    public void PlaySfx(string clipName, Vector3 position)
    {
        _=  PlaySfxInternalAsync(clipName, position, null, false);
    }

    /// <summary>
    /// Transform을 따라다니며 SFX 실행 (플레이어, Enemy 이동 SFX 등)
    /// </summary>
    public void PlaySfx(string clipName, Transform target)
    {
        _= PlaySfxInternalAsync(clipName, Vector3.zero, target, false);
    }

    private async Task PlaySfxInternalAsync(string clipName, Vector3 position, Transform target, bool is2D)
    {
        AudioClip clip = await LoadAudioClipAsync(clipName);
        if (clip == null)
        {
            Debug.LogError($"[SoundManager] SFX 로드 실패: {clipName}");
            return;
        }

        AudioSource source = GetAvailableSfxSource();
        if (source == null)
        {
            Debug.LogWarning($"[SoundManager] 사용 가능한 SFX Source가 없음: {clipName}");
            return;
        }

        source.clip = clip;
        source.spatialBlend = is2D ? 0f : 1f;

        if (target != null)
        {
            source.transform.SetParent(target);
            source.transform.localPosition = Vector3.zero;
        }
        else
        {
            source.transform.SetParent(transform);
            source.transform.position = position;
        }

        source.gameObject.SetActive(true);
        source.Play();

        await WaitForSfxFinishedAsync(source);
    }

    private async Task WaitForSfxFinishedAsync(AudioSource source)
    {
        while (source != null && source.isPlaying)
        {
            await Task.Yield();
        }

        if (source == null) return;

        // todo. Pool 반환 방식으로 수정
        source.clip = null;
        source.transform.SetParent(transform);
        source.gameObject.SetActive(false);
    }

    private AudioSource GetAvailableSfxSource()
    {
        // todo. Object Pooling 방식 수정 필요
        foreach (AudioSource source in _sfxPool)
        {
            if (!source.gameObject.activeSelf) 
            {
                return source;
            }
        }
        return CreateSfxAudioSource();
    }

    public void StopAllSfx()
    {
        foreach (AudioSource source in _sfxPool)
        {
            if (!source.isPlaying) continue;
            source.Stop();
            source.clip = null;
            source.gameObject.SetActive(false);
        }
    }

    private AudioSource CreateSfxAudioSource(int i = -1)
    {
        int index = (i == -1) ? _sfxPool.Count : i;
        GameObject sfxObject = new GameObject($"SFX_Source_{index}");
        sfxObject.transform.SetParent(transform);
        AudioSource newSource = sfxObject.AddComponent<AudioSource>();
        newSource.outputAudioMixerGroup = _effectGroup;
        newSource.playOnAwake = false;
        newSource.spatialBlend = 1f;
        sfxObject.SetActive(false);
        _sfxPool.Add(newSource);
        return newSource;
    }
    #endregion
    
    #region Load Resource
    private async Task PreloadCommonSoundsAsync()
    {
        if (string.IsNullOrEmpty(_commonSoundLabel)) return;

        AsyncOperationHandle<IList<AudioClip>> handle =
            Addressables.LoadAssetsAsync<AudioClip>(
                _commonSoundLabel,
                clip =>
                {
                    if (clip == null) return;
                    _commonClips.TryAdd(clip.name, clip);
                });

        _commonHandles.Add(handle);
        await handle.Task;
    }

    // clipName과 실제 Addressables에 등록된 이름과 일치해야 한다.
    private async Task<AudioClip> LoadAudioClipAsync(string clipName)
    {
        if (string.IsNullOrEmpty(clipName)) return null;
        if (_sceneClips.TryGetValue(clipName, out AudioClip sceneCache))
        {
            return sceneCache;
        }
        if (_commonClips.TryGetValue(clipName, out AudioClip commonCache))
        {
            return commonCache;
        }

        AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>(clipName);
        AudioClip clip = await handle.Task;
        if (handle.Status != AsyncOperationStatus.Succeeded || clip == null)                        
        {                                                                                           
            if (handle.IsValid()) Addressables.Release(handle);
            return null;                                                                            
        }  

        _sceneClips[clipName] = clip;
        return clip;
    }

    // todo. SceneChage 시점 고려 후 구독 필요
    private void ReleaseCurrentSceneAudioClips()
    {
        foreach (KeyValuePair<string, AudioClip> kv in _sceneClips)
        {
            if (kv.Value == null) continue;
            Addressables.Release(kv.Value);
        }
        _sceneClips.Clear();
    }

    private void ReleaseCommonAudioClips()
    {
        foreach (KeyValuePair<string, AudioClip> kv in _commonClips)
        {
            if (kv.Value == null) continue;
            Addressables.Release(kv.Value);
        }
        _commonClips.Clear();
        
        foreach (AsyncOperationHandle handle in _commonHandles)
        {
            if (!handle.IsValid()) continue;
            Addressables.Release(handle);
        }
        _commonHandles.Clear();
    }
    
    private void ReleaseAllAudioClips()
    {
        ReleaseCurrentSceneAudioClips();
        ReleaseCommonAudioClips();
    }
    #endregion

#if UNITY_EDITOR
    [Header("Test Settings")]
    [SerializeField] private AudioType _testAudioType = AudioType.Music;
    [SerializeField] [Range(0f, 1f)] private float _testVolume = 0.5f;
    [SerializeField] private string _testBGMName = "MainTheme";
    [SerializeField] private string _testSfxName = "Click";
    [SerializeField] private float _testFadeTime = 1f;
    
    [ContextMenu("Test/Set Volume")]
    private void TestSetVolume()
    {
        SetAudioVolume(_testAudioType, _testVolume);
        Debug.Log($"[SoundManager] {_testAudioType} 볼륨 설정: {_testVolume}");
    }

    [ContextMenu("Test/Get Volume")]
    private void TestGetVolume()
    {
        float volume = GetVolume(_testAudioType);
        Debug.Log($"[SoundManager] {_testAudioType} 현재 볼륨: {volume}");
    }

    [ContextMenu("Test/Play BGM")]
    private void TestPlayBGM()
    {
        PlayBGM(_testBGMName, _testFadeTime);
        Debug.Log($"[SoundManager] BGM 재생: {_testBGMName}");
    }

    [ContextMenu("Test/Play SFX 2D")]
    private void TestPlaySfx2D()
    {
        PlaySfx(_testSfxName);
        Debug.Log($"[SoundManager] SFX 2D 재생: {_testSfxName}");
    }

    [ContextMenu("Test/Play SFX 3D")]
    private void TestPlaySfx3D()
    {
        PlaySfx(_testSfxName, Vector3.zero);
        Debug.Log($"[SoundManager] SFX 3D 재생: {_testSfxName}");
    }

    [ContextMenu("Test/Release All Clips")]
    private void TestReleaseAll()
    {
        ReleaseAllAudioClips();
    }
#endif
}