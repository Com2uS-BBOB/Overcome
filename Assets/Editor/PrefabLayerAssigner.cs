using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// 프리팹 레이어 자동 할당 에디터 도구 v3
/// 
/// ★ v3 변경사항: 경로/이름 키워드 분리로 폴더 충돌 방지
/// 
/// 분류 전략:
/// - 루트 오브젝트: 경로 기반 → 대부분 Buildings로 분류
/// - 자식 오브젝트: 이름 기반 → 세부 분류 (SmallProps/MediumProps)
/// - 키워드 없으면: 부모 레이어 상속
/// 
/// 레이어 체계:
/// - SmallProps (Layer 20): 작은 소품 - 컬링 거리 50m
/// - MediumProps (Layer 21): 중간 오브젝트 - 컬링 거리 100m  
/// - Buildings (Layer 22): 건물/도로/지형 - 컬링 없음
/// </summary>
public class PrefabLayerAssigner : EditorWindow
{
    // ===== 레이어 이름 =====
    private const string SmallPropsLayer = "SmallProps";     // Layer 20
    private const string MediumPropsLayer = "MediumProps";   // Layer 21
    private const string BuildingsLayer = "Buildings";       // Layer 22
    
    // =====================================================================
    // 경로용 키워드 (루트 분류 - 폴더 경로 검사)
    // 대형 구조물 위주로 Buildings 분류
    // =====================================================================
    private static readonly string[] PathKeywords_Buildings = new string[]
    {
        // 도로/플랫폼
        "Street", "Platform", "Road", "Bridge",
        // 건물/벽
        "Building", "Wall", "Block", "Floor", "Ground", "Terrain",
        // 도시 요소
        "City", "Elevation", "Window", "Door", "Gate", "Arc",
        "Corner", "Cornice", "Column",
        // 폴더 접두사
        "ENV_"
    };
    
    // =====================================================================
    // 이름용 키워드 (자식 분류 - 오브젝트 이름 검사)
    // 언더스코어(_) 접두사 패턴으로 폴더 충돌 방지
    // =====================================================================
    
    // SmallProps: 작은 장식 (50m 컬링)
    private static readonly string[] NameKeywords_SmallProps = new string[]
    {
        // 언더스코어 접두사 (자식 전용 - 폴더 충돌 없음)
        "_Deco",
        "_Detail",
        "_Small",
        
        // 표지판/포스터 (폴더명은 "Signs", "Posters"로 다름)
        "Sign_", "Poster_",  // 접두사로 사용될 때
        "_Sign", "_Poster",  // 접미사로 사용될 때
        
        // 기타 소품 (폴더 충돌 없음)
        "Graffiti", "Grafitti",
        "FX", "Particle",
        "Debris", "Trash", "Litter",
        "ADS_", "_ADS"
    };
    
    // MediumProps: 중간 크기 (100m 컬링)
    private static readonly string[] NameKeywords_MediumProps = new string[]
    {
        // 언더스코어 접두사 (자식 전용)
        "_Lights",
        "_Light",
        "_Tree",
        "_Vehicle",
        
        // 가구/조명 (구체적 이름)
        "StreetLight", "StreetLamp",
        "Lamp_", "_Lamp",
        "Bench_", "_Bench",
        
        // 차량 (구체적 이름)
        "FlyingCar", "FlyingTrain",
        "Vehicle_", "Car_"
    };
    
    // Buildings: 자식 이름에서도 Buildings로 분류할 키워드
    private static readonly string[] NameKeywords_Buildings = new string[]
    {
        "_Wall", "_Floor", "_Ground",
        "_UnderWall", "_UperWall", "_UpperWall",
        "StreetRoad"  // StreetRoad00 등
    };
    
    private Vector2 _scrollPosition;
    private string _targetPath = "Assets/_DLNK";
    private bool _includeChildren = true;
    private bool _individualChildCheck = true;
    private List<string> _previewResults = new List<string>();
    private bool _isPreviewMode = true;
    
    // 통계
    private int _totalObjectsProcessed = 0;

    [MenuItem("Tools/Optimization/Prefab Layer Assigner")]
    public static void ShowWindow()
    {
        var window = GetWindow<PrefabLayerAssigner>("Prefab Layer Assigner");
        window.minSize = new Vector2(550, 500);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("프리팹 레이어 자동 할당 도구 v3", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "경로/이름 키워드 분리로 폴더 충돌을 방지합니다.\n\n" +
            "• 루트: 경로 기반 → 대부분 Buildings로 분류\n" +
            "• 자식: 이름 기반 → _Deco, _Lights 등 세부 분류\n" +
            "• 매칭 없으면 부모 레이어 상속\n\n" +
            "★ 폴더명 'Deco', 'Light' 충돌 문제 해결됨",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        // 대상 경로 설정
        EditorGUILayout.BeginHorizontal();
        _targetPath = EditorGUILayout.TextField("대상 폴더", _targetPath);
        if (GUILayout.Button("선택", GUILayout.Width(60)))
        {
            string selected = EditorUtility.OpenFolderPanel("프리팹 폴더 선택", "Assets", "");
            if (!string.IsNullOrEmpty(selected))
            {
                if (selected.StartsWith(Application.dataPath))
                {
                    _targetPath = "Assets" + selected.Substring(Application.dataPath.Length);
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        _includeChildren = EditorGUILayout.Toggle("자식 오브젝트 포함", _includeChildren);
        
        EditorGUI.BeginDisabledGroup(!_includeChildren);
        _individualChildCheck = EditorGUILayout.Toggle(
            new GUIContent("자식 개별 키워드 검사", 
                "체크: 각 자식 이름으로 개별 분류\n" +
                "해제: 모든 자식이 부모와 동일한 레이어"),
            _individualChildCheck);
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Space(10);
        DrawLayerStatus();
        EditorGUILayout.Space(10);
        
        // 버튼 영역
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("미리보기", GUILayout.Height(30)))
        {
            _isPreviewMode = true;
            _previewResults = ProcessPrefabs(true);
        }
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("적용 및 보고서 생성", GUILayout.Height(30)))
        {
            if (!ValidateLayers())
            {
                EditorUtility.DisplayDialog("오류", 
                    "필요한 레이어가 설정되지 않았습니다.\n" +
                    "Project Settings → Tags and Layers에서 레이어를 추가하세요.", 
                    "확인");
                return;
            }
            
            if (EditorUtility.DisplayDialog("확인", 
                "프리팹 레이어를 변경하고 보고서를 생성합니다.\n\n" +
                "v3: 경로/이름 키워드 분리 적용\n" +
                "- 루트: 경로 기반 (Buildings 위주)\n" +
                "- 자식: 이름 기반 (세부 분류)\n\n" +
                "계속하시겠습니까?", 
                "적용", "취소"))
            {
                _isPreviewMode = false;
                _previewResults = ProcessPrefabs(false);
                GenerateReport();
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // 결과 표시
        if (_previewResults.Count > 0)
        {
            string label = _isPreviewMode ? "미리보기 결과:" : "적용 결과:";
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(180));
            foreach (var result in _previewResults)
            {
                EditorGUILayout.LabelField(result, EditorStyles.wordWrappedLabel);
            }
            EditorGUILayout.EndScrollView();
        }
    }

    private void DrawLayerStatus()
    {
        EditorGUILayout.LabelField("레이어 상태:", EditorStyles.boldLabel);
        
        int smallLayer = LayerMask.NameToLayer(SmallPropsLayer);
        int mediumLayer = LayerMask.NameToLayer(MediumPropsLayer);
        int buildingsLayer = LayerMask.NameToLayer(BuildingsLayer);
        
        EditorGUILayout.BeginHorizontal();
        DrawLayerStatusIcon(SmallPropsLayer, smallLayer != -1);
        DrawLayerStatusIcon(MediumPropsLayer, mediumLayer != -1);
        DrawLayerStatusIcon(BuildingsLayer, buildingsLayer != -1);
        EditorGUILayout.EndHorizontal();
    }
    
    private void DrawLayerStatusIcon(string layerName, bool exists)
    {
        string status = exists ? "✓" : "✗";
        GUIStyle style = new GUIStyle(EditorStyles.label);
        style.normal.textColor = exists ? Color.green : Color.red;
        EditorGUILayout.LabelField($"{status} {layerName}", style, GUILayout.Width(150));
    }

    private bool ValidateLayers()
    {
        return LayerMask.NameToLayer(SmallPropsLayer) != -1 &&
               LayerMask.NameToLayer(MediumPropsLayer) != -1 &&
               LayerMask.NameToLayer(BuildingsLayer) != -1;
    }

    private List<string> ProcessPrefabs(bool previewOnly)
    {
        var results = new List<string>();
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { _targetPath });
        
        int smallPropsCount = 0;
        int mediumPropsCount = 0;
        int buildingsCount = 0;
        int skippedCount = 0;
        _totalObjectsProcessed = 0;
        
        if (!ValidateLayers())
        {
            results.Add("⚠️ 오류: 레이어가 존재하지 않습니다!");
            return results;
        }
        
        results.Add($"총 {prefabGuids.Length}개 프리팹 검색됨");
        results.Add($"v3: 경로/이름 키워드 분리 모드");
        results.Add("─────────────────────────────────");
        
        foreach (var guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string rootLayer = DetermineLayerByPath(path);
            string prefabName = Path.GetFileNameWithoutExtension(path);
            
            // 경로에서 Buildings 키워드 매칭 안되면 Default 유지
            if (rootLayer == "Default")
            {
                skippedCount++;
                continue;
            }
            
            if (!previewOnly)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    string prefabPath = AssetDatabase.GetAssetPath(prefab);
                    GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                    
                    if (prefabRoot != null)
                    {
                        int rootLayerIndex = LayerMask.NameToLayer(rootLayer);
                        
                        if (_includeChildren && _individualChildCheck)
                        {
                            // v3: 자식은 이름 기반 분류
                            SetLayerWithNameBasedCheck(prefabRoot, rootLayerIndex, rootLayer);
                        }
                        else if (_includeChildren)
                        {
                            SetLayerRecursively(prefabRoot, rootLayerIndex);
                        }
                        else
                        {
                            prefabRoot.layer = rootLayerIndex;
                            _totalObjectsProcessed++;
                        }
                        
                        PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                        PrefabUtility.UnloadPrefabContents(prefabRoot);
                    }
                }
            }
            
            // 통계 (루트 기준)
            if (rootLayer == SmallPropsLayer)
            {
                smallPropsCount++;
                results.Add($"[SmallProps] {prefabName}");
            }
            else if (rootLayer == MediumPropsLayer)
            {
                mediumPropsCount++;
                results.Add($"[MediumProps] {prefabName}");
            }
            else if (rootLayer == BuildingsLayer)
            {
                buildingsCount++;
                results.Add($"[Buildings] {prefabName}");
            }
        }
        
        if (!previewOnly)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        results.Add("─────────────────────────────────");
        results.Add($"Buildings: {buildingsCount}개 프리팹 (컬링 없음)");
        results.Add($"MediumProps: {mediumPropsCount}개 프리팹 (컬링 100m)");
        results.Add($"SmallProps: {smallPropsCount}개 프리팹 (컬링 50m)");
        results.Add($"Default 유지: {skippedCount}개 프리팹");
        results.Add($"총 프리팹: {prefabGuids.Length}개");
        
        if (!previewOnly)
        {
            results.Add($"총 처리된 오브젝트: {_totalObjectsProcessed}개 (자식 포함)");
        }
        
        return results;
    }

    /// <summary>
    /// 경로 기반으로 루트 레이어를 결정합니다.
    /// Buildings 키워드만 검사하여 대형 구조물 보호
    /// </summary>
    private string DetermineLayerByPath(string path)
    {
        // Buildings 키워드 검사 (대형 구조물)
        foreach (var keyword in PathKeywords_Buildings)
        {
            if (path.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return BuildingsLayer;
        }
        
        // 매칭 없으면 Default (변경 안함)
        return "Default";
    }
    
    /// <summary>
    /// 이름 기반으로 자식 레이어를 결정합니다.
    /// 언더스코어 접두사 패턴으로 폴더 충돌 방지
    /// </summary>
    private string DetermineLayerByName(string objectName)
    {
        // 1. SmallProps 키워드 확인 (_Deco 등)
        foreach (var keyword in NameKeywords_SmallProps)
        {
            if (objectName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return SmallPropsLayer;
        }
        
        // 2. MediumProps 키워드 확인 (_Lights 등)
        foreach (var keyword in NameKeywords_MediumProps)
        {
            if (objectName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return MediumPropsLayer;
        }
        
        // 3. Buildings 키워드 확인 (_Wall, StreetRoad 등)
        foreach (var keyword in NameKeywords_Buildings)
        {
            if (objectName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return BuildingsLayer;
        }
        
        // 매칭 없음 → 부모 레이어 상속
        return null;
    }

    /// <summary>
    /// v3: 자식 오브젝트를 이름 기반으로 분류합니다.
    /// </summary>
    private void SetLayerWithNameBasedCheck(GameObject obj, int parentLayerIndex, string parentLayerName)
    {
        // 루트는 경로 기반으로 이미 결정됨
        // 자식은 이름 기반으로 분류
        string myLayerName = DetermineLayerByName(obj.name);
        int myLayerIndex;
        
        if (!string.IsNullOrEmpty(myLayerName))
        {
            // 키워드 매칭됨 → 해당 레이어 사용
            myLayerIndex = LayerMask.NameToLayer(myLayerName);
        }
        else
        {
            // 키워드 매칭 안됨 → 부모 레이어 상속
            myLayerIndex = parentLayerIndex;
            myLayerName = parentLayerName;
        }
        
        obj.layer = myLayerIndex;
        _totalObjectsProcessed++;
        
        // 자식들도 재귀적으로 처리
        foreach (Transform child in obj.transform)
        {
            SetLayerWithNameBasedCheck(child.gameObject, myLayerIndex, myLayerName);
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        _totalObjectsProcessed++;
        
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    private void GenerateReport()
    {
        var sb = new StringBuilder();
        sb.AppendLine("프리팹명,할당된 레이어,컬링 거리,경로");
        
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { _targetPath });
        int smallCount = 0, mediumCount = 0, buildingsCount = 0;
        
        foreach (var guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string layer = DetermineLayerByPath(path);
            string prefabName = Path.GetFileNameWithoutExtension(path);
            
            if (layer == "Default") continue;
            
            string cullDistance = layer switch
            {
                SmallPropsLayer => "50m",
                MediumPropsLayer => "100m",
                BuildingsLayer => "무한",
                _ => "-"
            };
            
            sb.AppendLine($"{prefabName},{layer},{cullDistance},{path}");
            
            if (layer == SmallPropsLayer) smallCount++;
            else if (layer == MediumPropsLayer) mediumCount++;
            else if (layer == BuildingsLayer) buildingsCount++;
        }
        
        string reportPath = "Assets/Editor/LayerAssignmentReport.csv";
        
        if (!Directory.Exists("Assets/Editor"))
            Directory.CreateDirectory("Assets/Editor");
        
        File.WriteAllText(reportPath, sb.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();
        
        string summary = $"레이어 할당 완료! (v3)\n\n" +
                        $"• Buildings: {buildingsCount}개 프리팹\n" +
                        $"• MediumProps: {mediumCount}개 프리팹\n" +
                        $"• SmallProps: {smallCount}개 프리팹\n" +
                        $"• 총 오브젝트: {_totalObjectsProcessed}개 (자식 포함)\n\n" +
                        $"보고서: {reportPath}";
        
        Debug.Log($"✅ {summary}");
        EditorUtility.DisplayDialog("완료", summary, "확인");
        
        var reportAsset = AssetDatabase.LoadAssetAtPath<Object>(reportPath);
        if (reportAsset != null)
        {
            Selection.activeObject = reportAsset;
            EditorGUIUtility.PingObject(reportAsset);
        }
    }
}
