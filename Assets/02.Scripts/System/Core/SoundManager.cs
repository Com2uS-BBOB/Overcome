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

    [Header("Scene BGM Settings")]
    [SerializeField] private SceneBGMConfig _sceneBGMConfig;
    [SerializeField] private bool _autoPlaySceneBGM = true;
    [Tooltip("LoadingScene에서는 BGM을 변경하지 않음")]
    [SerializeField] private bool _skipLoadingScene = true;

    private AudioSource _bgmSource;
    private Coroutine _fadeCoroutine;
    private Coroutine _introLoopCoroutine;
    private float _currentBGMVolume = 1f;
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
        ReleaseCurrentSceneAudioClips();

        // 씬별 자동 BGM 재생
        if (_autoPlaySceneBGM && _sceneBGMConfig != null)
        {
            if (Enum.TryParse(scene.name, out ESceneType sceneType))
            {
                // LoadingScene은 건너뛰기 (기존 BGM 유지)
                if (_skipLoadingScene && sceneType == ESceneType.LoadingScene)
                {
                    return;
                }

                PlaySceneBGM(sceneType);
            }
        }
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
        // PlayerDataManager에서 현재 플레이어의 설정 로드 시도
        if (PlayerDataManager.Instance != null)
        {
            PlayerSettings settings = PlayerDataManager.Instance.GetSettings();
            if (settings != null && (settings.MasterVolume > 0 || settings.MusicVolume > 0 || settings.SfxVolume > 0))
            {
                LoadPlayerVolumeSettings(settings);
                return;
            }
        }
        SetDefaultAudioVolume();
    }

    private void SetDefaultAudioVolume()
    {
        // 저장된 설정이 없으면 기본값 사용
        SetAudioVolume(EAudioType.Master, 0.5f);
        SetAudioVolume(EAudioType.Music, 0.5f);
        SetAudioVolume(EAudioType.Effect, 0.5f);
    }
    
    public void LoadPlayerVolumeSettings(PlayerSettings settings)
    {
        if (settings == null)
        {
            SetDefaultAudioVolume();
            return;
        }

        float master = settings.MasterVolume > 0 ? settings.MasterVolume : 0.5f;
        float music = settings.MusicVolume > 0 ? settings.MusicVolume : 0.5f;
        float sfx = settings.SfxVolume > 0 ? settings.SfxVolume : 0.5f;

        SetAudioVolume(EAudioType.Master, master);
        SetAudioVolume(EAudioType.Music, music);
        SetAudioVolume(EAudioType.Effect, sfx);
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
            playerDataManager.SaveData();
        }
    }
    #endregion

    #region BGM Control
    public void StopBGM()
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        if (_introLoopCoroutine != null) StopCoroutine(_introLoopCoroutine);
        _bgmSource.Stop();
        _bgmSource.clip = null;
        _bgmSource.loop = true;
        _currentBGMVolume = 1f;
    }

    /// <summary>
    /// 페이드 아웃 후 BGM 정지
    /// </summary>
    public void StopBGMWithFade(float fadeOutDuration = 0.5f)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeOutAndStopCoroutine(fadeOutDuration));
    }

    private IEnumerator FadeOutAndStopCoroutine(float duration)
    {
        yield return FadeOutBGMCoroutine(duration);
        _bgmSource.Stop();
        _bgmSource.clip = null;
        _bgmSource.loop = true;
    }

    public void PauseBGM() => _bgmSource.Pause();
    public void ResumeBGM() => _bgmSource.UnPause();
    #endregion

    #region Scene BGM Control
    /// <summary>
    /// 씬 타입에 맞는 BGM을 자동 재생 (SceneBGMConfig 참조)
    /// </summary>
    public void PlaySceneBGM(ESceneType sceneType)
    {
        if (_sceneBGMConfig == null)
        {
            Debug.LogWarning("[SoundManager] SceneBGMConfig가 할당되지 않았습니다.");
            return;
        }

        SceneBGMData bgmData = _sceneBGMConfig.GetBGMData(sceneType);
        if (bgmData == null)
        {
            return;
        }

        if (!bgmData.PlayOnSceneLoad)
        {
            return;
        }

        CrossfadeToBGM(bgmData);
    }

    /// <summary>
    /// 크로스페이드로 새 BGM으로 전환
    /// </summary>
    public void CrossfadeToBGM(SceneBGMData bgmData)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        if (_introLoopCoroutine != null) StopCoroutine(_introLoopCoroutine);
        _fadeCoroutine = StartCoroutine(CrossfadeCoroutine(bgmData));
    }

    /// <summary>
    /// 기존 BGM에서 새 BGM으로 크로스페이드
    /// </summary>
    public void CrossfadeToBGM(string newBGMName, float fadeOutDuration = 0.5f, float fadeInDuration = 1f)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        if (_introLoopCoroutine != null) StopCoroutine(_introLoopCoroutine);
        _fadeCoroutine = StartCoroutine(CrossfadeToSingleBGMCoroutine(newBGMName, fadeOutDuration, fadeInDuration));
    }

    private IEnumerator CrossfadeCoroutine(SceneBGMData bgmData)
    {
        // 1. 기존 BGM 페이드 아웃
        if (_bgmSource.isPlaying && bgmData.FadeOutDuration > 0f)
        {
            yield return FadeOutBGMCoroutine(bgmData.FadeOutDuration);
        }

        _bgmSource.Stop();

        // 2. 새 BGM 로드 및 재생
        if (bgmData.UseIntroLoop)
        {
            yield return PlayBGMWithIntroAndFadeCoroutine(bgmData.IntroBGMName, bgmData.LoopBGMName, bgmData.FadeInDuration, bgmData.VolumeMultiplier);
        }
        else
        {
            yield return PlayBGMWithFadeCoroutine(bgmData.BGMName, bgmData.FadeInDuration, bgmData.VolumeMultiplier);
        }
    }

    private IEnumerator CrossfadeToSingleBGMCoroutine(string bgmName, float fadeOutDuration, float fadeInDuration)
    {
        // 1. 기존 BGM 페이드 아웃
        if (_bgmSource.isPlaying && fadeOutDuration > 0f)
        {
            yield return FadeOutBGMCoroutine(fadeOutDuration);
        }

        _bgmSource.Stop();

        // 2. 새 BGM 로드 및 페이드 인
        yield return PlayBGMWithFadeCoroutine(bgmName, fadeInDuration, 1f);
    }

    private IEnumerator PlayBGMWithFadeCoroutine(string clipName, float fadeInDuration, float targetVolumeMultiplier)
    {
        // Task를 코루틴에서 대기
        var loadTask = LoadAudioClipAsync(clipName);
        while (!loadTask.IsCompleted)
        {
            yield return null;
        }

        AudioClip loadedClip = loadTask.Result;
        if (loadedClip == null)
        {
            Debug.LogError($"[SoundManager] BGM 클립 로드 실패: {clipName}");
            yield break;
        }

        _bgmSource.clip = loadedClip;
        _bgmSource.loop = true;
        _bgmSource.volume = 0f;
        _bgmSource.Play();

        // 페이드 인
        yield return FadeInBGMCoroutine(fadeInDuration, targetVolumeMultiplier);
    }

    private IEnumerator PlayBGMWithIntroAndFadeCoroutine(string introClipName, string loopClipName, float fadeInDuration, float targetVolumeMultiplier)
    {
        // 인트로 로드
        var introTask = LoadAudioClipAsync(introClipName);
        while (!introTask.IsCompleted) yield return null;
        AudioClip introClip = introTask.Result;

        // 루프 로드
        var loopTask = LoadAudioClipAsync(loopClipName);
        while (!loopTask.IsCompleted) yield return null;
        AudioClip loopClip = loopTask.Result;

        if (introClip == null || loopClip == null)
        {
            Debug.LogError($"[SoundManager] 인트로/루프 BGM 로드 실패: {introClipName}, {loopClipName}");
            yield break;
        }

        _bgmSource.clip = introClip;
        _bgmSource.loop = false;
        _bgmSource.volume = 0f;
        _bgmSource.Play();

        // 페이드 인
        yield return FadeInBGMCoroutine(fadeInDuration, targetVolumeMultiplier);

        // 인트로 끝나면 루프로 전환
        float remainingTime = introClip.length - fadeInDuration;
        if (remainingTime > 0)
        {
            yield return new WaitForSeconds(remainingTime);
        }

        if (_bgmSource != null && loopClip != null)
        {
            Debug.Log($"[SoundManager] 루프 BGM 전환: {loopClip.name}");
            _bgmSource.clip = loopClip;
            _bgmSource.loop = true;
            _bgmSource.Play();
        }
    }

    private IEnumerator FadeOutBGMCoroutine(float duration)
    {
        float startVolume = _bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _bgmSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        _bgmSource.volume = 0f;
    }

    private IEnumerator FadeInBGMCoroutine(float duration, float targetVolumeMultiplier = 1f)
    {
        float targetVolume = _currentBGMVolume * targetVolumeMultiplier;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _bgmSource.volume = Mathf.Lerp(0f, targetVolume, elapsed / duration);
            yield return null;
        }

        _bgmSource.volume = targetVolume;
    }

    /// <summary>
    /// BGM 볼륨 페이드 (현재 재생 중인 BGM의 볼륨만 조절)
    /// </summary>
    public void FadeBGMVolume(float targetVolume, float duration)
    {
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeBGMVolumeCoroutine(targetVolume, duration));
    }

    private IEnumerator FadeBGMVolumeCoroutine(float targetVolume, float duration)
    {
        float startVolume = _bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _bgmSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        _bgmSource.volume = targetVolume;
        _currentBGMVolume = targetVolume;
    }
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
        CrossfadeToBGM(_testBGMName, 0.5f, 1f);
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