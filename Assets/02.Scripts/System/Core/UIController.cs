using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : SingletonBehaviour<UIController>
{
    private readonly Dictionary<Type, BaseUI> _uiObjects = new Dictionary<Type, BaseUI>();
    private readonly List<BaseUI> _activeUIObjects = new List<BaseUI>();

    private int _pauseGameCount;
    private int _showCursorCount;

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

    public void RegisterUI<T>(T ui) where T : BaseUI
    {
        _uiObjects.TryAdd(typeof(T), ui);
    }

    public void UnregisterUI(BaseUI ui)
    {
        if (ui == null)
        {
            return;
        }

        Type type = ui.GetType();
        if (_activeUIObjects.Contains(ui))
        {
            CloseUI(ui);
        }
        _uiObjects.Remove(type);
    }

    private void ClearAllUI()
    {
        CloseAllUI();
        _uiObjects.Clear();
        _activeUIObjects.Clear();
        _pauseGameCount = 0;
        _showCursorCount = 0;
    }

    public void TryOpenUI<T>() where T : BaseUI
    {
        Type type = typeof(T);
        if (IsUIOpen<T>()) return;
        if (!_uiObjects.TryGetValue(type, out BaseUI ui)) return;

        _activeUIObjects.Add(ui);
        ui.BringToFront();

        var openEvent = new BaseUIEvent
        {
            OnOpenComplete = () => HandleOpenEvent(ui.Config)
        };
        ui.UIEventHandler = openEvent;
        ui.OnOpen();
    }

    public void CloseUI<T>() where T : BaseUI
    {
        Type type = typeof(T);
        BaseUI ui = _activeUIObjects.FirstOrDefault(u => u.GetType() == type);
        if (ui == null) return;
        CloseUI(ui);
    }

    private void CloseUI(BaseUI ui)
    {
        var closeEvent = new BaseUIEvent
        {
            OnCloseComplete = () =>
            {
                HandleCloseEvent(ui.Config);
                _activeUIObjects.Remove(ui);
            }
        };
        ui.UIEventHandler = closeEvent;
        ui.OnClose();
    }

    public void CloseAllUI()
    {
        var uiList = new List<BaseUI>(_activeUIObjects);

        foreach (var ui in uiList)
        {
            CloseUI(ui);
        }
    }

    private void HandleOpenEvent(UIConfig config)
    {
        if (config.PauseGame && ++_pauseGameCount == 1)
        {
            Time.timeScale = 0f;
        }
        if (config.ShowCursor && ++_showCursorCount == 1)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    private void HandleCloseEvent(UIConfig config)
    {
        if (config.PauseGame && --_pauseGameCount <= 0)
        {
            _pauseGameCount = 0;
            Time.timeScale = 1f;
        }
        if (config.ShowCursor && --_showCursorCount <= 0)
        {
            _showCursorCount = 0;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public T GetUI<T>() where T :  BaseUI => _uiObjects.TryGetValue(typeof(T), out BaseUI ui) ? ui as T : null;
    public int GetActiveUICount() => _activeUIObjects.Count;
    public BaseUI GetTopActiveUI() => _activeUIObjects.Count > 0 ? _activeUIObjects[^1] : null;
    public bool IsUIActive(BaseUI ui) => _activeUIObjects.Contains(ui);
    public bool IsUIOpen<T>() where T : BaseUI => _activeUIObjects.Any(ui => ui.GetType() == typeof(T));
}
