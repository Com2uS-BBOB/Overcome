using System;
using System.Collections.Generic;
using UnityEngine;

public class UI_Minimap : MonoBehaviour
{
    [Space(10)]
    [Header("UI References")]
    [SerializeField] private RectTransform _playerIcon;
    [SerializeField] private GameObject _enemyIconFolder;
    [SerializeField] private RectTransform _minimapRect;

    [Header("Extra References")]
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _player;
    [SerializeField] private GameObject _enemyIconPrefab;
    private Vector3 _playerPosition;
    
    [Space(10)]
    [Header("Minimap Settings")]
    [SerializeField] private float _presentArea = 30f;
    [SerializeField] private float _updateInterval = 0.05f;

    [Space(10)]
    [Header("Pool")]
    [SerializeField] private int _initialPoolSize = 10;

    [Space(10)]
    [Header("Visual")]
    [SerializeField] private Color _inRangeColor = Color.red;
    [SerializeField] private Color _outOfRangeColor = new Color(1f, 0.5f, 0f);

    private readonly Dictionary<Transform, EnemyMinimapIcon> _enemyIcons = new Dictionary<Transform, EnemyMinimapIcon>();
    private readonly List<Transform> _enemiesToRemove = new List<Transform>();
    private readonly Stack<EnemyMinimapIcon> _iconPool = new Stack<EnemyMinimapIcon>();
    private float _lastUpdateTime;
    private float _minimapRadius;
    private float _scale;

    private void Init()
    {
        if (_player == null)
        {
            _player = GameObject.FindGameObjectWithTag("Player").transform;
            if (_player == null)
            {
                enabled = false;
                return;
            }
        }
        
        _minimapRadius = Mathf.Min(_minimapRect.rect.width, _minimapRect.rect.height) / 2f;
        _scale = _minimapRadius / _presentArea;

        if (_camera == null)
        {
            _camera = Camera.main;
        }
        
        InitializePool();
    }

    private void Awake()
    {
        Init();
        EnemyEventController.Enemy.OnSpawned += RegisterEnemy;
        EnemyEventController.Enemy.OnKilled += UnregisterEnemy;
    }

    private void OnDestroy()
    {
        EnemyEventController.Enemy.OnSpawned -= RegisterEnemy;
        EnemyEventController.Enemy.OnKilled -= UnregisterEnemy;
    }

    private void LateUpdate()
    {
        if (Time.time - _lastUpdateTime < _updateInterval) return;

        UpdateMinimap();
        _lastUpdateTime = Time.time;
    }
    
    #region Pool
    private void InitializePool()
    {
        for (var i = 0; i < _initialPoolSize; i++)
        {
            EnemyMinimapIcon minimapIcon = CreateIcon();
            minimapIcon.gameObject.SetActive(false);
            _iconPool.Push(minimapIcon);
        }
    }

    private EnemyMinimapIcon CreateIcon()
    {
        GameObject iconObj = Instantiate(_enemyIconPrefab, _enemyIconFolder.transform);
        EnemyMinimapIcon minimapIcon = iconObj.GetComponent<EnemyMinimapIcon>();
        minimapIcon.Initialize(_inRangeColor, _outOfRangeColor, _minimapRadius);
        return minimapIcon;
    }

    private EnemyMinimapIcon GetIcon()
    {
        if (_iconPool.Count > 0)
        {
            EnemyMinimapIcon minimapIcon = _iconPool.Pop();
            minimapIcon.gameObject.SetActive(true);
            return minimapIcon;
        }

        return CreateIcon();
    }

    private void ReleaseIcon(EnemyMinimapIcon minimapIcon)
    {
        minimapIcon.gameObject.SetActive(false);
        _iconPool.Push(minimapIcon);
    }
    #endregion
    
    private void RegisterEnemy(EnemySpawnedEvent enemy)
    {
        Transform enemyTransform = enemy.Enemy.transform;
        if (_enemyIcons.ContainsKey(enemyTransform)) return;

        EnemyMinimapIcon minimapIcon = GetIcon();
        _enemyIcons[enemyTransform] = minimapIcon;
    }

    private void UnregisterEnemy(EnemyKilledEvent enemy)
    {
        Transform enemyTransform = enemy.Enemy.transform;
        RemoveEnemy(enemyTransform);
    }

    private void RemoveEnemy(Transform enemy)
    {
        if (!_enemyIcons.TryGetValue(enemy, out EnemyMinimapIcon icon)) return;

        ReleaseIcon(icon);
        _enemyIcons.Remove(enemy);
    }

    private void UpdateMinimap()
    {
        _playerPosition = _player.position;

        UpdatePlayerIcon();
        UpdateEnemyIcons();
        RemoveDestroyedEnemies();
    }

    private void UpdatePlayerIcon()
    {
        _playerIcon.localRotation = Quaternion.Euler(0, 0, -_camera.transform.eulerAngles.y);
    }

    private void UpdateEnemyIcons()
    {
        foreach ((Transform enemy, EnemyMinimapIcon icon) in _enemyIcons)
        {
            if (enemy == null)
            {
                _enemiesToRemove.Add(enemy);
                continue;
            }

            Vector3 relativePos = enemy.position - _playerPosition;
            Vector2 mapPosition = new Vector2(relativePos.x, relativePos.z) * _scale;

            icon.UpdatePosition(mapPosition);
        }
    }

    private void RemoveDestroyedEnemies()
    {
        foreach (Transform enemy in _enemiesToRemove)
        {
            RemoveEnemy(enemy);
        }

        _enemiesToRemove.Clear();
    }
}
