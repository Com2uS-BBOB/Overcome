using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

/// <summary>
/// 프리팹 레이어 자동 할당 에디터 도구
/// 폴더 경로 기반으로 프리팹의 레이어를 일괄 설정하고 보고서를 생성합니다.
/// </summary>
public class PrefabLayerAssigner : EditorWindow
{
    // ===== 레이어 이름 설정 (Project Settings에서 설정한 이름과 일치해야 함) =====
    private const string SmallPropsLayer = "SmallProps";
    private const string MediumPropsLayer = "MediumProps";
    
    // ===== 폴더 경로 패턴 → 레이어 매핑 규칙 =====
    // 키워드가 경로에 포함되면 해당 레이어로 분류
    private static readonly Dictionary<string, string> FolderToLayerRules = new Dictionary<string, string>
    {
        // SmallProps (작은 소품) - 컬링 거리 50m
        { "Detail Small", SmallPropsLayer },
        { "Grass", SmallPropsLayer },
        { "Plants", SmallPropsLayer },
        { "Props", SmallPropsLayer },
        { "Signs", SmallPropsLayer },
        { "Decals", SmallPropsLayer },
        { "Trash", SmallPropsLayer },
        { "Posters", SmallPropsLayer },
        { "Graffiti", SmallPropsLayer },
        { "FX", SmallPropsLayer },
        { "Particle", SmallPropsLayer },
        
        // MediumProps (중간 크기) - 컬링 거리 100m
        { "Detail Big", MediumPropsLayer },
        { "Trees", MediumPropsLayer },
        { "Vehicles", MediumPropsLayer },
        { "Cars", MediumPropsLayer },
        { "Chars", MediumPropsLayer },
        { "Characters", MediumPropsLayer },
        { "Lights", MediumPropsLayer },
        { "Street Lights", MediumPropsLayer },
        { "Furniture", MediumPropsLayer },
    };
    
    // ===== 제외 패턴 (이 키워드가 포함되면 Default 유지) =====
    private static readonly string[] ExcludePatterns = new string[]
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
        "Block"
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
            "폴더 경로를 기반으로 프리팹의 레이어를 자동 할당합니다.\n" +
            "- SmallProps: 풀, 식물, 소품, 간판, 데칼 등 (컬링 50m)\n" +
            "- MediumProps: 나무, 차량, 가로등 등 (컬링 100m)\n" +
            "- Default: 건물, 도로, 지형 등 (컬링 없음)",
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
                // Assets 상대 경로로 변환
                if (selected.StartsWith(Application.dataPath))
                {
                    _targetPath = "Assets" + selected.Substring(Application.dataPath.Length);
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        _includeChildren = EditorGUILayout.Toggle("자식 오브젝트 포함", _includeChildren);
        
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
            if (EditorUtility.DisplayDialog("확인", 
                "프리팹 레이어를 변경하고 보고서를 생성합니다.\n계속하시겠습니까?", 
                "적용", "취소"))
            {
                _isPreviewMode = false;
                _previewResults = ProcessPrefabs(false);
                GenerateReport(_previewResults);
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // 결과 표시
        if (_previewResults.Count > 0)
        {
            EditorGUILayout.LabelField(_isPreviewMode ? "미리보기 결과:" : "적용 결과:", EditorStyles.boldLabel);
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(250));
            foreach (var result in _previewResults)
            {
                EditorGUILayout.LabelField(result, EditorStyles.wordWrappedLabel);
            }
            EditorGUILayout.EndScrollView();
        }
    }

    /// <summary>
    /// 프리팹을 처리하고 레이어를 할당합니다.
    /// </summary>
    /// <param name="previewOnly">true면 미리보기만, false면 실제 적용</param>
    /// <returns>처리 결과 로그 목록</returns>
    private List<string> ProcessPrefabs(bool previewOnly)
    {
        var results = new List<string>();
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { _targetPath });
        
        int smallPropsCount = 0;
        int mediumPropsCount = 0;
        int skippedCount = 0;
        
        // 레이어 존재 확인
        int smallLayer = LayerMask.NameToLayer(SmallPropsLayer);
        int mediumLayer = LayerMask.NameToLayer(MediumPropsLayer);
        
        if (smallLayer == -1 || mediumLayer == -1)
        {
            results.Add("⚠️ 오류: 레이어가 존재하지 않습니다!");
            results.Add($"   SmallProps 레이어: {(smallLayer == -1 ? "없음 ❌" : "있음 ✓")}");
            results.Add($"   MediumProps 레이어: {(mediumLayer == -1 ? "없음 ❌" : "있음 ✓")}");
            results.Add("");
            results.Add("Project Settings → Tags and Layers에서 레이어를 먼저 추가하세요.");
            return results;
        }
        
        results.Add($"총 {prefabGuids.Length}개 프리팹 검색됨");
        results.Add("─────────────────────────────────");
        
        foreach (var guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string assignedLayer = DetermineLayer(path);
            
            if (assignedLayer == "Default")
            {
                skippedCount++;
                continue;
            }
            
            string prefabName = Path.GetFileNameWithoutExtension(path);
            
            if (!previewOnly)
            {
                // 실제 레이어 변경
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
        }
        
        if (!previewOnly)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        results.Add("─────────────────────────────────");
        results.Add($"SmallProps: {smallPropsCount}개");
        results.Add($"MediumProps: {mediumPropsCount}개");
        results.Add($"Default 유지: {skippedCount}개");
        results.Add($"총 처리: {prefabGuids.Length}개");
        
        return results;
    }

    /// <summary>
    /// 경로를 기반으로 적절한 레이어를 결정합니다.
    /// </summary>
    private string DetermineLayer(string path)
    {
        // 제외 패턴 먼저 확인 (건물, 도로 등은 Default 유지)
        foreach (var exclude in ExcludePatterns)
        {
            if (path.IndexOf(exclude, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Default";
            }
        }
        
        // 폴더 규칙 확인
        foreach (var rule in FolderToLayerRules)
        {
            if (path.IndexOf(rule.Key, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return rule.Value;
            }
        }
        
        return "Default";
    }

    /// <summary>
    /// 오브젝트와 모든 자식의 레이어를 재귀적으로 설정합니다.
    /// </summary>
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

    /// <summary>
    /// CSV 보고서를 생성합니다.
    /// </summary>
    private void GenerateReport(List<string> results)
    {
        var sb = new StringBuilder();
        sb.AppendLine("프리팹명,할당된 레이어,경로");
        
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { _targetPath });
        
        foreach (var guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string layer = DetermineLayer(path);
            string prefabName = Path.GetFileNameWithoutExtension(path);
            
            if (layer != "Default")
            {
                sb.AppendLine($"{prefabName},{layer},{path}");
            }
        }
        
        // 보고서 저장
        string reportPath = "Assets/Editor/LayerAssignmentReport.csv";
        File.WriteAllText(reportPath, sb.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();
        
        Debug.Log($"✅ 보고서가 생성되었습니다: {reportPath}");
        EditorUtility.DisplayDialog("완료", 
            $"레이어 할당이 완료되었습니다.\n보고서: {reportPath}", "확인");
        
        // 보고서 파일 선택
        var reportAsset = AssetDatabase.LoadAssetAtPath<Object>(reportPath);
        if (reportAsset != null)
        {
            Selection.activeObject = reportAsset;
            EditorGUIUtility.PingObject(reportAsset);
        }
    }
}
