using UnityEngine;

namespace NeonHighCity.Environment
{
    /// <summary>
    /// BoxCollider 영역에 네온 그리드를 표시하는 컴포넌트.
    /// 플레이어가 갈 수 없는 구역을 시각적으로 표현한다.
    /// 
    /// [ExecuteAlways]: Edit 모드에서도 실행되어 실시간 미리보기 가능.
    /// 사용 셰이더: Custom/NeonGridFlat (평면 전용)
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
        
        #endregion

        #region Inspector Fields
        
        [Header("Grid Face")]
        [Tooltip("그리드가 표시될 면 방향\n" +
                 "• Top: 바닥/천장 (수평)\n" +
                 "• WallFrontBack: 앞뒤 벽 (Z축 방향)\n" +
                 "• WallLeftRight: 좌우 벽 (X축 방향)")]
        [SerializeField] private GridFace _gridFace = GridFace.WallLeftRight;
        
        [Tooltip("그리드 위치 오프셋 (면의 법선 방향)\n" +
                 "양수: 바깥쪽, 음수: 안쪽")]
        [SerializeField] private float _faceOffset = 0.01f;
        
        [Header("Grid Material")]
        [Tooltip("그리드 셰이더가 적용된 Material.\n" +
                 "추천: Assets/Materials/M_NeonGridFlat.mat")]
        [SerializeField] private Material _gridMaterial;
        
        [Header("Grid Appearance")]
        [Tooltip("그리드 색상 (HDR 지원, 밝기 높이면 Bloom 효과)")]
        [ColorUsage(true, true)]
        [SerializeField] private Color _gridColor = new Color(0f, 2f, 2f, 1f);
        
        [Tooltip("그리드 선 밀도 (값이 클수록 촘촘함)")]
        [SerializeField] [Range(1f, 50f)] private float _gridDensity = 10f;
        
        [Tooltip("그리드 선 두께 (값이 클수록 두꺼움)")]
        [SerializeField] [Range(0.01f, 0.2f)] private float _lineThickness = 0.05f;
        
        [Tooltip("발광 강도 (높을수록 밝음)")]
        [SerializeField] [Range(0.1f, 5f)] private float _intensity = 1f;
        
        [Header("Animation")]
        [Tooltip("X축 스크롤 속도 (0이면 정지)")]
        [SerializeField] [Range(-10f, 10f)] private float _scrollSpeedX = 0f;
        
        [Tooltip("Y축 스크롤 속도 (0이면 정지)")]
        [SerializeField] [Range(-10f, 10f)] private float _scrollSpeedY = 0.5f;
        
        [Header("Pulse Animation")]
        [Tooltip("밝기 깜빡임(Pulse) 활성화")]
        [SerializeField] private bool _enablePulse = false;
        
        [Tooltip("깜빡임 속도")]
        [SerializeField] [Range(0.5f, 5f)] private float _pulseSpeed = 2f;
        
        [Tooltip("깜빡임 최소 강도 비율 (0~1)")]
        [SerializeField] [Range(0f, 1f)] private float _pulseMinRatio = 0.3f;
        
        #endregion

        #region Private Fields
        
        private BoxCollider _boxCollider;
        private GameObject _gridQuad;
        private MeshRenderer _quadRenderer;
        private Material _materialInstance;
        
        private float _baseIntensity;
        
        // Shader Property IDs
        private static readonly int GridColorProperty = Shader.PropertyToID("_GridColor");
        private static readonly int GridDensityProperty = Shader.PropertyToID("_GridDensity");
        private static readonly int LineThicknessProperty = Shader.PropertyToID("_LineThickness");
        private static readonly int IntensityProperty = Shader.PropertyToID("_Intensity");
        private static readonly int ScrollSpeedXProperty = Shader.PropertyToID("_ScrollSpeedX");
        private static readonly int ScrollSpeedYProperty = Shader.PropertyToID("_ScrollSpeedY");
        
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
            
            CreateGridQuad();
            SetupMaterial();
            UpdateGridTransform();
            ApplyMaterialProperties();
            
            _baseIntensity = _intensity;
        }
        
        private void OnDisable()
        {
            CleanupGridQuad();
        }
        
        private void Update()
        {
            if (Application.isPlaying && _enablePulse && _materialInstance != null)
            {
                UpdatePulseAnimation();
            }
        }
        
        private void OnValidate()
        {
            if (_gridQuad == null || _materialInstance == null)
            {
                return;
            }
            
            UpdateGridTransform();
            ApplyMaterialProperties();
            _baseIntensity = _intensity;
        }
        
        #endregion

        #region Grid Setup
        
        private void CreateGridQuad()
        {
            if (_gridQuad != null) return;
            
            _gridQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _gridQuad.name = $"{gameObject.name}_GridVisual";
            _gridQuad.transform.SetParent(transform);
            
            // DontSave만 설정 (NotEditable 제거 - 편집 가능하게)
            _gridQuad.hideFlags = HideFlags.DontSave;
            
            // 명시적으로 활성화
            _gridQuad.SetActive(true);
            
            // Quad의 Collider 제거
            Collider quadCollider = _gridQuad.GetComponent<Collider>();
            if (quadCollider != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(quadCollider);
                }
                else
                {
                    DestroyImmediate(quadCollider);
                }
            }
            
            _quadRenderer = _gridQuad.GetComponent<MeshRenderer>();
        }
        
        private void CleanupGridQuad()
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
            
            if (_gridQuad != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_gridQuad);
                }
                else
                {
                    DestroyImmediate(_gridQuad);
                }
                _gridQuad = null;
            }
            
            _quadRenderer = null;
        }
        
        private void SetupMaterial()
        {
            if (_quadRenderer == null) return;
            
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
                Shader gridShader = Shader.Find("Custom/NeonGridFlat");
                
                if (gridShader != null)
                {
                    _materialInstance = new Material(gridShader);
                }
                else
                {
                    Debug.LogWarning($"[NoGoZone] {gameObject.name}: Grid Material이 할당되지 않았습니다.\n" +
                                     "추천 경로: Assets/Materials/M_NeonGridFlat.mat", this);
                    
                    _materialInstance = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                    _materialInstance.color = _gridColor;
                }
            }
            else
            {
                _materialInstance = new Material(_gridMaterial);
            }
            
            _materialInstance.hideFlags = HideFlags.DontSave;
            _quadRenderer.sharedMaterial = _materialInstance;
        }
        
        private void UpdateGridTransform()
        {
            if (_gridQuad == null || _boxCollider == null) return;
            
            Vector3 center = _boxCollider.center;
            Vector3 size = _boxCollider.size;
            
            Vector3 position;
            Quaternion rotation;
            Vector3 scale;
            
            switch (_gridFace)
            {
                case GridFace.Top:
                    float topY = center.y + (size.y / 2f) + _faceOffset;
                    position = new Vector3(center.x, topY, center.z);
                    rotation = Quaternion.Euler(90f, 0f, 0f);
                    scale = new Vector3(size.x, size.z, 1f);
                    break;
                    
                case GridFace.WallFrontBack:
                    float frontZ = center.z + (size.z / 2f) + _faceOffset;
                    position = new Vector3(center.x, center.y, frontZ);
                    rotation = Quaternion.identity;
                    scale = new Vector3(size.x, size.y, 1f);
                    break;
                    
                case GridFace.WallLeftRight:
                    float sideX = center.x + (size.x / 2f) + _faceOffset;
                    position = new Vector3(sideX, center.y, center.z);
                    rotation = Quaternion.Euler(0f, 90f, 0f);
                    scale = new Vector3(size.z, size.y, 1f);
                    break;
                    
                default:
                    position = center;
                    rotation = Quaternion.identity;
                    scale = Vector3.one;
                    break;
            }
            
            _gridQuad.transform.localPosition = position;
            _gridQuad.transform.localRotation = rotation;
            _gridQuad.transform.localScale = scale;
        }
        
        private void ApplyMaterialProperties()
        {
            if (_materialInstance == null) return;
            
            if (_materialInstance.HasProperty(GridColorProperty))
            {
                _materialInstance.SetColor(GridColorProperty, _gridColor);
            }
            
            if (_materialInstance.HasProperty(GridDensityProperty))
            {
                _materialInstance.SetFloat(GridDensityProperty, _gridDensity);
            }
            
            if (_materialInstance.HasProperty(LineThicknessProperty))
            {
                _materialInstance.SetFloat(LineThicknessProperty, _lineThickness);
            }
            
            if (_materialInstance.HasProperty(IntensityProperty))
            {
                _materialInstance.SetFloat(IntensityProperty, _intensity);
            }
            
            if (_materialInstance.HasProperty(ScrollSpeedXProperty))
            {
                _materialInstance.SetFloat(ScrollSpeedXProperty, _scrollSpeedX);
            }
            
            if (_materialInstance.HasProperty(ScrollSpeedYProperty))
            {
                _materialInstance.SetFloat(ScrollSpeedYProperty, _scrollSpeedY);
            }
            
            if (_materialInstance.HasProperty("_BaseColor"))
            {
                _materialInstance.SetColor("_BaseColor", _gridColor);
            }
        }
        
        #endregion

        #region Animation
        
        private void UpdatePulseAnimation()
        {
            float pulse = Mathf.Sin(Time.time * _pulseSpeed) * 0.5f + 0.5f;
            float minIntensity = _baseIntensity * _pulseMinRatio;
            float currentIntensity = Mathf.Lerp(minIntensity, _baseIntensity, pulse);
            
            if (_materialInstance.HasProperty(IntensityProperty))
            {
                _materialInstance.SetFloat(IntensityProperty, currentIntensity);
            }
        }
        
        #endregion

        #region Public API
        
        public void SetGridColor(Color newColor)
        {
            _gridColor = newColor;
            ApplyMaterialProperties();
        }
        
        public void SetGridVisible(bool isVisible)
        {
            if (_gridQuad != null)
            {
                _gridQuad.SetActive(isVisible);
            }
        }
        
        public void SetGridDensity(float density)
        {
            _gridDensity = Mathf.Clamp(density, 1f, 50f);
            ApplyMaterialProperties();
        }
        
        public void SetIntensity(float intensity)
        {
            _intensity = Mathf.Clamp(intensity, 0.1f, 5f);
            _baseIntensity = _intensity;
            ApplyMaterialProperties();
        }
        
        public void SetLineThickness(float thickness)
        {
            _lineThickness = Mathf.Clamp(thickness, 0.01f, 0.2f);
            ApplyMaterialProperties();
        }
        
        public void SetGridFace(GridFace face)
        {
            _gridFace = face;
            UpdateGridTransform();
        }
        
        #endregion

        #region Editor Gizmos
        
        private void OnDrawGizmosSelected()
        {
            BoxCollider col = GetComponent<BoxCollider>();
            if (col == null) return;
            
            Gizmos.color = new Color(_gridColor.r, _gridColor.g, _gridColor.b, 0.2f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.center, col.size);
            
            Gizmos.color = new Color(_gridColor.r, _gridColor.g, _gridColor.b, 0.5f);
            Vector3 center = col.center;
            Vector3 size = col.size;
            
            Vector3 faceCenter;
            Vector3 faceSize;
            
            switch (_gridFace)
            {
                case GridFace.Top:
                    faceCenter = center + Vector3.up * (size.y / 2f);
                    faceSize = new Vector3(size.x, 0.01f, size.z);
                    break;
                case GridFace.WallFrontBack:
                    faceCenter = center + Vector3.forward * (size.z / 2f);
                    faceSize = new Vector3(size.x, size.y, 0.01f);
                    break;
                case GridFace.WallLeftRight:
                    faceCenter = center + Vector3.right * (size.x / 2f);
                    faceSize = new Vector3(0.01f, size.y, size.z);
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
