#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class GradeDataImporter : EditorWindow
{
    private TextAsset _csvFile;

    [MenuItem("Tools/Import Grade Data from CSV")]
    public static void ShowWindow()
    {
        GetWindow<GradeDataImporter>("Grade Data Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("CSV to ScriptableObject Importer", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        _csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", _csvFile, typeof(TextAsset), false);

        if (GUILayout.Button("Import and Create StageData"))
        {
            if (_csvFile != null)
            {
                ImportCSV();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please assign a CSV file first!", "OK");
            }
        }
    }

    private void ImportCSV()
    {
        string csvText = _csvFile.text;
        
        // 다양한 줄바꿈 문자 처리
        csvText = csvText.Replace("\r\n", "\n").Replace("\r", "\n");
        string[] lines = csvText.Split('\n');

        if (lines.Length < 2)
        {
            Debug.LogError("CSV file is empty or invalid.");
            return;
        }

        // 첫 번째 줄 디버그 출력
        Debug.Log($"[Debug] First line: '{lines[0]}'");
        Debug.Log($"[Debug] Line length: {lines[0].Length}");

        // 헤더 파싱 (탭이나 콤마 구분 모두 지원)
        char delimiter = lines[0].Contains('\t') ? '\t' : ',';
        Debug.Log($"[Debug] Using delimiter: '{delimiter}'");

        string[] headers = lines[0].Split(delimiter);
        
        // 헤더 디버그 출력
        Debug.Log($"[Debug] Headers count: {headers.Length}");
        for (int i = 0; i < headers.Length; i++)
        {
            Debug.Log($"[Debug] Header[{i}]: '{headers[i].Trim()}'");
        }

        List<string> stageIds = new List<string>();
        int rewardsColumnIndex = -1;
        
        for (int i = 1; i < headers.Length; i++)
        {
            string header = headers[i].Trim();
            
            // "Rewards" 또는 "Reward" 모두 허용
            if (header.Equals("Rewards", System.StringComparison.OrdinalIgnoreCase) ||
                header.Equals("Reward", System.StringComparison.OrdinalIgnoreCase))
            {
                rewardsColumnIndex = i;
                Debug.Log($"[Debug] Found Rewards column at index: {i}");
                break;
            }
            
            // Rewards 컬럼이 아니면 스테이지 ID로 추가
            if (!string.IsNullOrWhiteSpace(header))
            {
                stageIds.Add(header);
            }
        }

        if (rewardsColumnIndex == -1)
        {
            Debug.LogError($"'Rewards' column not found in CSV! Headers: {string.Join(", ", headers)}");
            return;
        }

        Debug.Log($"[Debug] Stage IDs: {string.Join(", ", stageIds)}");

        // 등급별 보상 매핑
        Dictionary<string, int> gradeRewards = new Dictionary<string, int>();

        // 스테이지별 등급 설정 생성
        Dictionary<string, List<GradeConfig>> stageGrades = new Dictionary<string, List<GradeConfig>>();
        
        foreach (string stageId in stageIds)
        {
            stageGrades[stageId] = new List<GradeConfig>();
        }

        // 각 등급 라인 파싱
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] values = lines[i].Split(delimiter);
            if (values.Length < 2) continue;

            string grade = values[0].Trim(); // S, A, B, C

            // Rewards 컬럼에서 보상 값 읽기
            int rewardStars = 0;
            if (rewardsColumnIndex < values.Length)
            {
                if (int.TryParse(values[rewardsColumnIndex].Trim(), out int parsedReward))
                {
                    rewardStars = parsedReward;
                    gradeRewards[grade] = rewardStars;
                    Debug.Log($"[Debug] Grade {grade} = {rewardStars} stars");
                }
            }

            // 각 스테이지의 점수 파싱
            for (int j = 1; j < values.Length && j - 1 < stageIds.Count; j++)
            {
                // Rewards 컬럼은 건너뛰기
                if (j == rewardsColumnIndex) continue;
                
                string stageId = stageIds[j - 1];
                if (int.TryParse(values[j].Trim(), out int requiredScore))
                {
                    stageGrades[stageId].Add(new GradeConfig
                    {
                        Grade = grade,
                        RequiredScore = requiredScore,
                        RewardStars = rewardStars
                    });
                }
            }
        }

        // ScriptableObject 생성
        StageData stageData = ScriptableObject.CreateInstance<StageData>();
        
        List<StageGradeConfig> stageConfigs = new List<StageGradeConfig>();
        foreach (var kvp in stageGrades)
        {
            stageConfigs.Add(new StageGradeConfig
            {
                StageId = kvp.Key,
                Grades = kvp.Value.ToArray()
            });
        }

        // SerializedObject를 통해 private 필드 설정
        SerializedObject serializedObject = new SerializedObject(stageData);
        SerializedProperty stageConfigsProp = serializedObject.FindProperty("_stageConfigs");
        
        stageConfigsProp.arraySize = stageConfigs.Count;
        for (int i = 0; i < stageConfigs.Count; i++)
        {
            SerializedProperty element = stageConfigsProp.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("StageId").stringValue = stageConfigs[i].StageId;
            
            SerializedProperty gradesProp = element.FindPropertyRelative("Grades");
            gradesProp.arraySize = stageConfigs[i].Grades.Length;
            
            for (int j = 0; j < stageConfigs[i].Grades.Length; j++)
            {
                SerializedProperty gradeElement = gradesProp.GetArrayElementAtIndex(j);
                gradeElement.FindPropertyRelative("Grade").stringValue = stageConfigs[i].Grades[j].Grade;
                gradeElement.FindPropertyRelative("RequiredScore").intValue = stageConfigs[i].Grades[j].RequiredScore;
                gradeElement.FindPropertyRelative("RewardStars").intValue = stageConfigs[i].Grades[j].RewardStars;
            }
        }
        
        serializedObject.ApplyModifiedProperties();

        // 파일 저장
        string path = "Assets/10.ScriptableObjects/StageData.asset";
        AssetDatabase.CreateAsset(stageData, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 보상 정보 출력
        string rewardInfo = "Grade Rewards:\n";
        foreach (var kvp in gradeRewards)
        {
            rewardInfo += $"{kvp.Key} = {kvp.Value} stars\n";
        }

        EditorUtility.DisplayDialog("Success",
            $"StageData created at {path}\n\n{rewardInfo}\n\nStages: {stageIds.Count}",
            "OK");
        Selection.activeObject = stageData;
        
        Debug.Log($"[GradeDataImporter] Import completed successfully!");
        Debug.Log($"[GradeDataImporter] Stages imported: {string.Join(", ", stageIds)}");
        Debug.Log($"[GradeDataImporter] {rewardInfo}");
    }
}
#endif