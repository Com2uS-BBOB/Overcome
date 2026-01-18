using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class UIController : SingletonBehaviour<UIController>
{
    [SerializeField] private GameObject _canvasPrefab;
    private Transform _uiRoot;
    
    // 지금 Hierarchy 창에 있는 UI 목록
    private readonly Dictionary<Type, BaseUI> _uiInstances = new Dictionary<Type, BaseUI>();
    
    // 현재 활성화된 UI 목록
    private readonly Dictionary<Type, BaseUI> _activeUI = new Dictionary<Type, BaseUI>();
    private readonly List<BaseUI> _activeUIList = new List<BaseUI>();
    
    // Addressables
    private readonly Dictionary<Type, AsyncOperationHandle<GameObject>> _loadedHandles = new Dictionary<Type, AsyncOperationHandle<GameObject>>();
    private readonly HashSet<Type> _loadingUIs = new HashSet<Type>();

    #region Unity Lifecycle Functions
    protected override void Init()
    {
        if (_canvasPrefab == null)
        {
            Debug.LogError($"[UIController] Missing canvas prefab");
        }
        GameObject uiRoot = Instantiate(_canvasPrefab);
        _uiRoot = uiRoot.transform;
        DontDestroyOnLoad(_uiRoot);

        SceneManager.sceneUnloaded += ClearSceneUI;
    }
    
    protected override void Clear()
    {
        ReleaseAll();
    }
    #endregion
    
    #region UI Functions (UI Open, Close)
    public async Task<T> OpenUI<T>() where T : BaseUI
    {
        Type type = typeof(T);
        if (_loadingUIs.Contains(type)) return null;

        if (_activeUI.TryGetValue(type, out BaseUI ui))
        {
            ui.OnOpen();
            return ui as T;
        }

        T newUI = await LoadUIAsync<T>();
        if (newUI == null)
        {
            Debug.LogError($"[UIController] Failed to load {typeof(T)}");
            return null;
        }
        ShowUI(newUI);
        return newUI;
    }
    
    private void ShowUI(BaseUI ui)
    {
        _activeUI.Add(ui.GetType(), ui);
        _activeUIList.Add(ui);
        ui.OnOpen();
    }

    public void CloseUI<T>() where T : BaseUI
    {
        if (!_activeUI.TryGetValue(typeof(T), out BaseUI ui)) return;
        ui.OnClose();
        _activeUI.Remove(typeof(T));
        _activeUIList.Remove(ui);
    }

    public void CloseUI(BaseUI ui)
    {
        if (ui == null) return;
        Type type = ui.GetType();
        if (!_activeUI.ContainsKey(type)) return;

        ui.OnClose();
        _activeUI.Remove(type);
        _activeUIList.Remove(ui);
    }

    public bool CloseLastUI()
    {
        if (_activeUIList.Count == 0) return false;

        BaseUI lastOpenedUI = _activeUIList[^1];
        _activeUI.Remove(lastOpenedUI.GetType());
        _activeUIList.Remove(lastOpenedUI);

        lastOpenedUI.OnClose();
        return true;
    }
    
    private void ClearSceneUI(Scene arg0)
    {
        while (_activeUIList.Count > 0)
        {
            CloseLastUI();
        }
    }
    #endregion
    
    #region Load UI Resource
    private async Task<T> LoadUIAsync<T>() where T : BaseUI
    {
        Type type = typeof(T);
        string addressID = type.Name;

        // 이미 Loading 중인지 여부는 Open에서 처리
        _loadingUIs.Add(type);

        try
        {
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(addressID);
            await handle.Task;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
            {
                Addressables.Release(handle);
                return null;
            }

            GameObject uiInstance = Instantiate(handle.Result, _uiRoot);
            uiInstance.SetActive(false);

            T uiComponent = uiInstance.GetComponent<T>();
            if (uiComponent == null)
            {
                Destroy(uiInstance);
                Addressables.Release(handle);
                return null;
            }

            _loadedHandles[type] = handle;
            _uiInstances[type] = uiComponent;

            return uiComponent;
        }
        catch (Exception e)
        {
            Debug.LogError($"[UIController] Exception loading {type.Name}: {e}"); 
            return null;
        }
        finally
        {
            _loadingUIs.Remove(type);
        }
    }

    private void ReleaseUI<T>() where T : BaseUI
    {
        Type type = typeof(T);
        if (IsUIOpened<T>())
        {
            CloseUI<T>();
        }

        if (_uiInstances.TryGetValue(type, out BaseUI ui))
        {
            if (ui != null)
            {
                Destroy(ui.gameObject);
            }
            _uiInstances.Remove(type);
        }
        
        if (_loadedHandles.TryGetValue(type, out AsyncOperationHandle<GameObject> handle))
        {
            Addressables.Release(handle);
            _loadedHandles.Remove(type);
        }
    }

    private void ReleaseAll()
    {
        foreach (AsyncOperationHandle<GameObject> handle in _loadedHandles.Values)
        {
            Addressables.Release(handle);
        }
        _loadedHandles.Clear();
        _uiInstances.Clear();
        _activeUI.Clear();
        _activeUIList.Clear();
    }
    #endregion
    
    #region Query
    public bool IsUIOpened<T>() where T : BaseUI => _activeUI.ContainsKey(typeof(T));
    #endregion
}