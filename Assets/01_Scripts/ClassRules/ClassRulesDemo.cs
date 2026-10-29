using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// 0501 우리 반 규칙 스킬 (.claude/skills/class-rules/SKILL.md = .agents/skills/class-rules/SKILL.md)
// 탭 4개: SKILL.md 보기 · A 결과 코드 · B 결과 코드 · 실행 결과
// - A: 규칙 스킬 없이 흔히 나오는 코드 (ScoreManagerA.cs: public 필드 · 매직 넘버 · Update 에서 FindObjectOfType · OnGUI · Debug.Log)
// - B: 규칙 스킬대로 나온 코드 (ScoreCounterB.cs: [SerializeField] 숫자 · camelCase · 한글 주석 한 줄 · Debug.Log 없음 · UI Toolkit)
// 두 코드 모두 실제로 컴파일되어 '실행 결과' 탭에서 돌아간다 (Space = 점수 +10)
// 파일 원문은 씬을 만들 때(Tools > Skills Lab > Build All Scenes) 저장소의 파일에서 읽어 넣는다
public class ClassRulesDemo : AbCompareScene
{
    enum Tab { SkillFile, CodeA, CodeB, Run }

    static readonly string[] TabNames = { "SKILL.md 보기", "A 결과 코드", "B 결과 코드", "실행 결과" };
    static readonly Color TabOn = new Color32(90, 140, 255, 255);
    static readonly Color TabOff = new Color32(45, 52, 70, 255);

    [Header("파일 원문 (씬을 만들 때 읽어 넣음)")]
    [SerializeField, TextArea(3, 20)] string skillFrontMatter;   // SKILL.md 맨 위 YAML (name · description)
    [SerializeField, TextArea(5, 40)] string skillBody;          // SKILL.md 본문
    [SerializeField, TextArea(5, 40)] string codeA;              // ScoreManagerA.cs
    [SerializeField, TextArea(5, 40)] string codeB;              // ScoreCounterB.cs

    [Header("실행 결과")]
    [SerializeField] ScoreManagerA runnerA;
    [SerializeField] ScoreCounterB runnerB;
    [SerializeField] GameObject coin;   // 점수를 얻으면 튀는 동전 (실행 결과 탭에서만 보임)

    Tab tab = Tab.SkillFile;
    readonly Button[] tabButtons = new Button[4];
    VisualElement page;
    ScrollView scroll;
    Label pathLabel, frontLabel, bodyLabel, codeLabel, noteLabel, runLabel;

    protected override void BuildSceneUi()
    {
        // 탭 줄
        var bar = new VisualElement { pickingMode = PickingMode.Ignore };
        bar.style.position = Position.Absolute;
        bar.style.left = 150f;
        bar.style.top = 232f;
        bar.style.flexDirection = FlexDirection.Row;
        Root.Add(bar);
        for (int i = 0; i < TabNames.Length; i++)
        {
            var picked = (Tab)i;
            var button = new Button(() => SelectTab(picked)) { text = TabNames[i], focusable = false };
            button.style.fontSize = 22f;
            button.style.color = Color.white;
            button.style.marginLeft = 0f;
            button.style.marginRight = 8f;
            button.style.paddingLeft = button.style.paddingRight = 18f;
            button.style.paddingTop = button.style.paddingBottom = 8f;
            button.style.borderLeftWidth = button.style.borderRightWidth = button.style.borderTopWidth = button.style.borderBottomWidth = 0f;
            bar.Add(button);
            tabButtons[i] = button;
        }

        // 탭 내용 (코드·SKILL.md 는 스크롤)
        page = AddPanel(Root, 150f, 290f, 1050f);
        page.style.height = 700f;
        pathLabel = AddText(page, "", 19f, new Color(1f, 0.83f, 0.47f));
        pathLabel.style.marginBottom = 8f;
        scroll = new ScrollView(ScrollViewMode.Vertical);
        scroll.style.flexGrow = 1f;
        page.Add(scroll);
        frontLabel = AddText(scroll.contentContainer, "", 18f, new Color(0.75f, 1f, 0.8f));
        frontLabel.enableRichText = false;
        frontLabel.style.backgroundColor = new Color(0.1f, 0.25f, 0.15f, 0.9f);
        frontLabel.style.paddingLeft = frontLabel.style.paddingRight = 10f;
        frontLabel.style.paddingTop = frontLabel.style.paddingBottom = 6f;
        frontLabel.style.marginBottom = 8f;
        bodyLabel = AddText(scroll.contentContainer, "", 18f, new Color(0.88f, 0.9f, 0.95f));
        bodyLabel.enableRichText = false;
        bodyLabel.style.whiteSpace = WhiteSpace.PreWrap;   // 원문 들여쓰기 그대로
        codeLabel = AddText(scroll.contentContainer, "", 18f, new Color(0.88f, 0.9f, 0.95f));
        codeLabel.enableRichText = false;   // 코드 속 <T> 를 글자 그대로
        codeLabel.style.whiteSpace = WhiteSpace.PreWrap;
        noteLabel = AddText(page, "", 18f, new Color(0.95f, 0.88f, 0.6f));
        noteLabel.style.marginTop = 8f;

        // 실행 결과 탭 안내 (화면 아래쪽)
        runLabel = AddText(Root, "", 22f, new Color(0.85f, 0.9f, 0.95f));
        runLabel.style.position = Position.Absolute;
        runLabel.style.left = 160f;
        runLabel.style.top = 900f;
        runLabel.style.width = 1040f;

        // 강의 모드가 넘겨 준 지금 용어에 맞는 탭부터
        switch (LectureMode.CurrentId)
        {
            case "0503": tab = Tab.Run; break;
            default: tab = Tab.SkillFile; break;
        }
    }

    protected override void ShowSide(bool useSkill)
    {
        // 코드 탭을 보고 있으면 A/B 키가 코드 탭도 바꾼다
        if (tab == Tab.CodeA || tab == Tab.CodeB)
            tab = useSkill ? Tab.CodeB : Tab.CodeA;
        Refresh();
    }

    protected override void Update()
    {
        base.Update();
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (keyboard.rightArrowKey.wasPressedThisFrame) SelectTab((Tab)(((int)tab + 1) % 4));
        else if (keyboard.leftArrowKey.wasPressedThisFrame) SelectTab((Tab)(((int)tab + 3) % 4));
    }

    void SelectTab(Tab next)
    {
        tab = next;
        // A/B 코드 탭을 고르면 A/B 도 그쪽으로 (SetSide 가 Refresh 까지 부름)
        if (next == Tab.CodeA && UseSkill) { SetSide(false); return; }
        if (next == Tab.CodeB && !UseSkill) { SetSide(true); return; }
        Refresh();
    }

    void Refresh()
    {
        for (int i = 0; i < tabButtons.Length; i++)
            tabButtons[i].style.backgroundColor = (int)tab == i ? TabOn : TabOff;

        bool run = tab == Tab.Run;
        page.style.display = run ? DisplayStyle.None : DisplayStyle.Flex;
        runLabel.style.display = run ? DisplayStyle.Flex : DisplayStyle.None;
        frontLabel.style.display = bodyLabel.style.display = tab == Tab.SkillFile ? DisplayStyle.Flex : DisplayStyle.None;
        codeLabel.style.display = tab == Tab.CodeA || tab == Tab.CodeB ? DisplayStyle.Flex : DisplayStyle.None;
        scroll.scrollOffset = Vector2.zero;

        // 실행은 '실행 결과' 탭에서 지금 쪽(A/B) 하나만 (OnGUI 가 다른 탭 위에 겹쳐 그리지 않게)
        runnerA.enabled = run && !UseSkill;
        runnerB.enabled = run && UseSkill;
        coin.SetActive(run);

        switch (tab)
        {
            case Tab.SkillFile:
                pathLabel.text = "Claude Code: .claude/skills/class-rules/SKILL.md   ·   Codex: .agents/skills/class-rules/SKILL.md   (내용 같음)";
                frontLabel.text = skillFrontMatter;
                bodyLabel.text = skillBody;
                noteLabel.text = "초록 칸 = 맨 위 YAML. AI는 처음에 name · description 만 읽고, 요청과 맞으면 아래 본문을 읽는다\n" +
                                 "흐린 설명 예: 'description: 코드 규칙'  →  요청과 맞는지 판단할 낱말이 없어 잘 안 꺼낸다";
                break;
            case Tab.CodeA:
                pathLabel.text = "A 결과 코드: Assets/01_Scripts/ClassRules/ScoreManagerA.cs   (규칙 스킬 없이 흔한 코드, 재현)";
                codeLabel.text = codeA;
                noteLabel.text = "규칙과 다른 곳: public 필드(Score) · 숫자 10·100 이 코드 안에 · Update 마다 FindObjectOfType · OnGUI · Debug.Log · 영어 주석";
                break;
            case Tab.CodeB:
                pathLabel.text = "B 결과 코드: Assets/01_Scripts/ClassRules/ScoreCounterB.cs   (우리 반 규칙대로, 재현)";
                codeLabel.text = codeB;
                noteLabel.text = "규칙대로: 필드 camelCase · 숫자는 [SerializeField] 로 인스펙터에 · 부품은 인스펙터로 연결 · 한글 주석 한 줄 · Debug.Log 없음 · UI Toolkit(USS)";
                break;
            case Tab.Run:
                runLabel.text = UseSkill
                    ? "실행 중: ScoreCounterB — Space = 점수 +10 (UI Toolkit 글자 · 콘솔 로그 없음)\n← → 탭 바꾸기"
                    : "실행 중: ScoreManagerA — Space = 점수 +10 (OnGUI 글자 · 누를 때마다 콘솔에 Debug.Log)\n← → 탭 바꾸기";
                break;
        }
    }
}
