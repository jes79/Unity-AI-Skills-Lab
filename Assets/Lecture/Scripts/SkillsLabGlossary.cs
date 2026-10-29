using System;
using UnityEngine;

// AI 스킬 랩 강의 용어 데이터
// 원본은 Resources/SkillsLabGlossary.json 하나 (OMP Studio 앱의 강의 준비 창도 같은 파일을 읽는다)
// JsonUtility 로 읽으므로 JSON 칸 이름과 아래 변수 이름이 같아야 한다 (강의 화면이 쓰지 않는 칸은 읽지 않고 넘어감)
[Serializable]
public class GlossaryEntry
{
    public string id;        // 4자리 번호 (예: 0304) = 명령줄 -glossaryStart 값
    public int cat;          // 분야 번호 (1부터)
    public string sub;
    public string term;      // 용어 이름
    public string easy;      // 쉽게 말하면
    public string concept;   // ② 개념
    public string controls;  // ③ 조작 ("조작 : 하는 일" 줄바꿈)
    public string check;     // ③ 확인 포인트
    public string vague;     // ④ 막연한 요청
    public string prompt;    // ④ 구체적인 요청
    public LabSetup lab;     // 이 용어로 들어올 때 어떤 데모 씬을 열지

    public string CategoryName => SkillsLabGlossary.Category(cat).name;
}

// 용어별 랩 준비: AI 스킬 랩은 비교 씬이 따로 있어서 '어느 씬을 열지'만 적는다 (00_SkillMap 은 여러 용어가 같이 씀)
[Serializable]
public class LabSetup
{
    public string scene;     // 열 예제 씬 이름 (예: 0302_ButtonTransition = Assets/00_Scenes/0302_ButtonTransition.unity)
}

[Serializable]
public class GlossaryCategory
{
    public int n;
    public string name;
    public string presenter;      // 담당 팀원 키 (art, dev ...)
    public string intro;          // 분야 소개: 회사에서 이 분야를 맡은 사람이 하는 일
    public string numberExample;  // ④ '조건·수치'가 빠졌을 때 보여 줄 예
    public string targetExample;  // ④ '대상'이 빠졌을 때 보여 줄 예
}

[Serializable]
public class PresenterData
{
    public string role;
    public string nextLine;
    public string demoLine;
    public string accent;   // #rrggbb
}

[Serializable]
public class PresenterTable
{
    public PresenterData art, dev, director, plan, sound;
}

[Serializable]
public class GlossaryData
{
    public string id;
    public string title;
    public string[] targetWords;   // ④ '대상' 판정 단어 (앞에 있을수록 먼저 찾음). 앱도 같은 목록을 쓴다
    public PresenterTable presenters;
    public GlossaryCategory[] categories;
    public GlossaryEntry[] entries;
}

public static class SkillsLabGlossary
{
    public const string ResourceName = "SkillsLabGlossary";

    // 플레이할 때마다 JSON 을 다시 읽는다 (도메인을 다시 불러오지 않는 설정이어도 static 이 남지 않게. JSON 을 고친 뒤 바로 반영)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetCache() => data = null;

    static GlossaryData data;

    public static GlossaryData Data
    {
        get
        {
            if (data != null)
                return data;
            var file = Resources.Load<TextAsset>(ResourceName);
            if (file == null)
            {
                Debug.LogError($"[Lecture] Resources/{ResourceName}.json 이 없습니다.");
                data = new GlossaryData { categories = Array.Empty<GlossaryCategory>(), entries = Array.Empty<GlossaryEntry>() };
                return data;
            }
            data = JsonUtility.FromJson<GlossaryData>(file.text);
            data.categories ??= Array.Empty<GlossaryCategory>();
            data.entries ??= Array.Empty<GlossaryEntry>();
            return data;
        }
    }

    public static GlossaryEntry[] Entries => Data.entries;

    public static bool TryFind(string id, out GlossaryEntry entry)
    {
        entry = Array.Find(Entries, e => e.id == id);
        return entry != null;
    }

    // 이 예제 씬을 쓰는 첫 용어 (씬 목록에 한글 이름을 붙일 때)
    public static GlossaryEntry ForScene(string scene)
    {
        return Array.Find(Entries, e => e.lab != null && e.lab.scene == scene);
    }

    public static GlossaryCategory Category(int n)
    {
        var all = Data.categories;
        return Array.Find(all, c => c.n == n) ?? new GlossaryCategory { n = n, name = "", presenter = "dev", intro = "" };
    }
}
