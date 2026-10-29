using UnityEngine;
using UnityEngine.SceneManagement;

// 용어로 들어올 때마다 그 용어의 예제 씬을 연다
// (AI 스킬 랩은 비교 씬이 따로 → JSON 의 lab.scene 씬을 새로 불러온다. 같은 씬을 쓰는 용어끼리 옮겨도 새로 열어 처음 상태로)
// - 씬을 통째로 바꾸므로(LoadSceneMode.Single) 데모는 늘 처음 상태로 시작한다
// - 강의 화면(LectureOverlay)은 DontDestroyOnLoad 라서 씬이 바뀌어도 남는다
// - 씬 이름이 빌드에 없으면 경고만 남기고 지금 화면을 그대로 둔다
public static class LectureLabSetup
{
    // 첫 장면: 씬 목록·용어 목록을 띄우는 빈 배경 씬 (Assets/Lecture/LectureHub.unity)
    public const string HubScene = "LectureHub";

    public static bool InHub => SceneManager.GetActiveScene().name == HubScene;

    // entry = 열 용어, where = 경고에 붙일 위치 (예: "[Lecture] 0304 lab")
    public static bool Apply(GlossaryEntry entry, string where)
    {
        string scene = entry?.lab?.scene;
        if (string.IsNullOrEmpty(scene))
        {
            Debug.LogWarning($"{where}.scene 이 비어 있음");
            return false;
        }
        return LoadScene(scene, where);
    }

    // 허브(씬 목록·용어 목록)로 돌아감
    public static void LoadHub() => LoadScene(HubScene, "[Lecture] 허브");

    public static bool LoadScene(string scene, string where)
    {
        if (!Application.CanStreamedLevelBeLoaded(scene))
        {
            Debug.LogWarning($"{where} '{scene}' 씬이 빌드 목록에 없음");
            return false;
        }

        // (UI 이펙트 랩은 여기서 DOTween 트윈을 정리했지만, 이 랩은 DOTween 을 쓰지 않아 씬만 바꾼다)
        Debug.Log($"{where} → {scene} 씬 열기");
        SceneManager.LoadScene(scene, LoadSceneMode.Single);
        return true;
    }
}
