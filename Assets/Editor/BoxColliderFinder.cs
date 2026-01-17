using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 특정 위치 근처의 BoxCollider를 찾아 선택/삭제할 수 있는 에디터 도구.
/// 사용법: Unity 메뉴 > Tools > BoxCollider Finder
/// </summary>
public class BoxColliderFinder : EditorWindow
{
    // ===== 검색 설정 =====
    private Vector3 _searchPosition = Vector3.zero;
    private float _searchRadius = 10f;

    // ===== 검색 결과 =====
    private List<BoxCollider> _foundColliders = new List<BoxCollider>();
    private Vector2 _scrollPosition;

    [MenuItem("Tools/BoxCollider Finder")]
    public static void ShowWindow()
    {
        var window = GetWindow<BoxColliderFinder>("BoxCollider Finder");
        window.minSize = new Vector2(300, 400);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);

        // ===== 검색 위치 입력 =====
        EditorGUILayout.LabelField("검색 위치", EditorStyles.boldLabel);
        _searchPosition = EditorGUILayout.Vector3Field("좌표", _searchPosition);

        if (GUILayout.Button("선택된 오브젝트 위치 사용"))
        {
            if (Selection.activeTransform != null)
            {
                _searchPosition = Selection.activeTransform.position;
            }
        }

        EditorGUILayout.Space(10);

        // ===== 검색 반경 =====
        EditorGUILayout.LabelField("검색 반경", EditorStyles.boldLabel);
        _searchRadius = EditorGUILayout.Slider("반경", _searchRadius, 0.1f, 100f);

        EditorGUILayout.Space(10);

        // ===== 검색 버튼 =====
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("BoxCollider 검색", GUILayout.Height(30)))
        {
            FindBoxColliders();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(10);

        // ===== 검색 결과 표시 =====
        EditorGUILayout.LabelField($"검색 결과: {_foundColliders.Count}개", EditorStyles.boldLabel);

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        for (int i = _foundColliders.Count - 1; i >= 0; i--)
        {
            var collider = _foundColliders[i];

            if (collider == null)
            {
                _foundColliders.RemoveAt(i);
                continue;
            }

            EditorGUILayout.BeginHorizontal("box");

            float distance = Vector3.Distance(collider.transform.position, _searchPosition);
            string label = $"{collider.gameObject.name} ({distance:F1}m)";

            // 선택 버튼
            if (GUILayout.Button(label, GUILayout.ExpandWidth(true)))
            {
                Selection.activeGameObject = collider.gameObject;
                SceneView.lastActiveSceneView?.FrameSelected();
            }

            // 삭제 버튼
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("삭제", GUILayout.Width(50)))
            {
                if (EditorUtility.DisplayDialog("삭제 확인",
                    $"'{collider.gameObject.name}'의 BoxCollider를 삭제할까요?", "삭제", "취소"))
                {
                    Undo.DestroyObjectImmediate(collider);
                    _foundColliders.RemoveAt(i);
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void FindBoxColliders()
    {
        _foundColliders.Clear();

        // 씬의 모든 BoxCollider (비활성 포함)
        var allColliders = FindObjectsOfType<BoxCollider>(true);

        foreach (var collider in allColliders)
        {
            // 콜라이더 중심점 (월드 좌표)
            Vector3 center = collider.transform.TransformPoint(collider.center);
            float distance = Vector3.Distance(center, _searchPosition);

            if (distance <= _searchRadius)
            {
                _foundColliders.Add(collider);
            }
        }

        // 거리순 정렬
        _foundColliders.Sort((a, b) =>
        {
            float distA = Vector3.Distance(a.transform.TransformPoint(a.center), _searchPosition);
            float distB = Vector3.Distance(b.transform.TransformPoint(b.center), _searchPosition);
            return distA.CompareTo(distB);
        });

        Debug.Log($"[BoxCollider Finder] {_foundColliders.Count}개 발견");
    }
}