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

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}