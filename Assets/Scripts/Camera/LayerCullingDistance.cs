using UnityEngine;

/// <summary>
/// 카메라의 Layer Culling Distance를 설정합니다.
/// 레이어별로 다른 거리에서 오브젝트를 컬링하여 렌더링 성능을 최적화합니다.
/// 
/// 사용법: Main Camera에 이 컴포넌트를 추가하세요.
/// 
/// 컬링 거리 설명:
/// - 0 = Far Clip Plane 사용 (컬링 없음)
/// - 양수 값 = 해당 거리 이상에서 레이어의 오브젝트가 렌더링되지 않음
/// </summary>
[RequireComponent(typeof(Camera))]
public class LayerCullingDistance : MonoBehaviour
{
    [Header("레이어 컬링 거리 설정")]
    [Tooltip("SmallProps 레이어 컬링 거리 (미터)")]
    [SerializeField] private float _smallPropsCullDistance = 50f;
    
    [Tooltip("MediumProps 레이어 컬링 거리 (미터)")]
    [SerializeField] private float _mediumPropsCullDistance = 100f;
    
    [Tooltip("Buildings 레이어 컬링 거리 (0 = 컬링 없음)")]
    [SerializeField] private float _buildingsCullDistance = 0f;
    
    [Header("레이어 이름 (Project Settings와 일치해야 함)")]
    [SerializeField] private string _smallPropsLayerName = "SmallProps";
    [SerializeField] private string _mediumPropsLayerName = "MediumProps";
    [SerializeField] private string _buildingsLayerName = "Buildings";
    
    [Header("디버그")]
    [SerializeField] private bool _showDebugLog = true;
    
    private Camera _camera;
    
    private void Awake()
    {
        _camera = GetComponent<Camera>();
        
        if (_camera == null)
        {
            Debug.LogError($"[LayerCullingDistance] Camera 컴포넌트를 찾을 수 없습니다: {gameObject.name}");
            return;
        }
        
        ApplyCullingDistances();
    }
    
    /// <summary>
    /// 레이어별 컬링 거리를 카메라에 적용합니다.
    /// </summary>
    public void ApplyCullingDistances()
    {
        if (_camera == null) return;
        
        // Unity는 32개 레이어를 지원 (0~31)
        float[] distances = new float[32];
        
        // 레이어 인덱스 찾기
        int smallPropsLayer = LayerMask.NameToLayer(_smallPropsLayerName);
        int mediumPropsLayer = LayerMask.NameToLayer(_mediumPropsLayerName);
        int buildingsLayer = LayerMask.NameToLayer(_buildingsLayerName);
        
        // 레이어 존재 확인 및 거리 설정
        bool hasError = false;
        
        if (smallPropsLayer != -1)
        {
            distances[smallPropsLayer] = _smallPropsCullDistance;
        }
        else
        {
            Debug.LogWarning($"[LayerCullingDistance] '{_smallPropsLayerName}' 레이어를 찾을 수 없습니다.");
            hasError = true;
        }
        
        if (mediumPropsLayer != -1)
        {
            distances[mediumPropsLayer] = _mediumPropsCullDistance;
        }
        else
        {
            Debug.LogWarning($"[LayerCullingDistance] '{_mediumPropsLayerName}' 레이어를 찾을 수 없습니다.");
            hasError = true;
        }
        
        if (buildingsLayer != -1)
        {
            distances[buildingsLayer] = _buildingsCullDistance;
        }
        else
        {
            Debug.LogWarning($"[LayerCullingDistance] '{_buildingsLayerName}' 레이어를 찾을 수 없습니다.");
            hasError = true;
        }
        
        // 카메라에 적용
        _camera.layerCullDistances = distances;
        
        // 디버그 로그
        if (_showDebugLog && !hasError)
        {
            Debug.Log($"[LayerCullingDistance] 컬링 거리 적용 완료:\n" +
                     $"  • {_smallPropsLayerName} (Layer {smallPropsLayer}): {_smallPropsCullDistance}m\n" +
                     $"  • {_mediumPropsLayerName} (Layer {mediumPropsLayer}): {_mediumPropsCullDistance}m\n" +
                     $"  • {_buildingsLayerName} (Layer {buildingsLayer}): {(_buildingsCullDistance == 0 ? "무한" : _buildingsCullDistance + "m")}");
        }
    }
    
    /// <summary>
    /// 런타임에 컬링 거리를 변경할 때 사용합니다.
    /// </summary>
    public void SetCullingDistance(string layerName, float distance)
    {
        int layerIndex = LayerMask.NameToLayer(layerName);
        
        if (layerIndex == -1)
        {
            Debug.LogWarning($"[LayerCullingDistance] '{layerName}' 레이어를 찾을 수 없습니다.");
            return;
        }
        
        float[] distances = _camera.layerCullDistances;
        distances[layerIndex] = distance;
        _camera.layerCullDistances = distances;
        
        if (_showDebugLog)
        {
            Debug.Log($"[LayerCullingDistance] {layerName} 컬링 거리 변경: {distance}m");
        }
    }
    
    // 인스펙터에서 값 변경 시 즉시 적용 (에디터 전용)
    #if UNITY_EDITOR
    private void OnValidate()
    {
        // Play 모드에서만 적용
        if (Application.isPlaying && _camera != null)
        {
            ApplyCullingDistances();
        }
    }
    #endif
}
