using UnityEngine;

namespace NeonHighCity.Environment
{
    /// <summary>
    /// BoxCollider 영역에 네온 그리드를 표시하는 컴포넌트.
    /// 플레이어가 갈 수 없는 구역을 시각적으로 표현한다.
    /// 
    /// [ExecuteAlways]: Edit 모드에서도 실행되어 실시간 미리보기 가능.
    /// 
    /// 메시 타입:
    /// - Cube: Holograms 에셋과 호환 (Fresnel 효과 작동)
    /// - Quad: 평면 전용 셰이더용 (NeonGridFlat)
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(BoxCollider))]
    public class NoGoZone : MonoBehaviour
    {
        #region Enums
        
        /// <summary>
        /// 그리드가 표시될 면 방향.
        /// </summary>
        public enum GridFace
        {
            [Tooltip("수평면 (바닥/천장) - X × Z 크기")]
            Top,
            
            [Tooltip("수직면 Z방향 (앞/뒤 벽) - X × Y 크기")]
            WallFrontBack,
            
            [Tooltip("수직면 X방향 (좌/우 벽) - Z × Y 크기")]
            WallLeftRight
        }
        
        /// <summary>
        /// 그리드 메시 타입.
        /// </summary>
        public enum MeshType
        {
            [Tooltip("얇은 박스 (Holograms 에셋 호환, Fresnel 작동)")]
            Cube,
            
            [Tooltip("단순 평면 (NeonGridFlat 셰이더용)")]
            Quad
        }
        
        #endregion

        #region Inspector Fields
        
        [Header("Mesh Settings")]
        [Tooltip("그리드 메시 타입\n" +
                 "• Cube: Holograms 에셋 호환 (Fresnel 효과 O)\n" +
                 "• Quad: 평면 전용 셰이더 (NeonGridFlat)")]
        [SerializeField] private MeshType _meshType = MeshType.Cube;
        
        [Tooltip("Cube 메시의 두께 (얇을수록 벽처럼 보임)")]
        [SerializeField] [Range(0.01f, 1f)] private float _cubeThickness = 0.1f;
        
        [Header("Grid Face")]
        [Tooltip("그리드가 표시될 면 방향\n" +
                 "• Top: 바닥/천장 (수평)\n" +
                 "• WallFrontBack: 앞뒤 벽 (Z축 방향)\n" +
                 "• WallLeftRight: 좌우 벽 (X축 방향)")]
        [SerializeField] private GridFace _gridFace = GridFace.WallLeftRight;
        
        [Tooltip("그리드 위치 오프셋 (면의 법선 방향)\n" +
                 "양수: 바깥쪽, 음수: 안쪽")]
        [SerializeField] private float _faceOffset = 0f;
        
        [Header("Grid Material")]
        [Tooltip("그리드 Material.\n" +
                 "• Cube용: Assets/Holograms/Materials/Examples/Basic/Grid_Hologram_Empty.mat\n" +
                 "• Quad용: Assets/Materials/M_NeonGridFlat.mat")]
        [SerializeField] private Material _gridMaterial;
        
        [Header("Grid Appearance (Holograms 에셋)")]
        [Tooltip("홀로그램 색상 (HDR 지원)")]
        [ColorUsage(true, true)]
        [SerializeField] private Color _hologramColor = new Color(0f, 1f, 1f, 1f);
        
        [Tooltip("발광 강도 (에셋 기본값: 20)")]
        [SerializeField] [Range(1f, 50f)] private float _emissionScale = 20f;
        
        [Tooltip("그리드 밀도 (에셋 기본값: 250, 낮을수록 넓은 간격)")]
        [SerializeField] [Range(50f, 500f)] private float _patternDensity = 250f;
        
        [Tooltip("Fresnel 강도 (가장자리 발광, 에셋 기본값: 1.5)")]
        [SerializeField] [Range(0.5f, 5f)] private float _fresnelPower = 1.5f;
        
        [Header("Animation (Holograms 에셋)")]
        [Tooltip("패턴 스크롤 속도 A (세로 방향, 에셋 기본값: 10)")]
        [SerializeField] [Range(0f, 30f)] private float _patternSpeedA = 10f;
        
        [Tooltip("패턴 스크롤 속도 B (가로 방향, 에셋 기본값: 0)")]
        [SerializeField] [Range(0f, 30f)] private float _patternSpeedB = 0f;
        
        #endregion

        #region Private Fields
        
        private BoxCollider _boxCollider;
        private GameObject _gridMesh;
        private MeshRenderer _meshRenderer;
        private Material _materialInstance;
        
        // 현재 생성된 메시 타입 추적 (변경 감지용)
        private MeshType _currentMeshType;
        
        // Shader Property IDs (Holograms 에셋)
        private static readonly int HologramColorProperty = Shader.PropertyToID("_Hologram_Color");
        private static readonly int EmissionScaleProperty = Shader.PropertyToID("_Emission_Scale");
        private static readonly int PatternDensityProperty = Shader.PropertyToID("_Pattern_Density");
        private static readonly int FresnelPowerProperty = Shader.PropertyToID("_Fresnel_Power");
        private static readonly int PatternSpeedAProperty = Shader.PropertyToID("_Pattern_Speed_A");
        private static readonly int PatternSpeedBProperty = Shader.PropertyToID("_Pattern_Speed_B");
        
        // Shader Property IDs (NeonGridFlat - Quad용)
        private static readonly int GridColorProperty = Shader.PropertyToID("_GridColor");
        private static readonly int GridDensityProperty = Shader.PropertyToID("_GridDensity");
        private static readonly int LineThicknessProperty = Shader.PropertyToID("_LineThickness");
        private static readonly int IntensityProperty = Shader.PropertyToID("_Intensity");
        
        #endregion

        #region Unity Lifecycle
        
        private void OnEnable()
        {
            _boxCollider = GetComponent<BoxCollider>();
            
            if (_boxCollider == null)
            {
                Debug.LogError($"[NoGoZone] {gameObject.name}: BoxCollider가 필요합니다.", this);
                enabled = false;
                return;
            }
            
            CreateGridMesh();
            SetupMaterial();
            UpdateGridTransform();
            ApplyMaterialProperties();
        }
        
        private void OnDisable()
        {
            CleanupGridMesh();
        }
        
        private void OnValidate()
        {
            // 메시 타입이 변경되면 재생성
            if (_gridMesh != null && _currentMeshType != _meshType)
            {
                CleanupGridMesh();
                CreateGridMesh();
                SetupMaterial();
            }
            
            if (_gridMesh == null || _materialInstance == null)
            {
                return;
            }
            
            UpdateGridTransform();
            ApplyMaterialProperties();
        }
        
        #endregion

        #region Grid Setup
        
        private void CreateGridMesh()
        {
            if (_gridMesh != null) return;
            
            // 선택된 메시 타입으로 생성
            PrimitiveType primitiveType = _meshType == MeshType.Cube 
                ? PrimitiveType.Cube 
                : PrimitiveType.Quad;
            
            _gridMesh = GameObject.CreatePrimitive(primitiveType);
            _gridMesh.name = $"{gameObject.name}_GridVisual";
            _gridMesh.transform.SetParent(transform);
            
            _gridMesh.hideFlags = HideFlags.DontSave;
            _gridMesh.SetActive(true);
            
            // 메시의 Collider 제거 (시각 전용)
            Collider meshCollider = _gridMesh.GetComponent<Collider>();
            if (meshCollider != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(meshCollider);
                }
                else
                {
                    DestroyImmediate(meshCollider);
                }
            }
            
            _meshRenderer = _gridMesh.GetComponent<MeshRenderer>();
            _currentMeshType = _meshType;
        }
        
        private void CleanupGridMesh()
        {
            if (_materialInstance != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_materialInstance);
                }
                else
                {
                    DestroyImmediate(_materialInstance);
                }
                _materialInstance = null;
            }
            
            if (_gridMesh != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_gridMesh);
                }
                else
                {
                    DestroyImmediate(_gridMesh);
                }
                _gridMesh = null;
            }
            
            _meshRenderer = null;
        }
        
        private void SetupMaterial()
        {
            if (_meshRenderer == null) return;
            
            if (_materialInstance != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_materialInstance);
                }
                else
                {
                    DestroyImmediate(_materialInstance);
                }
            }
            
            if (_gridMaterial == null)
            {
                string recommendedPath = _meshType == MeshType.Cube
                    ? "Assets/Holograms/Materials/Examples/Basic/Grid_Hologram_Empty.mat"
                    : "Assets/Materials/M_NeonGridFlat.mat";
                    
                Debug.LogWarning($"[NoGoZone] {gameObject.name}: Grid Material이 할당되지 않았습니다.\n" +
                                 $"추천 경로: {recommendedPath}", this);
                
                _materialInstance = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                _materialInstance.color = _hologramColor;
            }
            else
            {
                _materialInstance = new Material(_gridMaterial);
            }
            
            _materialInstance.hideFlags = HideFlags.DontSave;
            _meshRenderer.sharedMaterial = _materialInstance;
        }
        
        /// <summary>
        /// 선택된 GridFace와 MeshType에 따라 메시의 위치/회전/크기를 설정.
        /// </summary>
        private void UpdateGridTransform()
        {
            if (_gridMesh == null || _boxCollider == null) return;
            
            Vector3 center = _boxCollider.center;
            Vector3 size = _boxCollider.size;
            
            Vector3 position;
            Quaternion rotation;
            Vector3 scale;
            
            // Cube일 때 두께 적용, Quad일 때 두께 = 0 (1로 설정해도 평면이라 무의미)
            float thickness = _meshType == MeshType.Cube ? _cubeThickness : 1f;
            
            switch (_gridFace)
            {
                case GridFace.Top:
                    // 수평면: X × Z 크기, Y 방향 두께
                    float topY = center.y + (size.y / 2f) + _faceOffset;
                    position = new Vector3(center.x, topY, center.z);
                    rotation = _meshType == MeshType.Quad 
                        ? Quaternion.Euler(90f, 0f, 0f) 
                        : Quaternion.identity;
                    scale = _meshType == MeshType.Cube
                        ? new Vector3(size.x, thickness, size.z)
                        : new Vector3(size.x, size.z, 1f);
                    break;
                    
                case GridFace.WallFrontBack:
                    // Z방향 벽: X × Y 크기, Z 방향 두께
                    float frontZ = center.z + (size.z / 2f) + _faceOffset;
                    position = new Vector3(center.x, center.y, frontZ);
                    rotation = Quaternion.identity;
                    scale = _meshType == MeshType.Cube
                        ? new Vector3(size.x, size.y, thickness)
                        : new Vector3(size.x, size.y, 1f);
                    break;
                    
                case GridFace.WallLeftRight:
                    // X방향 벽: Z × Y 크기, X 방향 두께
                    float sideX = center.x + (size.x / 2f) + _faceOffset;
                    position = new Vector3(sideX, center.y, center.z);
                    rotation = _meshType == MeshType.Quad 
                        ? Quaternion.Euler(0f, 90f, 0f) 
                        : Quaternion.identity;
                    scale = _meshType == MeshType.Cube
                        ? new Vector3(thickness, size.y, size.z)
                        : new Vector3(size.z, size.y, 1f);
                    break;
                    
                default:
                    position = center;
                    rotation = Quaternion.identity;
                    scale = Vector3.one;
                    break;
            }
            
            _gridMesh.transform.localPosition = position;
            _gridMesh.transform.localRotation = rotation;
            _gridMesh.transform.localScale = scale;
        }
        
        /// <summary>
        /// Material 속성을 Inspector 값으로 업데이트.
        /// 메시 타입에 따라 다른 Property 사용.
        /// </summary>
        private void ApplyMaterialProperties()
        {
            if (_materialInstance == null) return;
            
            // Holograms 에셋 Property (Cube용)
            if (_materialInstance.HasProperty(HologramColorProperty))
            {
                _materialInstance.SetColor(HologramColorProperty, _hologramColor);
            }
            
            if (_materialInstance.HasProperty(EmissionScaleProperty))
            {
                _materialInstance.SetFloat(EmissionScaleProperty, _emissionScale);
            }
            
            if (_materialInstance.HasProperty(PatternDensityProperty))
            {
                _materialInstance.SetFloat(PatternDensityProperty, _patternDensity);
            }
            
            if (_materialInstance.HasProperty(FresnelPowerProperty))
            {
                _materialInstance.SetFloat(FresnelPowerProperty, _fresnelPower);
            }
            
            if (_materialInstance.HasProperty(PatternSpeedAProperty))
            {
                _materialInstance.SetFloat(PatternSpeedAProperty, _patternSpeedA);
            }
            
            if (_materialInstance.HasProperty(PatternSpeedBProperty))
            {
                _materialInstance.SetFloat(PatternSpeedBProperty, _patternSpeedB);
            }
            
            // NeonGridFlat Property (Quad용)
            if (_materialInstance.HasProperty(GridColorProperty))
            {
                _materialInstance.SetColor(GridColorProperty, _hologramColor);
            }
            
            if (_materialInstance.HasProperty(GridDensityProperty))
            {
                _materialInstance.SetFloat(GridDensityProperty, _patternDensity / 25f); // 스케일 조정
            }
            
            if (_materialInstance.HasProperty(IntensityProperty))
            {
                _materialInstance.SetFloat(IntensityProperty, _emissionScale / 20f); // 스케일 조정
            }
        }
        
        #endregion

        #region Public API
        
        public void SetHologramColor(Color newColor)
        {
            _hologramColor = newColor;
            ApplyMaterialProperties();
        }
        
        public void SetGridVisible(bool isVisible)
        {
            if (_gridMesh != null)
            {
                _gridMesh.SetActive(isVisible);
            }
        }
        
        public void SetEmissionScale(float scale)
        {
            _emissionScale = Mathf.Clamp(scale, 1f, 50f);
            ApplyMaterialProperties();
        }
        
        public void SetPatternDensity(float density)
        {
            _patternDensity = Mathf.Clamp(density, 50f, 500f);
            ApplyMaterialProperties();
        }
        
        public void SetGridFace(GridFace face)
        {
            _gridFace = face;
            UpdateGridTransform();
        }
        
        public void SetMeshType(MeshType meshType)
        {
            if (_meshType != meshType)
            {
                _meshType = meshType;
                CleanupGridMesh();
                CreateGridMesh();
                SetupMaterial();
                UpdateGridTransform();
                ApplyMaterialProperties();
            }
        }
        
        #endregion

        #region Editor Gizmos
        
        private void OnDrawGizmosSelected()
        {
            BoxCollider col = GetComponent<BoxCollider>();
            if (col == null) return;
            
            Gizmos.color = new Color(_hologramColor.r, _hologramColor.g, _hologramColor.b, 0.2f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.center, col.size);
            
            Gizmos.color = new Color(_hologramColor.r, _hologramColor.g, _hologramColor.b, 0.5f);
            Vector3 center = col.center;
            Vector3 size = col.size;
            
            Vector3 faceCenter;
            Vector3 faceSize;
            float thickness = _meshType == MeshType.Cube ? _cubeThickness : 0.01f;
            
            switch (_gridFace)
            {
                case GridFace.Top:
                    faceCenter = center + Vector3.up * (size.y / 2f);
                    faceSize = new Vector3(size.x, thickness, size.z);
                    break;
                case GridFace.WallFrontBack:
                    faceCenter = center + Vector3.forward * (size.z / 2f);
                    faceSize = new Vector3(size.x, size.y, thickness);
                    break;
                case GridFace.WallLeftRight:
                    faceCenter = center + Vector3.right * (size.x / 2f);
                    faceSize = new Vector3(thickness, size.y, size.z);
                    break;
                default:
                    faceCenter = center;
                    faceSize = size;
                    break;
            }
            
            Gizmos.DrawCube(faceCenter, faceSize);
        }
        
        #endregion
    }
}
