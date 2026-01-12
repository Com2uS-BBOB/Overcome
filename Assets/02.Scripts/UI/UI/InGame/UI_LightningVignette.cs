using System;
using _02.Scripts.Player.Gauge;
using UnityEngine;

public class UI_LightningVignette : MonoBehaviour
{
    [Serializable]
    public class EdgeParticleSettings
    {
        public ParticleSystem Prefab;
        public Vector2 Anchor = new Vector2(0.5f, 1f);
        public float ZRotation;
    }

    [SerializeField] private GaugeManager _gaugeManager;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private EdgeParticleSettings[] _edgeParticles;

    private ParticleSystem[] _spawnedParticles;

    private void Awake()
    {
        SpawnAndPositionParticles();
    }

    private void Start()
    {
        SubscribeOverDriveEvents();
        Stop();
    }

    private void OnDestroy()
    {
        if (_gaugeManager == null) return;
        UnsubscribeOverDriveEvents();
    }
    
    private void OnEnable()
    {
        Play();
    }

    private void OnDisable()
    {
        Stop();
    }
    
    private void SubscribeOverDriveEvents()
    {
        _gaugeManager.OnOverDriveActivated += ActiveUI;
        _gaugeManager.OnOverDriveDeactivated += Stop;
    }

    private void UnsubscribeOverDriveEvents()
    {
        _gaugeManager.OnOverDriveActivated -= ActiveUI;
        _gaugeManager.OnOverDriveDeactivated -= Stop;
    }
    
    private void SpawnAndPositionParticles()
    {
        RectTransform canvasRect = _canvas.GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.rect.size;

        _spawnedParticles = new ParticleSystem[_edgeParticles.Length];

        for (var i = 0; i < _edgeParticles.Length; i++)
        {
            EdgeParticleSettings settings = _edgeParticles[i];

            GameObject spawned = Instantiate(settings.Prefab.gameObject, transform);
            _spawnedParticles[i] = spawned.GetComponent<ParticleSystem>();

            var renderer = spawned.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.sortingLayerName = _canvas.sortingLayerName;
                renderer.sortingOrder = _canvas.sortingOrder + 1;
            }

            Vector3 localPos = new Vector3(
                (settings.Anchor.x - 0.5f) * canvasSize.x,
                (settings.Anchor.y - 0.5f) * canvasSize.y,
                0
            );

            spawned.transform.localPosition = localPos;
            spawned.transform.localRotation = Quaternion.Euler(0, 0, settings.ZRotation);
        }
    }

    public void ActiveUI()
    {
        if (_spawnedParticles == null) return;
        gameObject.SetActive(true);
        foreach (ParticleSystem particle in _spawnedParticles)
        {
            if (particle == null) continue;
            particle.gameObject.SetActive(true);
        }
    }

    public void Play()
    {
        if (_spawnedParticles == null) return;

        foreach (ParticleSystem particle in _spawnedParticles)
        {
            if (particle == null) continue;
            particle.gameObject.SetActive(true);
            particle.Play();
        }
    }

    public void Stop()
    {
        gameObject.SetActive(false);
        if (_spawnedParticles == null) return;

        foreach (ParticleSystem particle in _spawnedParticles)
        {
            if (particle == null) continue;
            particle.Stop();
        }
    }
}