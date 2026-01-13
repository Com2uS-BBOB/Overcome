using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// DECO_ 하위 오브젝트의 MeshCollider를 일괄 제거하는 에디터 도구.
/// 성능 최적화를 위해 장식 오브젝트의 불필요한 MeshCollider를 제거한다.
/// </summary>
public class RemoveDecoMeshColliders : EditorWindow
{
    // 제거 대상 부모 오브젝트 접두사
    private const string TargetPrefix = "DECO_";
    
    // 스크롤 위치 (미리보기용)
    private Vector2 _scrollPosition;
    
    // 검색된 MeshCollider 목록
    private List<MeshCollider> _foundColliders = new List<MeshCollider>();
    
    // 검색 완료 여부
    private bool _hasSearched = false;

    /// <summary>
    /// 메뉴에 등록: Tools > Remove DECO MeshColliders
    /// </summary>
    [MenuItem("Tools/Remove DECO MeshColliders")]
    public static void ShowWindow()
    {
        var window = GetWindow<RemoveDecoMeshColliders>("DECO MeshCollider 제거");
        window.minSize = new Vector2(400, 300);
    }

    private void OnGUI()
    {
        GUILayout.Label("DECO_ 하위 MeshCollider 일괄 제거", EditorStyles.boldLabel);
        GUILayout.Space(5);
        
        // 설명 박스
        EditorGUILayout.HelpBox(
            "이 도구는 'DECO_'로 시작하는 오브젝트 하위의 모든 MeshCollider를 제거합니다.\n" +
            "장식 오브젝트의 MeshCollider는 성능에 악영향을 주므로 제거를 권장합니다.",
            MessageType.Info);
        
        GUILayout.Space(10);

        // Step 1: 검색 버튼
        if (GUILayout.Button("1. MeshCollider 검색", GUILayout.Height(30)))
        {
            SearchMeshColliders();
        }

        GUILayout.Space(10);

        // 검색 결과 표시
        if (_hasSearched)
        {
            EditorGUILayout.LabelField($"발견된 MeshCollider: {_foundColliders.Count}개", EditorStyles.boldLabel);
            
            if (_foundColliders.Count > 0)
            {
                // 미리보기 스크롤 영역
                GUILayout.Label("미리보기 (처음 50개):", EditorStyles.miniLabel);
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(150));
                
                int previewCount = Mathf.Min(_foundColliders.Count, 50);
                for (int i = 0; i < previewCount; i++)
                {
                    if (_foundColliders[i] != null)
                    {
                        EditorGUILayout.LabelField($"  • {GetFullPath(_foundColliders[i].gameObject)}", EditorStyles.miniLabel);
                    }
                }
                
                if (_foundColliders.Count > 50)
                {
                    EditorGUILayout.LabelField($"  ... 외 {_foundColliders.Count - 50}개", EditorStyles.miniLabel);
                }
                
                EditorGUILayout.EndScrollView();

                GUILayout.Space(10);

                // Step 2: 제거 버튼
                GUI.backgroundColor = new Color(1f, 0.6f, 0.6f); // 빨간색 배경
                if (GUILayout.Button($"2. {_foundColliders.Count}개 MeshCollider 제거", GUILayout.Height(35)))
                {
                    RemoveColliders();
                }
                GUI.backgroundColor = Color.white;

                GUILayout.Space(5);
                EditorGUILayout.HelpBox("⚠️ Ctrl+Z로 되돌릴 수 있습니다.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("제거할 MeshCollider가 없습니다.", MessageType.Info);
            }
        }
    }

    /// <summary>
    /// DECO_ 하위의 모든 MeshCollider를 검색한다.
    /// </summary>
    private void SearchMeshColliders()
    {
        _foundColliders.Clear();
        
        // 씬의 모든 루트 오브젝트 검색
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager
            .GetActiveScene()
            .GetRootGameObjects();

        foreach (GameObject root in rootObjects)
        {
            // DECO_로 시작하는 오브젝트만 처리
            if (root.name.StartsWith(TargetPrefix))
            {
                // 하위의 모든 MeshCollider 수집
                MeshCollider[] colliders = root.GetComponentsInChildren<MeshCollider>(true);
                _foundColliders.AddRange(colliders);
            }
        }

        _hasSearched = true;
        
        Debug.Log($"[RemoveDecoMeshColliders] 검색 완료: {_foundColliders.Count}개의 MeshCollider 발견");
    }

    /// <summary>
    /// 검색된 MeshCollider를 모두 제거한다.
    /// </summary>
    private void RemoveColliders()
    {
        if (_foundColliders.Count == 0)
        {
            EditorUtility.DisplayDialog("알림", "제거할 MeshCollider가 없습니다.", "확인");
            return;
        }

        // 확인 다이얼로그
        bool confirm = EditorUtility.DisplayDialog(
            "MeshCollider 제거 확인",
            $"{_foundColliders.Count}개의 MeshCollider를 제거하시겠습니까?\n\n" +
            "이 작업은 Ctrl+Z로 되돌릴 수 있습니다.",
            "제거",
            "취소");

        if (!confirm) return;

        // Undo 그룹 시작
        Undo.SetCurrentGroupName("Remove DECO MeshColliders");
        int undoGroup = Undo.GetCurrentGroup();

        int removedCount = 0;
        int failedCount = 0;

        foreach (MeshCollider col in _foundColliders)
        {
            if (col != null)
            {
                Undo.DestroyObjectImmediate(col);
                removedCount++;
            }
            else
            {
                failedCount++;
            }
        }

        // Undo 그룹 종료
        Undo.CollapseUndoOperations(undoGroup);

        // 결과 표시
        string message = $"제거 완료: {removedCount}개";
        if (failedCount > 0)
        {
            message += $"\n실패: {failedCount}개 (이미 삭제됨)";
        }
        
        EditorUtility.DisplayDialog("완료", message, "확인");
        Debug.Log($"[RemoveDecoMeshColliders] {message}");

        // 검색 결과 초기화
        _foundColliders.Clear();
        _hasSearched = false;
    }

    /// <summary>
    /// 오브젝트의 전체 경로를 반환한다.
    /// </summary>
    private string GetFullPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        
        // 최대 3단계까지만 표시 (가독성)
        int depth = 0;
        while (parent != null && depth < 3)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
            depth++;
        }
        
        if (parent != null)
        {
            path = ".../" + path;
        }
        
        return path;
    }
}
