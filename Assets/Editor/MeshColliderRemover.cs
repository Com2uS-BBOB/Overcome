#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// 씬의 모든 MeshCollider를 일괄 제거하는 에디터 유틸리티.
/// 메뉴: Tools > Remove All MeshColliders
/// </summary>
public static class MeshColliderRemover
{
    [MenuItem("Tools/Remove All MeshColliders")]
    private static void RemoveAllMeshColliders()
    {
        // 씬의 모든 MeshCollider 검색 (비활성 오브젝트 포함)
        var meshColliders = Object.FindObjectsByType<MeshCollider>(
            FindObjectsInactive.Include, 
            FindObjectsSortMode.None
        );
        
        if (meshColliders.Length == 0)
        {
            EditorUtility.DisplayDialog("완료", "MeshCollider가 없습니다.", "확인");
            return;
        }
        
        // 제거 전 확인 다이얼로그
        bool confirmed = EditorUtility.DisplayDialog(
            "MeshCollider 제거",
            $"{meshColliders.Length}개의 MeshCollider를 제거합니다.\n계속하시겠습니까?",
            "제거", "취소"
        );
        
        if (!confirmed) return;
        
        // Undo 그룹 시작 (Ctrl+Z로 한 번에 복구 가능)
        Undo.SetCurrentGroupName("Remove All MeshColliders");
        int undoGroup = Undo.GetCurrentGroup();
        
        // 모든 MeshCollider 제거
        foreach (var collider in meshColliders)
        {
            Undo.DestroyObjectImmediate(collider);
        }
        
        // Undo 작업을 하나로 묶음
        Undo.CollapseUndoOperations(undoGroup);
        
        Debug.Log($"[MeshColliderRemover] {meshColliders.Length}개 MeshCollider 제거 완료. Ctrl+Z로 복구 가능.");
    }
}
#endif
