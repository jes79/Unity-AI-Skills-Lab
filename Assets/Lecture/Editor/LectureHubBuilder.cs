using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 강의 모드 첫 장면(허브) 만들기 + 빌드 씬 목록 정리
// - Assets/Lecture/LectureHub.unity : 카메라·EventSystem·제목만 있는 빈 씬 (예제 씬과 같은 모양, SkillsLabSceneKit 사용)
//   씬 목록·용어 목록은 LectureOverlay 가 UI Toolkit 으로 그 위에 그린다
// - Build Settings 씬 목록 = 허브(맨 앞) + 예제 씬 (Assets/00_Scenes 의 00_SkillMap, 0301_UIRouter ... 0501_ClassRules)
public static class LectureHubBuilder
{
    public const string HubPath = "Assets/Lecture/LectureHub.unity";

    // 예제 씬: Assets/00_Scenes/NN_Name.unity 또는 NNNN_Name.unity (숫자로 시작하는 씬)
    static readonly Regex DemoSceneName = new Regex(@"^\d{2,4}_");

    [MenuItem("Tools/Skills Lab/Build Lecture Hub")]
    public static void Build()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        SkillsLabSceneKit.NewScene("AI 스킬 랩", "아래 씬 목록에서 골라 보거나, F4 를 눌러 강의를 시작하세요");
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), HubPath);
        Debug.Log($"[LectureHubBuilder] {HubPath} 저장");

        UpdateBuildSettings();
    }

    // 허브가 없으면 만들고, Build Settings 씬 목록을 맞춘다 (강의 플레이어 빌드 전에 호출)
    public static void EnsureHub()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(HubPath) == null) Build();
        else UpdateBuildSettings();
    }

    // 허브 + 예제 씬 (이름 순서 = 번호 순서)
    public static string[] ScenePaths()
    {
        var demos = AssetDatabase.FindAssets("t:Scene", new[] { SkillsLabSceneKit.SceneFolder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => DemoSceneName.IsMatch(System.IO.Path.GetFileNameWithoutExtension(path)))
            .OrderBy(path => path, System.StringComparer.Ordinal);
        return new[] { HubPath }.Concat(demos).ToArray();
    }

    public static void UpdateBuildSettings()
    {
        var paths = ScenePaths();
        EditorBuildSettings.scenes = paths.Select(p => new EditorBuildSettingsScene(p, true)).ToArray();
        Debug.Log($"[LectureHubBuilder] Build Settings 씬 {paths.Length}개 (허브 + 예제 {paths.Length - 1}개)");
    }
}
