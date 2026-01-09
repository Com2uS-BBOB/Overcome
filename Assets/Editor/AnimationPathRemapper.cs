using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class AnimationPathRemapper : EditorWindow
{
    private string oldPath = "spine_03/clavicle";
    private string newPath = "spine_03/spine_04/spine_05/clavicle";
    private Vector2 scrollPos;
    private List<string> logs = new List<string>();

    [MenuItem("Tools/Animation Path Remapper")]
    static void ShowWindow()
    {
        GetWindow<AnimationPathRemapper>("Animation Path Remapper");
    }

    void OnGUI()
    {
        GUILayout.Label("Animation Path Remapper", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("Grruzam → 사용자 캐릭터 경로 변환", EditorStyles.helpBox);
        GUILayout.Space(10);

        oldPath = EditorGUILayout.TextField("기존 경로 (Old Path)", oldPath);
        newPath = EditorGUILayout.TextField("새 경로 (New Path)", newPath);

        GUILayout.Space(10);

        if (GUILayout.Button("선택한 애니메이션 클립 경로 변환", GUILayout.Height(30)))
        {
            RemapSelectedClips();
        }

        GUILayout.Space(5);

        if (GUILayout.Button("선택한 폴더 내 모든 클립 변환", GUILayout.Height(30)))
        {
            RemapClipsInSelectedFolder();
        }

        GUILayout.Space(10);
        GUILayout.Label("로그:", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(200));
        foreach (var log in logs)
        {
            GUILayout.Label(log);
        }
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("로그 지우기"))
        {
            logs.Clear();
        }
    }

    void RemapSelectedClips()
    {
        var selectedObjects = Selection.objects;
        int remappedCount = 0;

        foreach (var obj in selectedObjects)
        {
            if (obj is AnimationClip clip)
            {
                int count = RemapClip(clip);
                remappedCount += count;
                logs.Add($"[{clip.name}] {count}개 경로 변환됨");
            }
        }

        if (remappedCount > 0)
        {
            AssetDatabase.SaveAssets();
            logs.Add($"완료! 총 {remappedCount}개 경로 변환됨");
        }
        else
        {
            logs.Add("변환할 경로가 없거나 AnimationClip이 선택되지 않았습니다.");
        }

        Repaint();
    }

    void RemapClipsInSelectedFolder()
    {
        string folderPath = "";

        // 선택한 폴더 경로 가져오기
        if (Selection.activeObject != null)
        {
            folderPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                folderPath = System.IO.Path.GetDirectoryName(folderPath);
            }
        }

        if (string.IsNullOrEmpty(folderPath))
        {
            logs.Add("폴더를 선택해주세요.");
            Repaint();
            return;
        }

        // 폴더 내 모든 AnimationClip 찾기
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { folderPath });
        int totalRemapped = 0;

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);

            if (clip != null && !clip.name.Contains("__preview__"))
            {
                int count = RemapClip(clip);
                if (count > 0)
                {
                    totalRemapped += count;
                    logs.Add($"[{clip.name}] {count}개 경로 변환됨");
                }
            }
        }

        if (totalRemapped > 0)
        {
            AssetDatabase.SaveAssets();
            logs.Add($"완료! 총 {totalRemapped}개 경로 변환됨");
        }
        else
        {
            logs.Add("변환할 경로가 없습니다.");
        }

        Repaint();
    }

    int RemapClip(AnimationClip clip)
    {
        if (clip == null) return 0;

        int remappedCount = 0;

        // Transform 커브 바인딩 처리
        var curveBindings = AnimationUtility.GetCurveBindings(clip);
        foreach (var binding in curveBindings)
        {
            if (binding.path.Contains(oldPath))
            {
                var newBinding = binding;
                newBinding.path = binding.path.Replace(oldPath, newPath);

                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                AnimationUtility.SetEditorCurve(clip, binding, null);
                AnimationUtility.SetEditorCurve(clip, newBinding, curve);

                remappedCount++;
            }
        }

        // Object Reference 커브 바인딩 처리
        var objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
        foreach (var binding in objectBindings)
        {
            if (binding.path.Contains(oldPath))
            {
                var newBinding = binding;
                newBinding.path = binding.path.Replace(oldPath, newPath);

                var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
                AnimationUtility.SetObjectReferenceCurve(clip, binding, null);
                AnimationUtility.SetObjectReferenceCurve(clip, newBinding, keyframes);

                remappedCount++;
            }
        }

        if (remappedCount > 0)
        {
            EditorUtility.SetDirty(clip);
        }

        return remappedCount;
    }
}