#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;

public class ReplaceTMPFontTool : EditorWindow
{
    private TMP_FontAsset newFont;

    [MenuItem("Tools/Replace All TMP Fonts")]
    static void ShowWindow()
    {
        GetWindow<ReplaceTMPFontTool>("Replace TMP Fonts");
    }

    void OnGUI()
    {
        GUILayout.Label("Replace All TMP Fonts", EditorStyles.boldLabel);
        
        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField(
            "New Font", 
            newFont, 
            typeof(TMP_FontAsset), 
            false
        );

        if (GUILayout.Button("Replace All Fonts in Scene"))
        {
            ReplaceAllFontsInScene();
        }

        if (GUILayout.Button("Replace All Fonts in Project"))
        {
            ReplaceAllFontsInProject();
        }
    }

    void ReplaceAllFontsInScene()
    {
        if (newFont == null)
        {
            Debug.LogError("Please assign a new font!");
            return;
        }

        // UI 텍스트 (TextMeshProUGUI)
        // FindObjectsByType 사용 (Unity 2023+)
        TextMeshProUGUI[] uiTexts = FindObjectsByType<TextMeshProUGUI>(
            FindObjectsInactive.Include, 
            FindObjectsSortMode.None
        );
        
        foreach (var tmp in uiTexts)
        {
            Undo.RecordObject(tmp, "Replace Font");
            tmp.font = newFont;
            EditorUtility.SetDirty(tmp);
        }

        // 3D 텍스트 (TextMeshPro)
        TextMeshPro[] worldTexts = FindObjectsByType<TextMeshPro>(
            FindObjectsInactive.Include, 
            FindObjectsSortMode.None
        );
        
        foreach (var tmp in worldTexts)
        {
            Undo.RecordObject(tmp, "Replace Font");
            tmp.font = newFont;
            EditorUtility.SetDirty(tmp);
        }

        Debug.Log($"Replaced {uiTexts.Length + worldTexts.Length} TMP fonts!");
    }

    void ReplaceAllFontsInProject()
    {
        if (newFont == null)
        {
            Debug.LogError("Please assign a new font!");
            return;
        }

        string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");
        int count = 0;

        foreach (string guid in allPrefabs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            TextMeshProUGUI[] uiTexts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);
            TextMeshPro[] worldTexts = prefab.GetComponentsInChildren<TextMeshPro>(true);

            if (uiTexts.Length > 0 || worldTexts.Length > 0)
            {
                foreach (var tmp in uiTexts)
                {
                    tmp.font = newFont;
                    count++;
                }

                foreach (var tmp in worldTexts)
                {
                    tmp.font = newFont;
                    count++;
                }

                EditorUtility.SetDirty(prefab);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Replaced {count} TMP fonts in all prefabs!");
    }
}
#endif