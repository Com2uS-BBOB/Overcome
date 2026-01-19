using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

/// <summary>
/// 최적화된 레이어 할당 도구 v4
/// 
/// ★ DLNK Neon City 에셋에 최적화
/// ★ 씬 계층 구조 기반 분류 (더 정확하고 직관적)
/// 
/// 레이어 체계 (4단계):
/// - ENV_Structure (22): 도로, 플랫폼, 빌딩 메인 구조 → 컬링 없음/500m
/// - DECO_Large (21): 차량, 가로등, 대형 간판 → 150m
/// - DECO_Medium (20): 상자, 케이블폴, 파이프, 벤치 → 80m  
/// - DECO_Detail (19): 그래피티, 데칼, 작은 장식 → 40m
/// 
/// 분류 방식:
/// 1. 씬 루트 오브젝트 이름으로 1차 분류
/// 2. 자식 오브젝트 이름으로 2차 세분화
/// </summary>
public class OptimizedLayerAssigner : EditorWindow
{
    // ===== 레이어 설정 =====
    private const string Layer_ENV_Structure = "ENV_Structure";  // Layer 22
    private const string Layer_DECO_Large = "DECO_Large";        // Layer 21
    private const string Layer_DECO_Medium = "DECO_Medium";      // Layer 20
    private const string Layer_DECO_Detail = "DECO_Detail";      // Layer 19
    
    // ===== 컬링 거리 =====
    private const float CullDistance_Structure = 0f;    // 컬링 없음 (카메라 far clip 사용)
    private const float CullDistance_Large = 150f;
    private const float CullDistance_Medium = 80f;
    private const float CullDistance_Detail = 40f;
    
    // =====================================================================
    // 씬 루트 오브젝트 분류 규칙
    // =====================================================================
    
    /// <summary>
    /// ENV_Structure: 도로, 플랫폼, 빌딩 메인 구조
    /// </summary>
    private static readonly string[] RootPatterns_Structure = new string[]
    {
        "ENV_Streets",
        "ENV_Platforms", 
        "ENV_BuildingDeco",
        "ENV_BridgePlatform",
        "ENV_PlatformLow",
        "ENV_StreetHigh",
        "EnvironFxs"
    };
    
    /// <summary>
    /// DECO_Large: 차량, 가로등
    /// </summary>
    private static readonly string[] RootPatterns_Large = new string[]
    {
        "DECO_StreetLights",
        "DECO_Vehicles"
    };
    
    /// <summary>
    /// DECO_Medium: 중간 크기 소품
    /// </summary>
    private static readonly string[] RootPatterns_Medium = new string[]
    {
        "DECO_StreetDeco",
        "DECO_EnviroDeco",
        "DECO_ADS",
        "DECO_Vegetation"
    };
    
    /// <summary>
    /// DECO_Detail: 작은 장식, 데칼
    /// </summary>
    private static readonly string[] RootPatterns_Detail = new string[]
    {
        "DECO_Grafitti",
        "DECO_Graffiti",
        "DECO_StreetDecals"
    };

    // =====================================================================
    // 자식 오브젝트 세분화 키워드
    // =====================================================================
    
    /// <summary>
    /// DECO_Detail로 분류할 자식 키워드 (가장 작은 요소)
    /// </summary>
    private static readonly string[] ChildKeywords_Detail = new string[]
    {
        // 데칼/그래피티
        "Decal", "Graffiti", "Grafitti", "Poster", "Sticker",
        // 작은 장식
        "_Detail", "_Small", "_Tiny",
        "ADS_", "_ADS",
        // FX
        "FX", "Particle", "Glow"
    };
    
    /// <summary>
    /// DECO_Medium으로 분류할 자식 키워드
    /// </summary>
    private static readonly string[] ChildKeywords_Medium = new string[]
    {
        // 중간 크기 소품
        "Crate", "Barrel", "Box0", "Bin", "Trash",
        "Cable", "Pipe", "Vent", "AC_",
        // 가구
        "Bench", "Chair", "Table",
        // 표지판 (작은 것)
        "Sign_", "_Sign", "RoadSign",
        // 식물
        "Plant", "Bush", "Flower"
    };
    
    /// <summary>
    /// DECO_Large로 분류할 자식 키워드
    /// </summary>
    private static readonly string[] ChildKeywords_Large = new string[]
    {
        // 차량
        "Car", "Vehicle", "Truck", "Bus", "Train",
        "FloatingCar", "UrbanCar", "FlyingCar",
        // 조명
        "StreetLight", "StreetLamp", "Lamp", "Light_",
        // 대형 구조물
        "Tank", "Tower", "Antenna",
        "BusStop", "TrafficLight",
        // 나무
        "Tree"
    };
    
    /// <summary>
    /// ENV_Structure로 유지해야 할 자식 키워드 (항상 보여야 함)
    /// </summary>
    private static readonly string[] ChildKeywords_Structure = new string[]
    {
        // 건물 구조
        "Building", "Wall", "Floor", "Ground", "Roof",
        "_Wall", "_Floor", "_Ground",
        // 도로/플랫폼
        "Street", "Road", "Platform", "Bridge",
        // 구조적 요소
        "Column", "Pillar", "Beam", "Frame",
        "Window", "Door", "Gate", "Stair"
    };
    
    // ===== UI 변수 =====
    private Vector2 _scrollPosition;
    private List<PreviewItem> _previewItems = new List<PreviewItem>();
    private bool _hasPreview = false;
    
    // 통계
    private Dictionary<string, int> _layerCounts = new Dictionary<string, int>();
    
    private struct PreviewItem
    {
        public string ObjectPath;
        public string CurrentLayer;
        public string NewLayer;
        public bool WillChange;
    }

    [MenuItem("Tools/Optimization/Optimized Layer Assigner")]
    public static void ShowWindow()
    {
        var window = GetWindow<OptimizedLayerAssigner>("Optimized Layer Assigner");
        window.minSize = new Vector2(600, 550);
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("🏙️ 최적화된 레이어 할당 도구 v4", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "DLNK Neon City 에셋에 최적화된 레이어 할당 도구입니다.\n\n" +
            "★ 씬 계층 구조 기반 자동 분류\n" +
            "★ 4단계 레이어로 세분화된 컬링\n\n" +
            "레이어 체계:\n" +
            $"• ENV_Structure (22): 도로/플랫폼/빌딩 → 컬링 없음\n" +
            $"• DECO_Large (21): 차량/가로등 → {CullDistance_Large}m\n" +
            $"• DECO_Medium (20): 상자/파이프/벤치 → {CullDistance_Medium}m\n" +
            $"• DECO_Detail (19): 그래피티/데칼 → {CullDistance_Detail}m",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        // 레이어 상태 표시
        DrawLayerStatus();
        
        EditorGUILayout.Space(10);
        
        // 버튼
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("1. 레이어 자동 생성", GUILayout.Height(30)))
        {
            CreateRequiredLayers();
        }
        
        if (GUILayout.Button("2. 미리보기", GUILayout.Height(30)))
        {
            GeneratePreview();
        }
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("3. 씬에 적용", GUILayout.Height(30)))
        {
            if (!ValidateLayers())
            {
                EditorUtility.DisplayDialog("오류", 
                    "필요한 레이어가 없습니다.\n'레이어 자동 생성' 버튼을 먼저 클릭하세요.", 
                    "확인");
                return;
            }
            
            if (EditorUtility.DisplayDialog("확인", 
                "씬의 모든 오브젝트에 최적화된 레이어를 적용합니다.\n\n" +
                "계속하시겠습니까?", 
                "적용", "취소"))
            {
                ApplyLayersToScene();
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // 추가 도구
        EditorGUILayout.BeginHorizontal();
        
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("카메라 컬링 스크립트 생성", GUILayout.Height(25)))
        {
            CreateCameraCullingScript();
        }
        GUI.backgroundColor = Color.white;
        
        if (GUILayout.Button("보고서 생성", GUILayout.Height(25)))
        {
            GenerateReport();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // 미리보기 결과
        if (_hasPreview)
        {
            DrawPreviewResults();
        }
    }

    private void DrawLayerStatus()
    {
        EditorGUILayout.LabelField("레이어 상태:", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        DrawLayerStatusIcon(Layer_ENV_Structure, 22);
        DrawLayerStatusIcon(Layer_DECO_Large, 21);
        DrawLayerStatusIcon(Layer_DECO_Medium, 20);
        DrawLayerStatusIcon(Layer_DECO_Detail, 19);
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawLayerStatusIcon(string expectedName, int layerIndex)
    {
        string actualName = LayerMask.LayerToName(layerIndex);
        bool isCorrect = actualName == expectedName;
        bool isEmpty = string.IsNullOrEmpty(actualName);
        
        string status;
        Color color;
        
        if (isCorrect)
        {
            status = "✓";
            color = Color.green;
        }
        else if (isEmpty)
        {
            status = "○";
            color = Color.yellow;
        }
        else
        {
            status = "✗";
            color = Color.red;
        }
        
        GUIStyle style = new GUIStyle(EditorStyles.label);
        style.normal.textColor = color;
        
        string tooltip = isEmpty ? "(비어있음)" : $"(현재: {actualName})";
        EditorGUILayout.LabelField(
            new GUIContent($"{status} [{layerIndex}] {expectedName}", tooltip), 
            style, GUILayout.Width(145));
    }
    
    private bool ValidateLayers()
    {
        return LayerMask.NameToLayer(Layer_ENV_Structure) != -1 &&
               LayerMask.NameToLayer(Layer_DECO_Large) != -1 &&
               LayerMask.NameToLayer(Layer_DECO_Medium) != -1 &&
               LayerMask.NameToLayer(Layer_DECO_Detail) != -1;
    }
    
    private void CreateRequiredLayers()
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty layers = tagManager.FindProperty("layers");
        
        SetLayerName(layers, 19, Layer_DECO_Detail);
        SetLayerName(layers, 20, Layer_DECO_Medium);
        SetLayerName(layers, 21, Layer_DECO_Large);
        SetLayerName(layers, 22, Layer_ENV_Structure);
        
        tagManager.ApplyModifiedProperties();
        
        Debug.Log("✅ 레이어 생성 완료!");
        EditorUtility.DisplayDialog("완료", 
            "레이어가 생성되었습니다.\n\n" +
            $"[19] {Layer_DECO_Detail}\n" +
            $"[20] {Layer_DECO_Medium}\n" +
            $"[21] {Layer_DECO_Large}\n" +
            $"[22] {Layer_ENV_Structure}", 
            "확인");
    }
    
    private void SetLayerName(SerializedProperty layers, int index, string name)
    {
        SerializedProperty layer = layers.GetArrayElementAtIndex(index);
        if (string.IsNullOrEmpty(layer.stringValue))
        {
            layer.stringValue = name;
            Debug.Log($"  Layer {index} = {name}");
        }
        else if (layer.stringValue != name)
        {
            Debug.LogWarning($"  Layer {index} 이미 사용 중: {layer.stringValue} (→ {name} 로 변경됨)");
            layer.stringValue = name;
        }
    }

    private void GeneratePreview()
    {
        _previewItems.Clear();
        _layerCounts.Clear();
        _layerCounts[Layer_ENV_Structure] = 0;
        _layerCounts[Layer_DECO_Large] = 0;
        _layerCounts[Layer_DECO_Medium] = 0;
        _layerCounts[Layer_DECO_Detail] = 0;
        _layerCounts["Unchanged"] = 0;
        
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager
            .GetActiveScene().GetRootGameObjects();
        
        foreach (var root in rootObjects)
        {
            if (ShouldSkipObject(root)) continue;
            PreviewObjectRecursively(root, root.name);
        }
        
        _hasPreview = true;
        
        int totalChanges = _layerCounts.Values.Sum() - _layerCounts["Unchanged"];
        Debug.Log($"[OptimizedLayerAssigner] 미리보기 완료: {totalChanges}개 변경 예정");
    }
    
    private void PreviewObjectRecursively(GameObject obj, string rootParentName)
    {
        string newLayer = DetermineLayer(obj, rootParentName);
        string currentLayer = LayerMask.LayerToName(obj.layer);
        
        bool willChange = !string.IsNullOrEmpty(newLayer) && currentLayer != newLayer;
        
        if (willChange)
        {
            _layerCounts[newLayer]++;
            
            // 처음 50개만 상세 표시
            if (_previewItems.Count < 50)
            {
                _previewItems.Add(new PreviewItem
                {
                    ObjectPath = GetHierarchyPath(obj),
                    CurrentLayer = currentLayer,
                    NewLayer = newLayer,
                    WillChange = true
                });
            }
        }
        else
        {
            _layerCounts["Unchanged"]++;
        }
        
        foreach (Transform child in obj.transform)
        {
            PreviewObjectRecursively(child.gameObject, rootParentName);
        }
    }
    
    private void ApplyLayersToScene()
    {
        int totalChanged = 0;
        
        Undo.SetCurrentGroupName("Apply Optimized Layers");
        int undoGroup = Undo.GetCurrentGroup();
        
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager
            .GetActiveScene().GetRootGameObjects();
        
        foreach (var root in rootObjects)
        {
            if (ShouldSkipObject(root)) continue;
            totalChanged += ApplyLayersRecursively(root, root.name);
        }
        
        Undo.CollapseUndoOperations(undoGroup);
        
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        
        Debug.Log($"✅ [OptimizedLayerAssigner] 적용 완료: {totalChanged}개 오브젝트 변경");
        EditorUtility.DisplayDialog("완료", 
            $"{totalChanged}개 오브젝트의 레이어가 변경되었습니다.\n\n" +
            "Ctrl+S로 씬을 저장하세요.", 
            "확인");
        
        GeneratePreview();
    }
    
    private int ApplyLayersRecursively(GameObject obj, string rootParentName)
    {
        int changed = 0;
        
        string newLayerName = DetermineLayer(obj, rootParentName);
        
        if (!string.IsNullOrEmpty(newLayerName))
        {
            int newLayerIndex = LayerMask.NameToLayer(newLayerName);
            if (newLayerIndex != -1 && obj.layer != newLayerIndex)
            {
                Undo.RecordObject(obj, "Change Layer");
                obj.layer = newLayerIndex;
                changed++;
            }
        }
        
        foreach (Transform child in obj.transform)
        {
            changed += ApplyLayersRecursively(child.gameObject, rootParentName);
        }
        
        return changed;
    }

    /// <summary>
    /// 오브젝트의 레이어를 결정합니다.
    /// 1차: 루트 부모 이름으로 기본 분류
    /// 2차: 오브젝트 이름으로 세분화
    /// </summary>
    private string DetermineLayer(GameObject obj, string rootParentName)
    {
        // 1. 루트 부모 이름으로 기본 레이어 결정
        string baseLayer = GetBaseLayerFromRoot(rootParentName);
        
        // 2. 자식 오브젝트는 이름으로 세분화 가능
        string refinedLayer = RefineLayerByName(obj.name, baseLayer);
        
        return refinedLayer ?? baseLayer;
    }
    
    /// <summary>
    /// 루트 부모 이름으로 기본 레이어를 결정합니다.
    /// </summary>
    private string GetBaseLayerFromRoot(string rootName)
    {
        // DECO_Detail 체크
        foreach (var pattern in RootPatterns_Detail)
        {
            if (rootName.IndexOf(pattern, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return Layer_DECO_Detail;
        }
        
        // DECO_Medium 체크
        foreach (var pattern in RootPatterns_Medium)
        {
            if (rootName.IndexOf(pattern, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return Layer_DECO_Medium;
        }
        
        // DECO_Large 체크
        foreach (var pattern in RootPatterns_Large)
        {
            if (rootName.IndexOf(pattern, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return Layer_DECO_Large;
        }
        
        // ENV_Structure 체크 (기본값)
        foreach (var pattern in RootPatterns_Structure)
        {
            if (rootName.IndexOf(pattern, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return Layer_ENV_Structure;
        }
        
        // 매칭 없으면 Structure (안전하게)
        return Layer_ENV_Structure;
    }
    
    /// <summary>
    /// 오브젝트 이름으로 레이어를 세분화합니다.
    /// 상위 레이어에서 하위 레이어로만 이동 가능 (안전 장치)
    /// </summary>
    private string RefineLayerByName(string objectName, string baseLayer)
    {
        // Detail 키워드 체크
        foreach (var keyword in ChildKeywords_Detail)
        {
            if (objectName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // Detail은 어떤 레이어에서든 적용 가능
                return Layer_DECO_Detail;
            }
        }
        
        // Structure 키워드 체크 (구조물은 항상 Structure 유지)
        foreach (var keyword in ChildKeywords_Structure)
        {
            if (objectName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Layer_ENV_Structure;
            }
        }
        
        // Medium 키워드 체크 (Structure/Large에서만 적용)
        if (baseLayer == Layer_ENV_Structure || baseLayer == Layer_DECO_Large)
        {
            foreach (var keyword in ChildKeywords_Medium)
            {
                if (objectName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return Layer_DECO_Medium;
                }
            }
        }
        
        // Large 키워드 체크 (Structure에서만 적용)
        if (baseLayer == Layer_ENV_Structure)
        {
            foreach (var keyword in ChildKeywords_Large)
            {
                if (objectName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return Layer_DECO_Large;
                }
            }
        }
        
        // 세분화 없음 → 기본 레이어 유지
        return null;
    }

    private void DrawPreviewResults()
    {
        EditorGUILayout.LabelField("미리보기 결과", EditorStyles.boldLabel);
        
        // 통계 요약
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"ENV_Structure: {_layerCounts[Layer_ENV_Structure]}개 (컬링 없음)");
        EditorGUILayout.LabelField($"DECO_Large: {_layerCounts[Layer_DECO_Large]}개 ({CullDistance_Large}m)");
        EditorGUILayout.LabelField($"DECO_Medium: {_layerCounts[Layer_DECO_Medium]}개 ({CullDistance_Medium}m)");
        EditorGUILayout.LabelField($"DECO_Detail: {_layerCounts[Layer_DECO_Detail]}개 ({CullDistance_Detail}m)");
        EditorGUILayout.LabelField($"변경 없음: {_layerCounts["Unchanged"]}개");
        
        int total = _layerCounts.Values.Sum();
        int changes = total - _layerCounts["Unchanged"];
        EditorGUILayout.LabelField($"총 변경: {changes}개 / {total}개", EditorStyles.boldLabel);
        EditorGUILayout.EndVertical();
        
        if (_previewItems.Count > 0)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"변경 예정 (처음 {_previewItems.Count}개):");
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(150));
            
            foreach (var item in _previewItems)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                EditorGUILayout.LabelField(item.ObjectPath, GUILayout.Width(250));
                EditorGUILayout.LabelField($"{item.CurrentLayer} → {item.NewLayer}");
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }
    }
    
    private bool ShouldSkipObject(GameObject obj)
    {
        string name = obj.name.ToLower();
        
        if (obj.GetComponent<Camera>() != null) return true;
        if (name.Contains("directional light")) return true;
        if (name.Contains("event system")) return true;
        if (name.Contains("canvas")) return true;
        if (!obj.activeInHierarchy && obj.name == "NEON CITY") return true;
        
        return false;
    }
    
    private string GetHierarchyPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        int depth = 0;
        
        while (parent != null && depth < 2)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
            depth++;
        }
        
        if (parent != null)
            path = ".../" + path;
        
        return path;
    }

    private void CreateCameraCullingScript()
    {
        string scriptContent = @"using UnityEngine;

/// <summary>
/// 레이어별 거리 컬링을 적용하는 카메라 컴포넌트
/// 
/// 사용법: 메인 카메라에 이 스크립트를 추가하세요.
/// OptimizedLayerAssigner로 레이어 설정 후 사용합니다.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraLayerCulling : MonoBehaviour
{
    [Header(""레이어 컬링 거리"")]
    [Tooltip(""ENV_Structure (Layer 22) - 빌딩/도로"")]
    public float structureDistance = 0f; // 0 = 카메라 far clip plane 사용
    
    [Tooltip(""DECO_Large (Layer 21) - 차량/가로등"")]
    public float largeDistance = 150f;
    
    [Tooltip(""DECO_Medium (Layer 20) - 상자/파이프"")]
    public float mediumDistance = 80f;
    
    [Tooltip(""DECO_Detail (Layer 19) - 그래피티/데칼"")]
    public float detailDistance = 40f;
    
    [Header(""설정"")]
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
        
        Debug.Log($""[CameraLayerCulling] 적용됨 - Detail:{detailDistance}m, Medium:{mediumDistance}m, Large:{largeDistance}m"");
    }
    
    // Inspector에서 테스트용
    [ContextMenu(""Apply Culling Now"")]
    public void ApplyCullingManual()
    {
        ApplyCulling();
    }
}
";
        
        string path = "Assets/Scripts";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        string fullPath = path + "/CameraLayerCulling.cs";
        File.WriteAllText(fullPath, scriptContent);
        AssetDatabase.Refresh();
        
        Debug.Log($"✅ 카메라 컬링 스크립트 생성: {fullPath}");
        EditorUtility.DisplayDialog("완료", 
            $"카메라 컬링 스크립트가 생성되었습니다.\n\n" +
            $"경로: {fullPath}\n\n" +
            "메인 카메라에 이 스크립트를 추가하세요.", 
            "확인");
        
        var asset = AssetDatabase.LoadAssetAtPath<Object>(fullPath);
        if (asset != null)
        {
            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }
    }

    private void GenerateReport()
    {
        if (!_hasPreview)
        {
            EditorUtility.DisplayDialog("알림", "먼저 '미리보기'를 실행하세요.", "확인");
            return;
        }
        
        var sb = new StringBuilder();
        sb.AppendLine("레이어,오브젝트 수,컬링 거리");
        sb.AppendLine($"{Layer_ENV_Structure},{_layerCounts[Layer_ENV_Structure]},무한");
        sb.AppendLine($"{Layer_DECO_Large},{_layerCounts[Layer_DECO_Large]},{CullDistance_Large}m");
        sb.AppendLine($"{Layer_DECO_Medium},{_layerCounts[Layer_DECO_Medium]},{CullDistance_Medium}m");
        sb.AppendLine($"{Layer_DECO_Detail},{_layerCounts[Layer_DECO_Detail]},{CullDistance_Detail}m");
        sb.AppendLine($"변경 없음,{_layerCounts["Unchanged"]},-");
        sb.AppendLine();
        sb.AppendLine("=== 변경 상세 (처음 50개) ===");
        sb.AppendLine("경로,이전 레이어,새 레이어");
        
        foreach (var item in _previewItems)
        {
            sb.AppendLine($"{item.ObjectPath},{item.CurrentLayer},{item.NewLayer}");
        }
        
        string reportPath = "Assets/Editor/OptimizedLayerReport.csv";
        
        if (!Directory.Exists("Assets/Editor"))
            Directory.CreateDirectory("Assets/Editor");
        
        File.WriteAllText(reportPath, sb.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();
        
        int total = _layerCounts.Values.Sum();
        int changes = total - _layerCounts["Unchanged"];
        
        Debug.Log($"✅ 보고서 생성: {reportPath}");
        EditorUtility.DisplayDialog("보고서 생성 완료", 
            $"총 {total}개 오브젝트 분석\n" +
            $"변경 예정: {changes}개\n\n" +
            $"보고서: {reportPath}", 
            "확인");
        
        var reportAsset = AssetDatabase.LoadAssetAtPath<Object>(reportPath);
        if (reportAsset != null)
        {
            Selection.activeObject = reportAsset;
            EditorGUIUtility.PingObject(reportAsset);
        }
    }
}
