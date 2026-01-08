using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_KillLog : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private KillLogItem _killLogItemPrefab;
    [SerializeField] private Transform _killLogContainer;
    [SerializeField] private int _maxActiveItems = 5;
    [SerializeField] private float _itemLifetime = 3f;

    [Header("Pool Settings")]
    [SerializeField] private int _initialPoolSize = 10;
    private readonly Queue<KillLogItem> _itemPool = new Queue<KillLogItem>();
    private readonly List<KillLogItem> _activeItems = new List<KillLogItem>();
    
    // TestCode
    [SerializeField] private Sprite[] _skillIcon;
    [SerializeField] private Sprite[] _enemyIcon;

    private void Start()
    {
        InitializePool();
        if (KillLogSystem.Instance != null)
        {
            KillLogSystem.Instance.OnKillLogged += OnKillLogged;
        }
    }

    private void OnDestroy()
    {
        
        if (KillLogSystem.Instance != null)
        {
            KillLogSystem.Instance.OnKillLogged -= OnKillLogged;
        }
    }

    private void InitializePool()
    {
        for (int i = 0; i < _initialPoolSize; i++)
        {
            CreateNewItem();
        }
    }

    private KillLogItem CreateNewItem()
    {
        KillLogItem item = Instantiate(_killLogItemPrefab, _killLogContainer);
        item.gameObject.SetActive(false);
        item.OnExpired += ReturnToPool;
        _itemPool.Enqueue(item);
        return item;
    }

    private void OnKillLogged(KillLogConfig config)
    {
        if (_activeItems.Count >= _maxActiveItems)
        {
            KillLogItem oldestItem = _activeItems[0];
            ReturnToPool(oldestItem);
        }
        KillLogItem item = GetItemFromPool();

        // todo. 실제 각각의 정보에 맞는 Icon 정보 필요
        Sprite skillIcon = _skillIcon[UnityEngine.Random.Range(0, _skillIcon.Length)];
        Sprite enemyIcon = _enemyIcon[UnityEngine.Random.Range(0, _enemyIcon.Length)];
        
        item.Initialize(skillIcon, enemyIcon, _itemLifetime);
        _activeItems.Add(item);
        item.transform.SetAsFirstSibling();
    }

    private KillLogItem GetItemFromPool()
    {
        if (_itemPool.Count == 0)
        {
            return CreateNewItem();
        }

        return _itemPool.Dequeue();
    }

    private void ReturnToPool(KillLogItem item)
    {
        if (_activeItems.Contains(item))
        {
            _activeItems.Remove(item);
        }

        item.gameObject.SetActive(false);
        _itemPool.Enqueue(item);
    }
}
