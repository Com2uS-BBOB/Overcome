using System;
using UnityEngine;

public class UI_LightningVignette : MonoBehaviour
{
    [Serializable]
    public class EdgeParticleSettings
    {
        public ParticleSystem prefab;
        public Vector2 anchor = new Vector2(0.5f, 1f);
        public float zRotation;
    }

    [SerializeField] private Canvas _canvas;
    [SerializeField] private EdgeParticleSettings[] _edgeParticles;

    private ParticleSystem[] _spawnedParticles;

    private void Start()
    {
        SpawnAndPositionParticles();
    }

    private void SpawnAndPositionParticles()
    {
        RectTransform canvasRect = _canvas.GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.rect.size;

        _spawnedParticles = new ParticleSystem[_edgeParticles.Length];

        for (var i = 0; i < _edgeParticles.Length; i++)
        {
            EdgeParticleSettings settings = _edgeParticles[i];

            GameObject spawned = Instantiate(settings.prefab.gameObject, transform);
            _spawnedParticles[i] = spawned.GetComponent<ParticleSystem>();

            // anchor (0~1) → localPosition 변환
            Vector3 localPos = new Vector3(
                (settings.anchor.x - 0.5f) * canvasSize.x,
                (settings.anchor.y - 0.5f) * canvasSize.y,
                0
            );

            spawned.transform.localPosition = localPos;
            spawned.transform.localRotation = Quaternion.Euler(0, 0, settings.zRotation);
        }
    }

    public void SetActive(bool active)
    {
        if (_spawnedParticles == null) return;

        foreach (ParticleSystem particle in _spawnedParticles)
        {
            if (particle == null) continue;
            particle.gameObject.SetActive(active);
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
        if (_spawnedParticles == null) return;

        foreach (ParticleSystem particle in _spawnedParticles)
        {
            if (particle == null) continue;
            particle.Stop();
        }
    }

    private void OnEnable()
    {
        Play();
    }

    private void OnDisable()
    {
        Stop();
    }
}