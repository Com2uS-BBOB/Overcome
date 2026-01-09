using UnityEngine;

/// <summary>
/// 레이어별 거리 컬링을 적용하는 카메라 컴포넌트
/// 
/// 사용법: 메인 카메라에 이 스크립트를 추가하세요.
/// OptimizedLayerAssigner로 레이어 설정 후 사용합니다.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraLayerCulling : MonoBehaviour
{
    [Header("레이어 컬링 거리")]
    [Tooltip("ENV_Structure (Layer 22) - 빌딩/도로")]
    public float structureDistance = 0f; // 0 = 카메라 far clip plane 사용
    
    [Tooltip("DECO_Large (Layer 21) - 차량/가로등")]
    public float largeDistance = 150f;
    
    [Tooltip("DECO_Medium (Layer 20) - 상자/파이프")]
    public float mediumDistance = 80f;
    
    [Tooltip("DECO_Detail (Layer 19) - 그래피티/데칼")]
    public float detailDistance = 40f;
    
    [Header("설정")]
    public bool useSphericalCulling = true;
    
    private Camera _camera;
    
    void Start()
    {
        ApplyCulling();
    }
    
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplyCulling();
        }
    }
    
    public void ApplyCulling()
    {
        _camera = GetComponent<Camera>();
        if (_camera == null) return;
        
        float[] distances = new float[32];
        float defaultDist = _camera.farClipPlane;
        
        // 기본값: 카메라 far clip plane
        for (int i = 0; i < 32; i++)
        {
            distances[i] = defaultDist;
        }
        
        // 레이어별 컬링 거리 설정
        distances[19] = detailDistance > 0 ? detailDistance : defaultDist;     // DECO_Detail
        distances[20] = mediumDistance > 0 ? mediumDistance : defaultDist;     // DECO_Medium
        distances[21] = largeDistance > 0 ? largeDistance : defaultDist;       // DECO_Large
        distances[22] = structureDistance > 0 ? structureDistance : defaultDist; // ENV_Structure
        
        _camera.layerCullDistances = distances;
        _camera.layerCullSpherical = useSphericalCulling;
        
        Debug.Log($"[CameraLayerCulling] 적용됨 - Detail:{detailDistance}m, Medium:{mediumDistance}m, Large:{largeDistance}m");
    }
    
    // Inspector에서 테스트용
    [ContextMenu("Apply Culling Now")]
    public void ApplyCullingManual()
    {
        ApplyCulling();
    }
}
