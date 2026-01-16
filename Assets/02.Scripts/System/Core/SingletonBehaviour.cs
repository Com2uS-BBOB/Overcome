using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    protected virtual bool DontDestroy => true;

    private static T _instance;

    public static T Instance => _instance;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this as T;
        
        if (DontDestroy)
        {
            DontDestroyOnLoad(gameObject);
        }
    
        Init();
    }

    protected virtual void Init() { }
    protected virtual void Clear() { }

    private void OnDestroy()
    {
        Clear();
        if (_instance != this) return;
        _instance = null;
    }
}