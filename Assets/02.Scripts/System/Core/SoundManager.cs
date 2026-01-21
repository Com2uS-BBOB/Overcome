using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using _02.Scripts.Player.Common;

public enum EAudioType
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

    [Header("Pool Settings")]
    [SerializeField] private AudioSource _audioSourcePrefab;
    [SerializeField] private int _sfxPoolSize = 15;

    [Header("Common Settings")]
    [SerializeField] private string _commonSoundLabel = "Common";

    private AudioSource _bgmSource;
    private ObjectPool<AudioSource> _sfxPool;

    // clipName(Address 또는 이름) -> AudioClip 캐시
    private readonly Dictionary<string, AudioClip> _sceneClips = new Dictionary<string, AudioClip>();
    private readonly Dictionary<string, AudioClip> _commonClips = new Dictionary<string, AudioClip>();

    // Addressables Handle 저장 (메모리 누수 방지)
    private readonly Dictionary<string, AsyncOperationHandle<AudioClip>> _sceneHandles = new Dictionary<string, AsyncOperationHandle<AudioClip>>();
    private readonly List<AsyncOperationHandle> _commonHandles = new List<AsyncOperationHandle>();

    // 중복 로드 방지를 위한 Task 캐싱
    private readonly Dictionary<string, Task<AudioClip>> _loadingTasks = new Dictionary<string, Task<AudioClip>>();

    private float _masterVolume = 1f;
    private float _musicVolume = 1f;
    private float _effectVolume = 1f;

    public AudioMixer Mixer => _audioMixer;

    protected override void Init()
    {
        if (_audioMixer == null)
        {
            Debug.LogError("[SoundManager] AudioMixer가 할당되지 않았습니다!");
            enabled = false;
            return;
        }

        InitializeAudioSources();
        _ = PreloadCommonSoundsAsync();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        LoadVolumeSettings();
    }

    protected override void Clear()
    {
        _bgmSource?.Stop();
        _sfxPool?.Clear();
        ReleaseAllAudioClips();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[SoundManager] Scene 변경 감지: {scene.name} - Scene 오디오 클립 해제");
        ReleaseCurrentSceneAudioClips();
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

        // SFX Pool 초기화
        _sfxPool = new ObjectPool<AudioSource>(_audioSourcePrefab, transform, _sfxPoolSize);
    }

    #region Volume Setting
    private void LoadVolumeSettings()
    {
        var settings = PlayerDataManager.Instance?.GetSettings();
        if (settings != null)
        {
            Debug.Log($"[SoundManager] 저장된 볼륨 로드 - Master: {settings.MasterVolume}, Music: {settings.MusicVolume}, Effect: {settings.SfxVolume}");
            SetAudioVolume(EAudioType.Master, settings.MasterVolume);
            SetAudioVolume(EAudioType.Music, settings.MusicVolume);
            SetAudioVolume(EAudioType.Effect, settings.SfxVolume);
        }
        else
        {
            Debug.Log("[SoundManager] 저장된 설정 없음 - 기본값 1f 적용");
            SetAudioVolume(EAudioType.Master, 1f);
            SetAudioVolume(EAudioType.Music, 1f);
            SetAudioVolume(EAudioType.Effect, 1f);
        }
    }

    public void SetAudioVolume(EAudioType eAudioType, float volume)
    {
        volume = Mathf.Clamp01(volume);
        float value = Mathf.Max(0.0001f, volume);

        switch (eAudioType)
        {
            case EAudioType.Master:
                _masterVolume = volume;
                break;
            case EAudioType.Music:
                _musicVolume = volume;
                break;
            case EAudioType.Effect:
                _effectVolume = volume;
                break;
        }

        _audioMixer.SetFloat(eAudioType.ToString(), Mathf.Log10(value) * 20);
        SaveVolumeSettings();
    }

    public float GetVolume(EAudioType eAudioType)
    {
        if (eAudioType == EAudioType.Master) return _masterVolume;
        return eAudioType == EAudioType.Music ? _musicVolume : _effectVolume;
    }

    public void PauseAudio() => _audioMixer.SetFloat(nameof(EAudioType.Master), Mathf.Log10(0.0001f) * 20);
    public void ResumeAudio() => SetAudioVolume(EAudioType.Master, _masterVolume);

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
    public void PlayBGM(string clipName)
    {
        PlayBGMAsync(clipName);
    }

    private async void PlayBGMAsync(string clipName)
    {
        try
        {
            Debug.Log($"[SoundManager] PlayBGM 시작: {clipName}");
            _bgmSource.Stop();

            var clip = await LoadAudioClipAsync(clipName);
            if (clip == null)
            {
                Debug.LogError($"[SoundManager] BGM 클립 로드 실패: {clipName}");
                return;
            }

            Debug.Log($"[SoundManager] BGM 클립 로드 성공: {clip.name}, 길이: {clip.length}초");
            _bgmSource.clip = clip;
            _bgmSource.volume = 1f;
            _bgmSource.Play();
            Debug.Log($"[SoundManager] BGM 재생 시작, isPlaying: {_bgmSource.isPlaying}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public void StopBGM()
    {
        _bgmSource.Stop();
        _bgmSource.clip = null;
    }

    public void PauseBGM() => _bgmSource.Pause();
    public void ResumeBGM() => _bgmSource.UnPause();
    #endregion

    #region SFX Control
    /// <summary>
    /// 평면적인 SFX 실행 (UI SFX 등)
    /// </summary>
    public void PlaySfx(string clipName)
    {
        PlaySfxInternalAsync(clipName, Vector3.zero, null, true);
    }

    /// <summary>
    /// Position에 SFX 실행 (피격, 타격 등)
    /// </summary>
    public void PlaySfx(string clipName, Vector3 position)
    {
        PlaySfxInternalAsync(clipName, position, null, false);
    }

    /// <summary>
    /// Transform을 따라다니며 SFX 실행 (플레이어, Enemy 이동 SFX 등)
    /// </summary>
    public void PlaySfx(string clipName, Transform target)
    {
        PlaySfxInternalAsync(clipName, Vector3.zero, target, false);
    }

    private async void PlaySfxInternalAsync(string clipName, Vector3 position, Transform target, bool is2D)
    {
        try
        {
            AudioClip clip = await LoadAudioClipAsync(clipName);
            if (clip == null)
            {
                Debug.LogError($"[SoundManager] SFX 로드 실패: {clipName}");
                return;
            }

            AudioSource source = _sfxPool.Get();
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
            source.Play();

            StartCoroutine(WaitForSfxFinishedCoroutine(source));
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private IEnumerator WaitForSfxFinishedCoroutine(AudioSource source)
    {
        if (source == null || source.clip == null) yield break;

        // AudioSource.isPlaying을 직접 체크 (Time.time 대신)
        while (source != null && source.isPlaying)
        {
            yield return null;
        }

        if (source == null) yield break;

        source.clip = null;
        source.transform.SetParent(transform);
        _sfxPool.Return(source);
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

    /// <summary>
    /// ! 실제 Addressables에 등록된 clipName 사용.
    /// </summary>
    private async Task<AudioClip> LoadAudioClipAsync(string clipName)
    {
        if (string.IsNullOrEmpty(clipName)) return null;

        if (_sceneClips.TryGetValue(clipName, out AudioClip sceneCache)) return sceneCache;
        if (_commonClips.TryGetValue(clipName, out AudioClip commonCache)) return commonCache;
        if (_loadingTasks.TryGetValue(clipName, out Task<AudioClip> loadingTask)) return await loadingTask;
        
        
        Task<AudioClip> newLoadTask = LoadAudioClipInternalAsync(clipName);
        _loadingTasks[clipName] = newLoadTask;
        try
        {
            AudioClip clip = await newLoadTask;
            return clip;
        }
        finally
        {
            // 로딩 완료 후 Task 캐시에서 제거
            _loadingTasks.Remove(clipName);
        }
    }

    private async Task<AudioClip> LoadAudioClipInternalAsync(string clipName)
    {
        AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>(clipName);
        AudioClip clip = await handle.Task;

        if (handle.Status != AsyncOperationStatus.Succeeded || clip == null)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
            return null;
        }

        _sceneHandles[clipName] = handle;
        _sceneClips[clipName] = clip;
        return clip;
    }

    private void ReleaseCurrentSceneAudioClips()
    {
        foreach (AsyncOperationHandle<AudioClip> handle in _sceneHandles.Values)
        {
            if (!handle.IsValid()) continue;
            Addressables.Release(handle);
        }
        _sceneHandles.Clear();
        _sceneClips.Clear();
    }

    private void ReleaseCommonAudioClips()
    {
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
    [SerializeField] private EAudioType _testEAudioType = EAudioType.Music;
    [SerializeField] [Range(0f, 1f)] private float _testVolume = 0.5f;
    [SerializeField] private string _testBGMName = "MainTheme";
    [SerializeField] private string _testSfxName = "Click";

    [ContextMenu("Test/Set Volume")]
    private void TestSetVolume()
    {
        SetAudioVolume(_testEAudioType, _testVolume);
        Debug.Log($"[SoundManager] {_testEAudioType} 볼륨 설정: {_testVolume}");
    }

    [ContextMenu("Test/Get Volume")]
    private void TestGetVolume()
    {
        float volume = GetVolume(_testEAudioType);
        Debug.Log($"[SoundManager] {_testEAudioType} 현재 볼륨: {volume}");
    }

    [ContextMenu("Test/Play BGM")]
    private void TestPlayBGM()
    {
        PlayBGM(_testBGMName);
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