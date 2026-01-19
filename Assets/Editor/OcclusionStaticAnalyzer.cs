using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Occlusion Culling Static 플래그 분석 및 최적화 도구
/// 
/// DLNK Neon City 에셋 공식 가이드 기반:
/// "Using Occlusion Culling is recommended for better performance 
///  since a complex environment could use hundreds of prefabs."
/// 
/// 권장 설정:
/// - 큰 빌딩/벽/지형 → Occluder + Occludee (가리고 + 가려짐)
/// - 중간 소품 → Occludee만 (가려지기만 함)
/// - 작은 디테일 → Static 해제 또는 Occludee만
/// - 투명 오브젝트 → Static 해제 권장
/// </summary>
public class OcclusionStaticAnalyzer : EditorWindow
{
    private Vector2 _scrollPosition;
    private List<AnalysisResult> _results = new List<AnalysisResult>();
    private bool _hasAnalysis = false;
    
    // 통계
    private int _totalObjects = 0;
    private int _occluderCount = 0;
    private int _occludeeOnlyCount = 0;
    private int _noStaticCount = 0;
    private int _bothCount = 0;
    
    // 문제 감지
    private List<string> _issues = new List<string>();
    private List<string> _recommendations = new List<string>();
    
    private struct AnalysisResult
    {
        public string Path;
        public string ObjectName;
        public bool IsOccluder;
        public bool IsOccludee;
        public bool HasRenderer;
        public float BoundsSize;
        public string RecommendedSetting;
        public bool NeedsChange;
    }
    
    // ===== 분류 기준 (크기/이름 기반) =====
    
    // 큰 구조물 (Occluder + Occludee)
    private static readonly string[] LargeStructureKeywords = new string[]
    {
        "Building", "Wall", "Floor", "Platform", "Street", "Road",
        "Bridge", "Ground", "Terrain", "Block", "Gate", "Pillar"
    };
    
    // 중간 크기 (Occludee만)
    private static readonly string[] MediumPropKeywords = new string[]
    {
        "Car", "Vehicle", "Tank", "Light", "Lamp", "Tree",
        "BusStop", "Sign", "Pole", "Cable"
    };
    
    // 작은/투명 (Static 해제 권장)
    private static readonly string[] SmallOrTransparentKeywords = new string[]
    {
        "Decal", "Graffiti", "Grafitti", "FX", "Particle",
        "Glass", "Window", "Transparent", "Alpha"
    };
    
    [MenuItem("Tools/Optimization/Occlusion Static Analyzer")]
    public static void ShowWindow()
    {
        var window = GetWindow<OcclusionStaticAnalyzer>("Occlusion Analyzer");
        window.minSize = new Vector2(650, 500);
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("🔍 Occlusion Static 분석 도구", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Occluder/Occludee Static 플래그가 적절히 설정되었는지 분석합니다.\n\n" +
            "권장 설정:\n" +
            "• 큰 빌딩/벽 → Occluder + Occludee (다른 것을 가리고, 자신도 가려짐)\n" +
            "• 중간 소품 → Occludee만 (가려지기만 함)\n" +
            "• 작은/투명 → Static 해제 (오버헤드 방지)",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        // 버튼
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("1. 현재 상태 분석", GUILayout.Height(30)))
        {
            AnalyzeScene();
        }
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("2. 자동 최적화 적용", GUILayout.Height(30)))
        {
            if (!_hasAnalysis)
            {
                EditorUtility.DisplayDialog("알림", "먼저 '현재 상태 분석'을 실행하세요.", "확인");
                return;
            }
            
            int issueCount = _results.Count(r => r.NeedsChange);
            if (issueCount == 0)
            {
                EditorUtility.DisplayDialog("완료", "수정이 필요한 오브젝트가 없습니다!", "확인");
                return;
            }
            
            if (EditorUtility.DisplayDialog("확인",
                $"{issueCount}개 오브젝트의 Static 플래그를 최적화합니다.\n\n계속하시겠습니까?",
                "적용", "취소"))
            {
                ApplyOptimization();
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        if (_hasAnalysis)
        {
            DrawAnalysisResults();
        }
    }

    private void AnalyzeScene()
    {
        _results.Clear();
        _issues.Clear();
        _recommendations.Clear();
        
        _totalObjects = 0;
        _occluderCount = 0;
        _occludeeOnlyCount = 0;
        _noStaticCount = 0;
        _bothCount = 0;
        
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager
            .GetActiveScene().GetRootGameObjects();
        
        foreach (var root in rootObjects)
        {
            if (ShouldSkipObject(root)) continue;
            AnalyzeObjectRecursively(root, root.name);
        }
        
        // 문제점 분석
        AnalyzeIssues();
        
        _hasAnalysis = true;
        
        Debug.Log($"[OcclusionAnalyzer] 분석 완료: " +
                  $"총 {_totalObjects}개, Occluder+Occludee={_bothCount}, " +
                  $"Occludee만={_occludeeOnlyCount}, Static없음={_noStaticCount}");
    }
    
    private void AnalyzeObjectRecursively(GameObject obj, string rootName)
    {
        // 렌더러가 있는 오브젝트만 분석 (실제로 그려지는 것)
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            _totalObjects++;
            
            StaticEditorFlags flags = GameObjectUtility.GetStaticEditorFlags(obj);
            bool isOccluder = (flags & StaticEditorFlags.OccluderStatic) != 0;
            bool isOccludee = (flags & StaticEditorFlags.OccludeeStatic) != 0;
            
            // 통계
            if (isOccluder && isOccludee) _bothCount++;
            else if (isOccludee && !isOccluder) _occludeeOnlyCount++;
            else if (!isOccluder && !isOccludee) _noStaticCount++;
            if (isOccluder) _occluderCount++;
            
            // 권장 설정 판단
            float boundsSize = renderer.bounds.size.magnitude;
            string recommended = DetermineRecommendedSetting(obj.name, rootName, boundsSize);
            
            bool needsChange = false;
            if (recommended == "Occluder+Occludee" && (!isOccluder || !isOccludee))
                needsChange = true;
            else if (recommended == "Occludee만" && (isOccluder || !isOccludee))
                needsChange = true;
            else if (recommended == "Static 해제" && (isOccluder || isOccludee))
                needsChange = true;
            
            _results.Add(new AnalysisResult
            {
                Path = GetHierarchyPath(obj),
                ObjectName = obj.name,
                IsOccluder = isOccluder,
                IsOccludee = isOccludee,
                HasRenderer = true,
                BoundsSize = boundsSize,
                RecommendedSetting = recommended,
                NeedsChange = needsChange
            });
        }
        
        foreach (Transform child in obj.transform)
        {
            AnalyzeObjectRecursively(child.gameObject, rootName);
        }
    }
    
    private string DetermineRecommendedSetting(string objectName, string rootName, float boundsSize)
    {
        // 1. 작은/투명 오브젝트 체크
        foreach (var keyword in SmallOrTransparentKeywords)
        {
            if (objectName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return "Static 해제";
        }
        
        // 2. 루트 이름 기반 분류
        bool isStructureRoot = rootName.StartsWith("ENV_");
        bool isDecoRoot = rootName.StartsWith("DECO_");
        
        // 3. 큰 구조물 체크 (Occluder + Occludee)
        foreach (var keyword in LargeStructureKeywords)
        {
            if (objectName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // 크기도 확인 (5m 이상이면 확실히 큰 구조물)
                if (boundsSize > 5f)
                    return "Occluder+Occludee";
            }
        }
        
        // 4. ENV_ 루트의 자식은 기본적으로 Occluder+Occludee
        if (isStructureRoot && boundsSize > 3f)
            return "Occluder+Occludee";
        
        // 5. 중간 크기 소품
        foreach (var keyword in MediumPropKeywords)
        {
            if (objectName.IndexOf(keyword, System.StringComparison.OrdinalIgnoreCase) >= 0)
                return "Occludee만";
        }
        
        // 6. DECO_ 루트의 자식은 기본적으로 Occludee만
        if (isDecoRoot)
            return "Occludee만";
        
        // 7. 크기 기반 판단
        if (boundsSize > 10f)
            return "Occluder+Occludee";
        else if (boundsSize > 1f)
            return "Occludee만";
        else
            return "Occludee만"; // 작은 것도 일단 Occludee
    }

    private void AnalyzeIssues()
    {
        int needsChangeCount = _results.Count(r => r.NeedsChange);
        
        // 문제점 감지
        if (_occluderCount == 0)
        {
            _issues.Add("⚠️ Occluder Static이 설정된 오브젝트가 없습니다!");
            _recommendations.Add("→ 큰 빌딩/벽에 Occluder Static을 설정하세요.");
        }
        
        // 모든 오브젝트가 Occluder인 경우 (과도한 설정)
        float occluderRatio = (float)_occluderCount / _totalObjects;
        if (occluderRatio > 0.8f && _totalObjects > 100)
        {
            _issues.Add("⚠️ 대부분의 오브젝트가 Occluder로 설정되어 있습니다.");
            _recommendations.Add("→ 작은 소품은 Occludee만 설정하는 것이 효율적입니다.");
        }
        
        // 매우 작은 오브젝트가 Occluder인 경우
        int smallOccluders = _results.Count(r => r.IsOccluder && r.BoundsSize < 1f);
        if (smallOccluders > 10)
        {
            _issues.Add($"⚠️ 1m 미만의 작은 오브젝트 {smallOccluders}개가 Occluder로 설정됨");
            _recommendations.Add("→ 작은 오브젝트는 다른 것을 가리지 못하므로 Occludee만 권장");
        }
        
        if (needsChangeCount > 0)
        {
            _issues.Add($"📋 {needsChangeCount}개 오브젝트의 설정 변경 권장");
        }
        
        if (_issues.Count == 0)
        {
            _recommendations.Add("✅ Occlusion Static 설정이 적절합니다!");
        }
    }
    
    private void DrawAnalysisResults()
    {
        // 통계 요약
        EditorGUILayout.LabelField("📊 분석 결과", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"총 렌더러 오브젝트: {_totalObjects}개");
        EditorGUILayout.LabelField($"Occluder + Occludee: {_bothCount}개 (큰 구조물)");
        EditorGUILayout.LabelField($"Occludee만: {_occludeeOnlyCount}개 (중간 소품)");
        EditorGUILayout.LabelField($"Static 없음: {_noStaticCount}개");
        
        int needsChange = _results.Count(r => r.NeedsChange);
        if (needsChange > 0)
        {
            GUI.color = Color.yellow;
            EditorGUILayout.LabelField($"⚠️ 변경 권장: {needsChange}개", EditorStyles.boldLabel);
            GUI.color = Color.white;
        }
        else
        {
            GUI.color = Color.green;
            EditorGUILayout.LabelField("✅ 모든 설정이 적절합니다!", EditorStyles.boldLabel);
            GUI.color = Color.white;
        }
        EditorGUILayout.EndVertical();
        
        // 문제점/권장사항
        if (_issues.Count > 0 || _recommendations.Count > 0)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("💡 분석 결과", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            foreach (var issue in _issues)
            {
                EditorGUILayout.LabelField(issue);
            }
            foreach (var rec in _recommendations)
            {
                EditorGUILayout.LabelField(rec);
            }
            EditorGUILayout.EndVertical();
        }
        
        // 변경 필요한 오브젝트 목록
        var needsChangeList = _results.Where(r => r.NeedsChange).Take(30).ToList();
        if (needsChangeList.Count > 0)
        {
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField($"🔧 변경 권장 오브젝트 (처음 30개)", EditorStyles.boldLabel);
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(180));
            
            foreach (var item in needsChangeList)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                
                // 현재 상태
                string current = "";
                if (item.IsOccluder && item.IsOccludee) current = "[O+E]";
                else if (item.IsOccludee) current = "[E]";
                else if (item.IsOccluder) current = "[O]";
                else current = "[-]";
                
                EditorGUILayout.LabelField(current, GUILayout.Width(40));
                EditorGUILayout.LabelField(item.ObjectName, GUILayout.Width(200));
                EditorGUILayout.LabelField($"→ {item.RecommendedSetting}", GUILayout.Width(120));
                EditorGUILayout.LabelField($"({item.BoundsSize:F1}m)", GUILayout.Width(60));
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.LabelField("범례: [O]=Occluder, [E]=Occludee, [-]=없음", EditorStyles.miniLabel);
        }
    }

    private void ApplyOptimization()
    {
        int changedCount = 0;
        
        Undo.SetCurrentGroupName("Optimize Occlusion Static Flags");
        int undoGroup = Undo.GetCurrentGroup();
        
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager
            .GetActiveScene().GetRootGameObjects();
        
        foreach (var root in rootObjects)
        {
            if (ShouldSkipObject(root)) continue;
            changedCount += ApplyOptimizationRecursively(root, root.name);
        }
        
        Undo.CollapseUndoOperations(undoGroup);
        
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        
        Debug.Log($"✅ [OcclusionAnalyzer] 최적화 완료: {changedCount}개 오브젝트 변경됨");
        EditorUtility.DisplayDialog("완료",
            $"{changedCount}개 오브젝트의 Static 플래그가 최적화되었습니다.\n\n" +
            "⚠️ Occlusion Culling 재베이크 필요!\n" +
            "Window → Rendering → Occlusion Culling → Bake",
            "확인");
        
        // 다시 분석
        AnalyzeScene();
    }
    
    private int ApplyOptimizationRecursively(GameObject obj, string rootName)
    {
        int changed = 0;
        
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            float boundsSize = renderer.bounds.size.magnitude;
            string recommended = DetermineRecommendedSetting(obj.name, rootName, boundsSize);
            
            StaticEditorFlags currentFlags = GameObjectUtility.GetStaticEditorFlags(obj);
            StaticEditorFlags newFlags = currentFlags;
            
            // 권장 설정에 따라 플래그 조정
            if (recommended == "Occluder+Occludee")
            {
                newFlags |= StaticEditorFlags.OccluderStatic;
                newFlags |= StaticEditorFlags.OccludeeStatic;
            }
            else if (recommended == "Occludee만")
            {
                newFlags &= ~StaticEditorFlags.OccluderStatic;
                newFlags |= StaticEditorFlags.OccludeeStatic;
            }
            else if (recommended == "Static 해제")
            {
                newFlags &= ~StaticEditorFlags.OccluderStatic;
                newFlags &= ~StaticEditorFlags.OccludeeStatic;
            }
            
            if (newFlags != currentFlags)
            {
                Undo.RecordObject(obj, "Change Static Flags");
                GameObjectUtility.SetStaticEditorFlags(obj, newFlags);
                changed++;
            }
        }
        
        foreach (Transform child in obj.transform)
        {
            changed += ApplyOptimizationRecursively(child.gameObject, rootName);
        }
        
        return changed;
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
}
