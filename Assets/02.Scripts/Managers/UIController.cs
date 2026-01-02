using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : SingletonBehaviour<UIController>
{
    private readonly Dictionary<string, BaseUI> _uiObjects = new Dictionary<string, BaseUI>();
    private readonly Dictionary<string, BaseUI> _openUIObjects = new Dictionary<string, BaseUI>();
    private Stack<BaseUI> _activUIObjects;

    private int _sortLayerIndex = -20;
    private int _pauseGameCount;
    private int _showCursorCount;

    protected override void Init()
    {
        _activUIObjects = new Stack<BaseUI>();
    }

    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }

    private void OnSceneChanged(Scene previousScene, Scene currentScene)
    {
        ClearAllUI();
    }

    public void RegisterUI(string key, BaseUI baseUI)
    {
        _uiObjects.TryAdd(key, baseUI);
    }

    public void UnregisterUI(string key)
    {
        if (_openUIObjects.ContainsKey(key))
        {
            CloseUI(key);
        }

        _uiObjects.Remove(key);
    }

    private void ClearAllUI()
    {
        CloseAllUI();

        _uiObjects.Clear();
        _sortLayerIndex = -20;
        _pauseGameCount = 0;
        _showCursorCount = 0;
    }

    public void TryOpenUI(string key)
    {
        if (_openUIObjects.ContainsKey(key)) return;
        if (!_uiObjects.TryGetValue(key, out BaseUI ui)) return;

        _openUIObjects.Add(key, ui);
        _activUIObjects.Push(ui);

        ui.SetSortingOrder(_sortLayerIndex++);

        SetupOpenCallback(ui);

        ui.OnOpen();
    }

    private void SetupOpenCallback(BaseUI baseUI)
    {
        baseUI.UIEventHandler = new BaseUIEvent
        {
            OnOpenComplete = () =>
            {
                OpenUIEvent(baseUI.Config);
                baseUI.UIEventHandler = null;
            }
        };
    }

    private void OpenUIEvent(UIConfig config)
    {
        if (config.PauseGame)
        {
            _pauseGameCount++;
            if (_pauseGameCount == 1)
            {
                Time.timeScale = 0f;
            }
        }

        if (config.ShowCursor)
        {
            _showCursorCount++;
            if (_showCursorCount == 1)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    public void CloseUI(string key)
    {
        if (!_openUIObjects.TryGetValue(key, out BaseUI ui)) return;

        SetupCloseCallback(ui, key);

        ui.OnClose();
    }

    private void SetupCloseCallback(BaseUI baseUI, string key)
    {
        baseUI.UIEventHandler = new BaseUIEvent
        {
            OnCloseComplete = () =>
            {
                CloseUIEvent(baseUI.Config);
                RemoveUIFromStack(baseUI);
                _openUIObjects.Remove(key);
                baseUI.UIEventHandler = null;
            }
        };
    }

    private void RemoveUIFromStack(BaseUI baseUI)
    {
        if (_activUIObjects.Count > 0 && _activUIObjects.Peek() == baseUI)
        {
            _activUIObjects.Pop();
        }
    }

    private void CloseUIEvent(UIConfig config)
    {
        if (config.PauseGame)
        {
            _pauseGameCount--;
            if (_pauseGameCount <= 0)
            {
                _pauseGameCount = 0;
                Time.timeScale = 1f;
            }
        }

        if (config.ShowCursor)
        {
            _showCursorCount--;
            if (_showCursorCount <= 0)
            {
                _showCursorCount = 0;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    public void CloseAllUI()
    {
        var keys = new List<string>(_openUIObjects.Keys);

        foreach (string key in keys)
        {
            CloseUI(key);
        }
    }

    public T GetUI<T>(string key) where T : BaseUI
    {
        return _uiObjects.TryGetValue(key, out BaseUI ui) ? ui as T : null;
    }
}
