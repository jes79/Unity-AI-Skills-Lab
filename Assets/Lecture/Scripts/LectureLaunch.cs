using UnityEngine;

// AI 스킬 랩 강의 시작 처리 (강의 프로그램 Builds/LecturePlayer/SkillsLab.exe 와 에디터 플레이 둘 다)
// - 첫 장면이 허브(LectureHub)일 때만 강의 화면(LectureOverlay)을 만든다. 씬이 바뀌어도 남는다(DontDestroyOnLoad)
//   → 예제 씬 하나만 열어 플레이하면 강의 화면 없이 예제 그대로
// - 명령줄 인자 (OMP Studio 앱의 ▶ 예제 실행이 이렇게 실행한다)
//     -glossaryStart 0302 -glossaryLecture : 그 용어부터 강의 모드 (① 소개부터, 분야 첫 용어면 분야 소개부터)
//     -glossaryStart 0302                  : 강의 없이 그 용어의 예제 씬만 열기
//     인자 없음                            : 자유 모드 (허브의 씬 목록에서 골라 보기)
//   없는 번호를 주면 용어 목록을 열고 위쪽에 알림을 한 줄 띄운다
// - 강의 프로그램 빌드에서만: 테두리 없는 창 전체화면(현재 모니터 해상도)으로 시작
//   (Tools > Skills Lab > Build Lecture Player 가 SKILLS_LAB_LECTURE_PLAYER 정의를 붙여 빌드함)
public static class LectureLaunch
{
    public const string StartArg = "-glossaryStart";
    public const string LectureArg = "-glossaryLecture";

    // 상수(const) 대신 속성으로 둬서 '실행되지 않는 코드' 경고가 나지 않게 함
#if SKILLS_LAB_LECTURE_PLAYER
    public static bool IsLecturePlayer => true;
#else
    public static bool IsLecturePlayer => false;
#endif

    // 첫 장면이 뜨기 전에 창 모양부터 정함
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void SetupWindow()
    {
        if (!IsLecturePlayer || Application.isEditor) return;

        // 앱(OMP Studio)이 창 포커스를 가져가도 강의(글자 타이핑·데모 연출)가 멈추지 않게 (프로젝트 설정은 그대로, 강의 프로그램에서만)
        Application.runInBackground = true;

        // -screen-width / -screen-fullscreen 처럼 창 크기를 직접 준 실행은 그대로 둔다
        foreach (string arg in System.Environment.GetCommandLineArgs())
            if (arg.StartsWith("-screen-") || arg == "-popupwindow" || arg == "-window-mode") return;

        // 해상도·창 모드는 레지스트리에 남아 다음 실행에 다시 쓰이므로 매번 맞춰 둔다
        var display = Screen.mainWindowDisplayInfo;
        int width = display.width > 0 ? display.width : Screen.currentResolution.width;
        int height = display.height > 0 ? display.height : Screen.currentResolution.height;
        Screen.SetResolution(width, height, FullScreenMode.FullScreenWindow);
    }

    // 장면이 열린 뒤: 강의 화면을 만들고 인자를 본다
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void HandleArgs()
    {
        if (!LectureLabSetup.InHub) return; // 예제 씬을 바로 플레이함 → 강의 화면 없이 예제 그대로

        var lecture = LectureOverlay.Create();

        string[] args = System.Environment.GetCommandLineArgs();
        string id = null;
        bool lectureMode = false;
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == LectureArg) lectureMode = true;
            if (args[i] == StartArg && i + 1 < args.Length) id = args[i + 1].Trim();
        }
        if (id == null)
        {
            Debug.Log("[LectureLaunch] 자유 모드 → 씬 목록");
            lecture.RunWhenReady(lecture.OpenSceneList);
            return;
        }

        if (!SkillsLabGlossary.TryFind(id, out var entry))
        {
            Debug.LogWarning($"[LectureLaunch] {StartArg} {id}: 없는 용어 → 용어 목록");
            lecture.RunWhenReady(() => lecture.OpenTermList($"'{id}' 번 용어가 없어 용어 목록을 열었습니다. 용어 번호는 4자리입니다 (예: 0302)."));
            return;
        }

        Debug.Log($"[LectureLaunch] {entry.id} {entry.term} 열기 (강의 모드 {(lectureMode ? "켬" : "끔")})");
        // 강의 화면이 Start 를 마친 다음에 씬을 바꿔야 첫 장면의 준비와 겹치지 않는다
        if (lectureMode)
            lecture.RunWhenReady(() => lecture.StartFromTerm(entry));
        else
            lecture.RunWhenReady(() => lecture.PrepareLabOnly(entry));
    }
}
