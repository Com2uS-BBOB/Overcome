using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;

/// <summary>
/// 프리팹 레이어 자동 할당 에디터 도구
/// 폴더 경로 기반으로 프리팹의 레이어를 일괄 설정하고 보고서를 생성합니다.
/// 
/// 레이어 체계:
/// - SmallProps (User Layer 20): 작은 소품 - 컬링 거리 50m
/// - MediumProps (User Layer 21): 중간 오브젝트 - 컬링 거리 100m  
/// - Buildings (User Layer 22): 건물/도로/지형 - 컬링 없음 (항상 표시)
/// 
/// 우선순위: SmallProps > MediumProps > Buildings (구체적인 것 먼저 매칭)
/// </summary>
public class PrefabLayerAssigner : EditorWindow
{
    // ===== 레이어 이름 (Project Settings에서 설정한 이름과 일치해야 함) =====
    private const string SmallPropsLayer = "SmallProps";     // User Layer 20
    private const string MediumPropsLayer = "MediumProps";   // User Layer 21
    private const string BuildingsLayer = "Buildings";       // User Layer 22
    
    // ===== SmallProps 분류 키워드 (컬링 거리 50m) =====
    // 가장 먼저 매칭됨 - 작은 장식 요소들
    private static readonly string[] SmallPropsKeywords = new string[]
    {
        "Deco",           // 장식 요소 (추가됨)
        "Detail Small", 
        "Grass", 
        "Plants", 
        "Props", 
        "Signs",
        "Trash", 
        "Posters", 
        "Graffiti", 
        "FX",
        "Particle", 
        "Debris", 
        "Litter"
    };
    
    // ===== MediumProps 분류 키워드 (컬링 거리 100m) =====
    // 두 번째로 매칭됨 - 중간 크기 오브젝트
    private static readonly string[] MediumPropsKeywords = new string[]
    {
        "Decals",         // 데칼 (추가됨)
        "Fenced",         // 펜스가 있는 오브젝트 (추가됨)
        "Detail Big", 
        "Trees", 
        "Vehicles", 
        "Cars", 
        "Chars",
        "Characters", 
        "Lights", 
        "Street Lights", 
        "Furniture",
        "Bench", 
        "Lamp"
    };
    
    // ===== Buildings 분류 키워드 (컬링 없음) =====
    // 마지막으로 매칭됨 - 큰 구조물
    private static readonly string[] BuildingsKeywords = new string[]
    {
        "Building", 
        "Street", 
        "Road", 
        "Platform", 
        "Terrain",
        "Ground", 
        "Wall", 
        "Floor", 
        "Base", 
        "Block", 
        "City",
        "Elevation", 
        "Window", 
        "Door", 
        "Gate", 
        "Arc",
        "Corner", 
        "Cornice", 
        "Column"
    };
    
    private Vector2 _scrollPosition;
    private string _targetPath = "Assets/_DLNK";
    private bool _includeChildren = true;
    private List<string> _previewResults = new List<string>();
    private bool _isPreviewMode = true;

    [MenuItem("Tools/Optimization/Prefab Layer Assigner")]
    public static void ShowWindow()
    {
        var window = GetWindow<PrefabLayerAssigner>("Prefab Layer Assigner");
        window.minSize = new Vector2(500, 400);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("프리팹 레이어 자동 할당 도구", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "폴더 경로를 기반으로 프리팹의 레이어를 자동 할당합니다.\n\n" +
            "• SmallProps (Layer 20): Deco, 풀, 식물, 소품 등 → 컬링 50m\n" +
            "• MediumProps (Layer 21): Decals, Fenced, 나무, 차량 등 → 컬링 100m\n" +
            "• Buildings (Layer 22): 건물, 도로, 지형 등 → 컬링 없음\n\n" +
            "우선순위: SmallProps > MediumProps > Buildings",
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
                "프리팹 레이어를 변경하고 보고서를 생성합니다.\n계속하시겠습니까?", 
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
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200));
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
        
        if (!ValidateLayers())
        {
            results.Add("⚠️ 오류: 레이어가 존재하지 않습니다!");
            results.Add("");
            results.Add("Project Settings → Tags and Layers에서 다음 레이어를 추가하세요:");
            results.Add("  • User Layer 20: SmallProps");
            results.Add("  • User Layer 21: MediumProps");
            results.Add("  • User Layer 22: Buildings");
            return results;
        }
        
        results.Add($"총 {prefabGuids.Length}개 프리팹 검색됨");
        results.Add("─────────────────────────────────");
        
        foreach (var guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string assignedLayer = DetermineLayer(path);
            string prefabName = Path.GetFileNameWithoutExtension(path);
            
            if (assignedLayer == "Default")
            {
                skippedCount++;
                continue;
            }
            
            if (!previewOnly)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    SetLayerRecursively(prefab, LayerMask.NameToLayer(assignedLayer));
                    EditorUtility.SetDirty(prefab);
                }
            }
            
            if (assignedLayer == SmallPropsLayer)
            {
                smallPropsCount++;
                results.Add($"[SmallProps] {prefabName}");
            }
            else if (assignedLayer == MediumPropsLayer)
            {
                mediumPropsCount++;
                results.Add($"[MediumProps] {prefabName}");
            }
            else if (assignedLayer == BuildingsLayer)
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
        results.Add($"SmallProps: {smallPropsCount}개 (컬링 50m)");
        results.Add($"MediumProps: {mediumPropsCount}개 (컬링 100m)");
        results.Add($"Buildings: {buildingsCount}개 (컬링 없음)");
        results.Add($"Default 유지: {skippedCount}개");
        results.Add($"총 처리: {prefabGuids.Length}개");
        
        return results;
    }

    /// <summary>
    /// 경로를 기반으로 적절한 레이어를 결정합니다.
    /// 우선순위: SmallProps > MediumProps > Buildings > Default
    /// (구체적인 키워드가 먼저 매칭되도록 순서 변경)
    /// </summary>
    private string DetermineLayer(string path)
    {
        // 1. SmallProps 키워드 확인 (가장 구체적 - Deco 등)
        foreach (var keyword in SmallPropsKeywords)
        {
            if (path.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return SmallPropsLayer;
        }
        
        // 2. MediumProps 키워드 확인 (Decals, Fenced 등)
        foreach (var keyword in MediumPropsKeywords)
        {
            if (path.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return MediumPropsLayer;
        }
        
        // 3. Buildings 키워드 확인 (가장 일반적)
        foreach (var keyword in BuildingsKeywords)
        {
            if (path.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return BuildingsLayer;
        }
        
        return "Default";
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        
        if (_includeChildren)
        {
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
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
            string layer = DetermineLayer(path);
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
        
        string summary = $"레이어 할당 완료!\n\n" +
                        $"• SmallProps: {smallCount}개\n" +
                        $"• MediumProps: {mediumCount}개\n" +
                        $"• Buildings: {buildingsCount}개\n\n" +
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
