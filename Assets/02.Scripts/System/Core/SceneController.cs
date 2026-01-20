using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : SingletonBehaviour<SceneController>
{
    private ESceneType _currentScene = ESceneType.SampleScene;
    private Dictionary<ESceneType, string> _sceneNameMap;
    private bool _isLoading;
    public event Action<ESceneType> OnSceneChanged;
    public ESceneType CurrentScene => _currentScene;
    public bool IsLoading => _isLoading;
    private ESceneType _targetScene;
    public string TargetSceneName => _sceneNameMap.GetValueOrDefault(_targetScene);

    protected override void Init()
    {
        InitializeSceneMapping();
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    private void InitializeSceneMapping()
    {
        _sceneNameMap = new Dictionary<ESceneType, string>();
        HashSet<string> buildScenes = CollectBuildScenes();

        string currentSceneName = SceneManager.GetActiveScene().name;
        MapSceneTypesToScenes(buildScenes, currentSceneName);
    }

    private HashSet<string> CollectBuildScenes()
    {
        HashSet<string> buildScenes = new HashSet<string>();
        int sceneCount = SceneManager.sceneCountInBuildSettings;

        for (int i = 0; i < sceneCount; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            buildScenes.Add(sceneName);
        }

        return buildScenes;
    }

    private void MapSceneTypesToScenes(HashSet<string> buildScenes, string currentSceneName)
    {
        foreach (ESceneType sceneType in Enum.GetValues(typeof(ESceneType)))
        {
            string sceneName = sceneType.ToString();
            if (!buildScenes.Contains(sceneName)) continue;
            _sceneNameMap[sceneType] = sceneName;
            if (sceneName == currentSceneName)
            {
                _currentScene = sceneType;
            }
        }
    }

    private void OnActiveSceneChanged(Scene previousScene, Scene newScene)
    {
        if (!Enum.TryParse(newScene.name, out ESceneType sceneType))
        {
            Debug.LogWarning($"[SceneController] '{newScene.name}' is not defined in ESceneType enum.");
            return;
        }
        _currentScene = sceneType;

        if (_isLoading && sceneType == _targetScene)
        {
            _isLoading = false;
        }

        OnSceneChanged?.Invoke(sceneType);
    }

    private bool LoadScene(ESceneType nextScene)
    {
        if (!_sceneNameMap.TryGetValue(nextScene, out string sceneName)) return false;

        SceneManager.LoadScene(sceneName);
        return true;
    }

    public void LoadSceneAsync(ESceneType nextScene)
    {
        if (_isLoading) return;
        if (!_sceneNameMap.TryGetValue(nextScene, out string sceneName)) return;

        _isLoading = true;
        _targetScene = nextScene;
        if (LoadScene(ESceneType.LoadingScene)) return;
        _isLoading = false;
    }

    public void ReloadCurrentScene()
    {
        LoadSceneAsync(_currentScene);
    }
}