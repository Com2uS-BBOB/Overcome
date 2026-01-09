using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 프리팹의 레이어 설정을 씬 인스턴스에 동기화하는 에디터 도구입니다.
/// 
/// 핵심: 프리팹 소스의 레이어를 읽어와서 씬 인스턴스에 그대로 적용
/// (키워드 분류 없음 - PrefabLayerAssigner에서 설정한 레이어를 그대로 사용)
/// 
/// 사용 순서:
/// 1. PrefabLayerAssigner로 프리팹 레이어 설정
/// 2. SceneLayerAssigner로 씬 인스턴스에 동기화
/// 
/// 사용법: Tools → Optimization → Scene Layer Assigner
/// </summary>
public class SceneLayerAssigner : EditorWindow
{
    // 스크롤 위치
    private Vector2 _scrollPosition;
    
    // 미리보기 결과
    private List<PreviewResult> _previewResults = new List<PreviewResult>();
    private bool _hasPreview = false;
    
    // 통계
    private int _prefabInstanceCount = 0;
    private int _syncedObjectCount = 0;
    private int _changedObjectCount = 0;
    private int _nonPrefabCount = 0;
    
    private struct PreviewResult
    {
        public string Name;
        public string PrefabName;
        public string CurrentLayer;
        public string PrefabLayer;
        public bool WillChange;
        public int ChildSyncCount;
    }
    
    [MenuItem("Tools/Optimization/Scene Layer Assigner")]
    public static void ShowWindow()
    {
        var window = GetWindow<SceneLayerAssigner>("Scene Layer Assigner");
        window.minSize = new Vector2(550, 450);
    }
    
    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("씬 레이어 동기화 도구", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "프리팹 소스의 레이어 설정을 씬 인스턴스에 동기화합니다.\n\n" +
            "★ 키워드 분류 없음 - PrefabLayerAssigner에서 설정한 레이어를 그대로 사용\n\n" +
            "사용 순서:\n" +
            "1. PrefabLayerAssigner로 프리팹 레이어 설정\n" +
            "2. 이 도구로 씬 인스턴스에 동기화",
            MessageType.Info);
        
        EditorGUILayout.Space(10);
        
        // 버튼
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("미리보기 (Preview)", GUILayout.Height(30)))
        {
            GeneratePreview();
        }
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("동기화 적용 (Sync)", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("레이어 동기화 확인",
                "프리팹의 레이어 설정을 씬 인스턴스에 동기화합니다.\n\n" +
                "※ PrefabLayerAssigner를 먼저 실행했는지 확인하세요.\n\n계속하시겠습니까?",
                "동기화", "취소"))
            {
                ApplySync();
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        // 미리보기 결과 표시
        if (_hasPreview)
        {
            DrawPreviewResults();
        }
    }
    
    private void GeneratePreview()
    {
        _previewResults.Clear();
        _prefabInstanceCount = 0;
        _syncedObjectCount = 0;
        _changedObjectCount = 0;
        _nonPrefabCount = 0;
        
        // 씬의 모든 루트 오브젝트 가져오기
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager
            .GetActiveScene().GetRootGameObjects();
        
        foreach (var root in rootObjects)
        {
            // 시스템 오브젝트 제외
            if (ShouldSkipObject(root))
            {
                continue;
            }
            
            // 재귀적으로 모든 프리팹 인스턴스 검사
            PreviewObjectRecursively(root);
        }
        
        _hasPreview = true;
        Debug.Log($"[SceneLayerAssigner] 미리보기 완료: " +
                  $"프리팹 인스턴스={_prefabInstanceCount}, " +
                  $"동기화 대상={_syncedObjectCount}, " +
                  $"변경 예정={_changedObjectCount}, " +
                  $"비프리팹={_nonPrefabCount}");
    }
    
    private void PreviewObjectRecursively(GameObject obj)
    {
        // 이 오브젝트가 프리팹 인스턴스의 루트인지 확인
        GameObject prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(obj);
        bool isPrefabRoot = PrefabUtility.IsAnyPrefabInstanceRoot(obj);
        
        if (isPrefabRoot && prefabSource != null)
        {
            _prefabInstanceCount++;
            
            // 프리팹과 씬 인스턴스의 레이어 비교
            int childSyncCount = CountChildrenToSync(obj, prefabSource);
            bool rootWillChange = obj.layer != prefabSource.layer;
            
            string currentLayerName = LayerMask.LayerToName(obj.layer);
            string prefabLayerName = LayerMask.LayerToName(prefabSource.layer);
            
            if (rootWillChange || childSyncCount > 0)
            {
                _changedObjectCount += (rootWillChange ? 1 : 0) + childSyncCount;
                
                _previewResults.Add(new PreviewResult
                {
                    Name = obj.name,
                    PrefabName = prefabSource.name,
                    CurrentLayer = currentLayerName,
                    PrefabLayer = prefabLayerName,
                    WillChange = rootWillChange,
                    ChildSyncCount = childSyncCount
                });
            }
            
            _syncedObjectCount += 1 + CountAllChildren(obj.transform);
        }
        else if (prefabSource == null && !isPrefabRoot)
        {
            // 프리팹이 아닌 오브젝트
            _nonPrefabCount++;
        }
        
        // 자식들도 검사 (중첩 프리팹 처리)
        foreach (Transform child in obj.transform)
        {
            PreviewObjectRecursively(child.gameObject);
        }
    }
    
    /// <summary>
    /// 자식 오브젝트 중 레이어가 다른 것의 개수를 셉니다.
    /// </summary>
    private int CountChildrenToSync(GameObject sceneObj, GameObject prefabObj)
    {
        int count = 0;
        
        // 자식 개수가 다르면 구조가 다른 것 (수동 수정된 인스턴스)
        if (sceneObj.transform.childCount != prefabObj.transform.childCount)
        {
            return 0; // 구조가 다르면 동기화하지 않음
        }
        
        for (int i = 0; i < sceneObj.transform.childCount; i++)
        {
            Transform sceneChild = sceneObj.transform.GetChild(i);
            Transform prefabChild = prefabObj.transform.GetChild(i);
            
            // 이름이 같은지 확인
            if (sceneChild.name != prefabChild.name)
            {
                continue; // 이름이 다르면 스킵
            }
            
            // 레이어가 다르면 카운트
            if (sceneChild.gameObject.layer != prefabChild.gameObject.layer)
            {
                count++;
            }
            
            // 재귀적으로 자식도 검사
            count += CountChildrenToSync(sceneChild.gameObject, prefabChild.gameObject);
        }
        
        return count;
    }
    
    private void DrawPreviewResults()
    {
        EditorGUILayout.LabelField("미리보기 결과", EditorStyles.boldLabel);
        
        // 통계 요약
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField($"프리팹 인스턴스: {_prefabInstanceCount}개");
        EditorGUILayout.LabelField($"동기화 대상 오브젝트: {_syncedObjectCount}개 (자식 포함)");
        EditorGUILayout.LabelField($"변경 예정: {_changedObjectCount}개", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"비프리팹 오브젝트: {_nonPrefabCount}개 (동기화 대상 아님)");
        EditorGUILayout.EndVertical();
        
        if (_previewResults.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "변경할 오브젝트가 없습니다.\n" +
                "씬 인스턴스의 레이어가 이미 프리팹과 동일합니다.",
                MessageType.Info);
            return;
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"변경될 오브젝트 ({_previewResults.Count}개):");
        
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200));
        
        foreach (var result in _previewResults)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(result.Name, EditorStyles.boldLabel, GUILayout.Width(200));
            EditorGUILayout.LabelField($"← {result.PrefabName}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
            
            if (result.WillChange)
            {
                EditorGUILayout.LabelField($"  루트: {result.CurrentLayer} → {result.PrefabLayer}");
            }
            
            if (result.ChildSyncCount > 0)
            {
                EditorGUILayout.LabelField($"  자식: {result.ChildSyncCount}개 변경");
            }
            
            EditorGUILayout.EndVertical();
        }
        
        EditorGUILayout.EndScrollView();
    }
    
    private void ApplySync()
    {
        int totalChanged = 0;
        
        // Undo 등록
        Undo.SetCurrentGroupName("Scene Layer Sync from Prefabs");
        int undoGroup = Undo.GetCurrentGroup();
        
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager
            .GetActiveScene().GetRootGameObjects();
        
        foreach (var root in rootObjects)
        {
            if (ShouldSkipObject(root)) continue;
            
            totalChanged += SyncObjectRecursively(root);
        }
        
        Undo.CollapseUndoOperations(undoGroup);
        
        // 씬 변경 표시
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        
        Debug.Log($"[SceneLayerAssigner] 동기화 완료: {totalChanged}개 오브젝트 레이어 변경됨");
        EditorUtility.DisplayDialog("완료", 
            $"{totalChanged}개 오브젝트의 레이어가 프리팹과 동기화되었습니다.\n\n" +
            "Ctrl+S로 씬을 저장하세요.",
            "확인");
        
        // 미리보기 갱신
        GeneratePreview();
    }
    
    /// <summary>
    /// 프리팹 소스의 레이어를 씬 인스턴스에 동기화합니다.
    /// </summary>
    private int SyncObjectRecursively(GameObject sceneObj)
    {
        int changedCount = 0;
        
        // 프리팹 소스 가져오기
        GameObject prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(sceneObj);
        bool isPrefabRoot = PrefabUtility.IsAnyPrefabInstanceRoot(sceneObj);
        
        if (isPrefabRoot && prefabSource != null)
        {
            // 프리팹 인스턴스 발견 - 레이어 동기화
            changedCount += SyncLayersFromPrefab(sceneObj, prefabSource);
        }
        
        // 자식들도 검사 (중첩 프리팹 처리)
        foreach (Transform child in sceneObj.transform)
        {
            // 자식이 별도의 프리팹 인스턴스 루트인지 확인
            if (PrefabUtility.IsAnyPrefabInstanceRoot(child.gameObject))
            {
                // 중첩 프리팹은 별도로 처리
                changedCount += SyncObjectRecursively(child.gameObject);
            }
        }
        
        return changedCount;
    }
    
    /// <summary>
    /// 프리팹과 씬 인스턴스의 레이어를 재귀적으로 동기화합니다.
    /// </summary>
    private int SyncLayersFromPrefab(GameObject sceneObj, GameObject prefabObj)
    {
        int changedCount = 0;
        
        // 루트 레이어 동기화
        if (sceneObj.layer != prefabObj.layer)
        {
            Undo.RecordObject(sceneObj, "Sync Layer");
            sceneObj.layer = prefabObj.layer;
            changedCount++;
        }
        
        // 자식 동기화 (구조가 같을 때만)
        if (sceneObj.transform.childCount == prefabObj.transform.childCount)
        {
            for (int i = 0; i < sceneObj.transform.childCount; i++)
            {
                Transform sceneChild = sceneObj.transform.GetChild(i);
                Transform prefabChild = prefabObj.transform.GetChild(i);
                
                // 이름이 같은지 확인
                if (sceneChild.name == prefabChild.name)
                {
                    // 중첩 프리팹이 아닌 경우에만 동기화
                    if (!PrefabUtility.IsAnyPrefabInstanceRoot(sceneChild.gameObject))
                    {
                        changedCount += SyncLayersFromPrefab(sceneChild.gameObject, prefabChild.gameObject);
                    }
                }
            }
        }
        
        return changedCount;
    }
    
    private bool ShouldSkipObject(GameObject obj)
    {
        string name = obj.name.ToLower();
        
        // 시스템 오브젝트 제외
        if (obj.GetComponent<Camera>() != null) return true;
        if (name.Contains("directional light")) return true;
        if (name.Contains("event system")) return true;
        if (name.Contains("canvas")) return true;
        
        // 비활성 "NEON CITY" 오브젝트 제외
        if (!obj.activeInHierarchy && obj.name == "NEON CITY") return true;
        
        return false;
    }
    
    private int CountAllChildren(Transform parent)
    {
        int count = 0;
        foreach (Transform child in parent)
        {
            count++;
            count += CountAllChildren(child);
        }
        return count;
    }
}
