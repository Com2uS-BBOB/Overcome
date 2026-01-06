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
        OnSceneChanged?.Invoke(sceneType);
    }

    public void LoadScene(ESceneType nextScene)
    {
        if (!_sceneNameMap.TryGetValue(nextScene, out string sceneName)) return;

        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneAsync(ESceneType nextScene)
    {
        if (_isLoading) return;
        if (!_sceneNameMap.TryGetValue(nextScene, out string sceneName)) return;

        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    public void ReloadCurrentScene()
    {
        LoadSceneAsync(_currentScene);
    }

    private IEnumerator LoadSceneCoroutine(string nextScene)
    {
        _isLoading = true;
        // todo. Loading UI(또는 Scene) 표시

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            // todo. Loading Ui Update
            if (progress >= 1.0f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        // todo. Loading UI(또는 Scene) 제거
        _isLoading = false;
    }
}