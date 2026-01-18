using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _02.Scripts.Player.Overdrive
{
    /// <summary>
    /// Overdrive 중 플레이어 잔상 효과
    /// 스텔라 블레이드 타키모드 스타일
    /// </summary>
    public class AfterimageController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _spawnInterval = 0.05f;
        [SerializeField] private float _fadeOutDuration = 0.3f;
        [SerializeField] private int _poolSize = 20;

        [Header("Afterimage Material")]
        [SerializeField] private Material _afterimageMaterial;

        [Header("Colors")]
        [ColorUsage(true, true)]
        [SerializeField] private Color _afterimageColor = new Color(0f, 2f, 3f, 0.6f);

        [Header("Player Reference")]
        [SerializeField] private SkinnedMeshRenderer _playerMeshRenderer;

        private bool _isActive;
        private Coroutine _spawnCoroutine;
        private Queue<AfterimageInstance> _pool = new Queue<AfterimageInstance>();
        private List<AfterimageInstance> _activeInstances = new List<AfterimageInstance>();

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        private void Awake()
        {
            InitializePool();
        }

        private void InitializePool()
        {
            if (_afterimageMaterial == null || _playerMeshRenderer == null) return;

            for (int i = 0; i < _poolSize; i++)
            {
                var instance = CreateAfterimageInstance();
                instance.gameObject.SetActive(false);
                _pool.Enqueue(instance);
            }
        }

        private AfterimageInstance CreateAfterimageInstance()
        {
            var go = new GameObject("Afterimage");
            go.transform.SetParent(transform);

            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mr.material = new Material(_afterimageMaterial);
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;

            var instance = go.AddComponent<AfterimageInstance>();
            instance.Initialize(mf, mr, this);

            return instance;
        }

        public void StartAfterimage()
        {
            if (_isActive) return;
            if (_playerMeshRenderer == null || _afterimageMaterial == null) return;

            _isActive = true;
            _spawnCoroutine = StartCoroutine(SpawnAfterimages());
        }

        public void StopAfterimage()
        {
            _isActive = false;
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }
        }

        private IEnumerator SpawnAfterimages()
        {
            while (_isActive)
            {
                SpawnSingleAfterimage();
                yield return new WaitForSeconds(_spawnInterval);
            }
        }

        private void SpawnSingleAfterimage()
        {
            if (_playerMeshRenderer == null) return;

            // Mesh 스냅샷 생성
            Mesh bakedMesh = new Mesh();
            _playerMeshRenderer.BakeMesh(bakedMesh);

            // 풀에서 인스턴스 가져오기
            AfterimageInstance instance;
            if (_pool.Count > 0)
            {
                instance = _pool.Dequeue();
                instance.gameObject.SetActive(true);
            }
            else
            {
                // 풀이 비어있으면 가장 오래된 활성 인스턴스 재사용
                if (_activeInstances.Count > 0)
                {
                    instance = _activeInstances[0];
                    _activeInstances.RemoveAt(0);
                }
                else
                {
                    instance = CreateAfterimageInstance();
                }
            }

            _activeInstances.Add(instance);

            instance.Setup(
                bakedMesh,
                _playerMeshRenderer.transform.position,
                _playerMeshRenderer.transform.rotation,
                _playerMeshRenderer.transform.lossyScale,
                _afterimageColor,
                _fadeOutDuration
            );
        }

        public void ReturnToPool(AfterimageInstance instance)
        {
            instance.gameObject.SetActive(false);
            _activeInstances.Remove(instance);
            _pool.Enqueue(instance);
        }

        private void OnDestroy()
        {
            StopAfterimage();

            // 풀 정리
            while (_pool.Count > 0)
            {
                var instance = _pool.Dequeue();
                if (instance != null)
                    Destroy(instance.gameObject);
            }

            foreach (var instance in _activeInstances)
            {
                if (instance != null)
                    Destroy(instance.gameObject);
            }
            _activeInstances.Clear();
        }
    }

    /// <summary>
    /// 개별 잔상 인스턴스
    /// </summary>
    public class AfterimageInstance : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private AfterimageController _controller;
        private MaterialPropertyBlock _propertyBlock;
        private Coroutine _fadeCoroutine;

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        public void Initialize(MeshFilter mf, MeshRenderer mr, AfterimageController controller)
        {
            _meshFilter = mf;
            _meshRenderer = mr;
            _controller = controller;
            _propertyBlock = new MaterialPropertyBlock();
        }

        public void Setup(Mesh mesh, Vector3 position, Quaternion rotation, Vector3 scale, Color color, float fadeDuration)
        {
            _meshFilter.mesh = mesh;
            transform.position = position;
            transform.rotation = rotation;
            transform.localScale = scale;

            // 초기 색상 설정
            _meshRenderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetColor(BaseColor, color);
            _propertyBlock.SetColor(EmissionColor, color);
            _meshRenderer.SetPropertyBlock(_propertyBlock);

            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

            _fadeCoroutine = StartCoroutine(FadeOut(color, fadeDuration));
        }

        private IEnumerator FadeOut(Color startColor, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float alpha = Mathf.Lerp(startColor.a, 0f, t);

                Color currentColor = startColor;
                currentColor.a = alpha;

                // Emission도 함께 페이드
                Color emissionColor = startColor * (1f - t);

                _meshRenderer.GetPropertyBlock(_propertyBlock);
                _propertyBlock.SetColor(BaseColor, currentColor);
                _propertyBlock.SetColor(EmissionColor, emissionColor);
                _meshRenderer.SetPropertyBlock(_propertyBlock);

                elapsed += Time.deltaTime;
                yield return null;
            }

            _fadeCoroutine = null;
            _controller.ReturnToPool(this);
        }

        private void OnDisable()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }
        }
    }
}