using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// 00_SkillMap 그림 지도: 사람 → AI(Claude Code / Codex) → 플러그인 상자(스킬 책) · Unity CLI(손) · MCP(전화선) → Unity 에디터
// - 용어마다 해당 칸을 강조하고(나머지는 흐리게), 아래 설명 패널에 그 용어의 핵심 줄을 보여 준다
// - 강의 모드가 용어를 열면 LectureMode.CurrentId 를 읽어 그 용어로 맞춘다 (같은 씬을 쓰는 용어 0101~0204, 0601~0604)
// - 조작: ← / → 다른 용어, Space 다음 단계 (0105 · 0201 · 0202 · 0204 는 단계가 여러 개)
public class SkillMapView : MonoBehaviour
{
    // 한 용어의 한 단계: 강조할 칸들 + 설명 줄
    class Step
    {
        public string[] focus;
        public string[] lines;
        public Step(string[] focus, params string[] lines) { this.focus = focus; this.lines = lines; }
    }

    class View
    {
        public string id, title;
        public Step[] steps;
        public View(string id, string title, params Step[] steps) { this.id = id; this.title = title; this.steps = steps; }
    }

    static readonly Color Ink = new Color32(235, 238, 245, 255);
    static readonly Color Focus = new Color32(255, 212, 121, 255);
    static readonly Color Line = new Color32(110, 120, 140, 255);
    static readonly Color NodeBg = new Color32(34, 40, 56, 255);

    // 칸 이름 → (위치·크기, 1920x1080 기준)
    static readonly Dictionary<string, Rect> NodeRects = new Dictionary<string, Rect>
    {
        { "person", new Rect(40, 300, 150, 160) },
        { "ai", new Rect(250, 250, 240, 260) },
        { "plugin", new Rect(560, 50, 430, 330) },
        { "cli", new Rect(560, 405, 430, 115) },
        { "mcp", new Rect(560, 545, 430, 105) },
        { "editor", new Rect(1045, 250, 165, 260) },
    };

    // 화살표: (출발 칸, 도착 칸). 강조 이름은 "arrow.출발-도착"
    static readonly string[][] Arrows =
    {
        new[] { "person", "ai" },
        new[] { "ai", "plugin" },
        new[] { "ai", "cli" },
        new[] { "ai", "mcp" },
        new[] { "cli", "editor" },
        new[] { "mcp", "editor" },
    };

    static readonly View[] Views =
    {
        new View("0101", "스킬 (Skill) — AI에게 건네는 '일 설명서' 폴더",
            new Step(new[] { "plugin", "plugin.book", "chip.references", "chip.scripts" },
                "스킬 = 폴더 하나. 그 안의 SKILL.md 맨 위에 YAML(name · description), 아래에 본문 지시",
                "선택: references/ · scripts/ · assets/ 폴더를 같이 둘 수 있다",
                "AI는 처음엔 이름·설명만 읽고, 필요할 때 본문을 읽는다")),
        new View("0102", "플러그인 (Plugin) — Unity 스킬(설명서) 30여 권을 한 번에 꽂는 상자",
            new Step(new[] { "plugin", "plugin.books" },
                "Unity 공식 플러그인 (Unity Technologies) · github.com/Unity-Technologies/unity-agent-plugin = 스킬 묶음",
                "Claude Code용 29개 스킬(2026-09-09 발표) · Codex용 31개(2026-09-16) → 지금은 '30+ 스킬'",
                "Unity CLI 프로그램은 따로 설치한다. 에디터 조종에는 CLI · Pipeline 패키지 · 열린 에디터가 추가로 필요",
                "Unity 6.0 이상. Asset Store / Package Manager 패키지가 아니다")),
        new View("0103", "Unity CLI — 터미널에서 Unity를 움직이는 '손'",
            new Step(new[] { "cli", "editor", "arrow.cli-editor", "arrow.ai-cli" },
                "에디터를 조종하는 스킬은 Unity CLI 명령을 실행한다",
                "설치(Windows PowerShell): irm https://unity.com/install.ps1 | iex   또는   winget install Unity.CLI",
                "예: unity status · unity open ./MyProject · unity editors -i · unity doctor")),
        new View("0104", "MCP — AI와 프로그램을 잇는 표준 '전화선'",
            new Step(new[] { "mcp", "arrow.ai-mcp", "arrow.mcp-editor" },
                "Unity 플러그인은 스킬만 담고 MCP 서버는 쓰지 않는다",
                "MCP 서버는 따로 연결한다: unity mcp configure")),
        new View("0105", "스킬 · CLI · MCP 차이 — 아는 것 vs 하는 것",
            new Step(new[] { "plugin", "plugin.books", "arrow.ai-plugin" }, "① 스킬 = 아는 것: 언제·어떻게 할지 적힌 설명서 (AI가 읽는다)"),
            new Step(new[] { "cli", "arrow.ai-cli", "arrow.cli-editor", "editor" }, "② Unity CLI = 하는 것: 터미널 명령으로 에디터를 움직이는 손 (스킬이 실행한다)"),
            new Step(new[] { "mcp", "arrow.ai-mcp", "arrow.mcp-editor", "editor" }, "③ MCP = 하는 것: 따로 연결하는 전화선 (unity mcp configure)"),
            new Step(new[] { "person", "ai", "plugin", "cli", "mcp", "editor", "arrow.person-ai", "arrow.ai-plugin", "arrow.ai-cli", "arrow.ai-mcp", "arrow.cli-editor", "arrow.mcp-editor" },
                "④ 전체: 플러그인(스킬) = 아는 것, CLI · MCP = 하는 것",
                "AGENTS.md / CLAUDE.md = 항상 읽는 규칙, 스킬 = 필요할 때만 꺼내 읽는 설명서")),
        new View("0201", "Claude Code에 설치 — 명령 두 줄",
            new Step(new[] { "ai", "ai.claude" }, "① (claude 세션 안) /plugin marketplace add Unity-Technologies/unity-agent-plugin"),
            new Step(new[] { "ai", "ai.claude", "plugin", "arrow.ai-plugin" },
                "① /plugin marketplace add Unity-Technologies/unity-agent-plugin",
                "② /plugin install unity@unity-agent-plugin"),
            new Step(new[] { "ai", "ai.claude", "plugin", "arrow.ai-plugin" },
                "범위(scope): user = 모든 프로젝트 · project = 저장소를 같이 쓰는 모두 · local = 나만, 이 저장소만",
                "터미널: claude plugin install unity@unity-agent-plugin --scope local   ·   VS Code 확장은 /plugins"),
            new Step(new[] { "ai", "ai.claude", "plugin" },
                "전/후 실습: claude plugin disable unity@unity-agent-plugin → 같은 요청 → claude plugin enable unity@unity-agent-plugin",
                "세션 안에서는 /plugin disable · /plugin enable (또는 /plugin → Installed 탭)")),
        new View("0202", "Codex에 설치 — 같은 상자, 명령만 조금 다름",
            new Step(new[] { "ai", "ai.codex" }, "① (터미널) codex plugin marketplace add Unity-Technologies/unity-agent-plugin"),
            new Step(new[] { "ai", "ai.codex", "plugin", "arrow.ai-plugin" },
                "① codex plugin marketplace add Unity-Technologies/unity-agent-plugin",
                "② codex plugin add unity@unity-agent-plugin"),
            new Step(new[] { "ai", "ai.codex", "plugin" },
                "확인: /unity: · codex plugin list   ·   업데이트: codex plugin marketplace upgrade unity-agent-plugin",
                "전/후 실습(끄기 대신 빼고 다시 넣기): codex plugin remove unity@unity-agent-plugin → 같은 요청 → codex plugin add unity@unity-agent-plugin")),
        new View("0203", "설치 확인 (/unity:) — 목록이 뜨면 설명서가 꽂힌 것",
            new Step(new[] { "plugin", "plugin.books", "ai" },
                "/unity: 를 입력하면 스킬 목록이 뜬다 (예: /unity:ui · /unity:ui-uitk · /unity:2d-pixel-perfect)",
                "Claude Code: /plugin 으로 설치 상태 확인 · 업데이트 /plugin marketplace update unity-agent-plugin → /reload-plugins",
                "Codex: codex plugin list")),
        new View("0204", "자동 호출 vs 직접 부르기",
            new Step(new[] { "ai", "plugin", "plugin.books", "arrow.ai-plugin" },
                "자동 호출: 요청이 스킬의 description 과 맞으면 AI가 알아서 꺼낸다",
                "Codex는 처음에 스킬 이름·설명·경로만 읽고(문맥의 2% 안), 고르면 SKILL.md 전체를 읽는다"),
            new Step(new[] { "person", "ai", "plugin", "arrow.person-ai", "arrow.ai-plugin" },
                "직접 부르기: Claude Code는 /스킬이름 (플러그인 스킬은 /unity:ui 처럼)",
                "Codex는 $스킬이름 또는 /skills (Unity 플러그인은 /unity: 메뉴도 됨) — 자동으로 안 불리면 이름을 직접 부른다")),
        new View("0601", "질문이 많아짐 — 바꾸기 전에 먼저 묻는다",
            new Step(new[] { "person", "ai", "arrow.person-ai" },
                "플러그인이 있으면 AI가 무언가를 바꾸기 전에 질문을 더 한다",
                "예: /ui 는 프로젝트가 어떤 UI 시스템을 쓰는지 먼저 보고 UI Toolkit · uGUI · IMGUI 전문 스킬로 넘긴다",
                "요청에 내 환경(Unity 6 URP, UI Toolkit 등)을 먼저 적어 주면 질문이 줄어든다")),
        new View("0602", "결과가 이상할 때 references",
            new Step(new[] { "plugin", "plugin.book", "chip.references" },
                "결과가 이상하면: \"스킬의 references 폴더를 먼저 읽고 다시 해 줘\"",
                "UI 요청엔 프레임워크 이름을 넣기 (새 프로젝트는 UI Toolkit) · 자동으로 안 불리면 스킬 이름을 직접 부르기",
                "한계: 단순 uGUI 작업은 스킬이 있어도 차이가 작다")),
        new View("0603", "남이 만든 스킬 조심 — scripts 는 내 PC에서 실행된다",
            new Step(new[] { "plugin", "plugin.book", "chip.scripts", "person" },
                "스킬 폴더의 scripts/ 는 AI가 내 PC에서 실행할 수 있는 코드다",
                "받기 전에 SKILL.md 와 scripts/ 를 직접 열어 읽어 본다",
                "출처를 아는 곳(예: Unity-Technologies/unity-agent-plugin)에서 받되, 공식 저장소라도 설치 전에 직접 읽는다 (공식 ≠ 무조건 안전)")),
        new View("0604", "버전 조건 — Unity 6 이상, 에디터 조종은 CLI + Pipeline + 에디터 열림",
            new Step(new[] { "editor", "cli", "arrow.cli-editor" },
                "플러그인: Unity 6.0 이상",
                "에디터를 조종하는 스킬의 조건: Unity CLI 설치 + 프로젝트에 Pipeline 패키지(unity pipeline install) + 에디터가 열려 있음",
                "연결 확인: unity status   ·   문제 진단: unity doctor")),
    };

    [SerializeField] UIDocument document;

    readonly Dictionary<string, VisualElement> parts = new Dictionary<string, VisualElement>();
    readonly HashSet<string> focused = new HashSet<string>();
    VisualElement arrowLayer;
    Label titleLabel, linesLabel, stepLabel;
    int viewIndex, stepIndex;
    string lastLectureId;

    void OnEnable()
    {
        var root = document.rootVisualElement;
        arrowLayer = new VisualElement { pickingMode = PickingMode.Ignore };
        arrowLayer.style.position = Position.Absolute;
        arrowLayer.style.left = arrowLayer.style.top = arrowLayer.style.right = arrowLayer.style.bottom = 0f;
        arrowLayer.generateVisualContent += DrawArrows;
        root.Add(arrowLayer);

        var title = Text(root, "스킬 지도  ·  사람 → AI → 플러그인(스킬) · CLI · MCP → Unity 에디터", 22f, new Color(0.75f, 0.8f, 0.9f));
        title.style.position = Position.Absolute;
        title.style.left = 150f;
        title.style.top = 18f;

        BuildPerson(root);
        BuildAi(root);
        BuildPlugin(root);
        BuildBox(root, "cli", "Unity CLI  —  손", "터미널 명령으로 에디터를 움직임\nunity status · unity open ./MyProject");
        BuildBox(root, "mcp", "MCP  —  전화선", "플러그인과 별개, 따로 연결\nunity mcp configure");
        BuildBox(root, "editor", "Unity 에디터", "Unity 6 이상\n에디터 열림\n+ Pipeline 패키지", 20f);
        BuildDetail(root);

        // 강의 모드면 지금 용어, 아니면 명령줄 -glossaryStart 번호(강의 없이 이 씬만 열었을 때)부터
        lastLectureId = LectureMode.CurrentId;
        int start = FindView(lastLectureId ?? StartIdFromCommandLine());
        ShowView(start >= 0 ? start : 0, 0);
    }

    static string StartIdFromCommandLine()
    {
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == LectureLaunch.StartArg) return args[i + 1].Trim();
        return null;
    }

    void Update()
    {
        // 강의 모드에서 용어가 바뀌면 그 용어로
        if (LectureMode.CurrentId != lastLectureId)
        {
            lastLectureId = LectureMode.CurrentId;
            int index = FindView(lastLectureId);
            if (index >= 0) ShowView(index, 0);
        }

        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (keyboard.rightArrowKey.wasPressedThisFrame) ShowView((viewIndex + 1) % Views.Length, 0);
        else if (keyboard.leftArrowKey.wasPressedThisFrame) ShowView((viewIndex + Views.Length - 1) % Views.Length, 0);
        else if (keyboard.spaceKey.wasPressedThisFrame) ShowView(viewIndex, (stepIndex + 1) % Views[viewIndex].steps.Length);
    }

    static int FindView(string id)
    {
        for (int i = 0; i < Views.Length; i++)
            if (Views[i].id == id) return i;
        return -1;
    }

    void ShowView(int index, int step)
    {
        viewIndex = index;
        stepIndex = step;
        var view = Views[index];
        var current = view.steps[step];

        focused.Clear();
        foreach (string key in current.focus) focused.Add(key);

        foreach (var pair in parts)
        {
            bool on = focused.Contains(pair.Key);
            bool box = NodeRects.ContainsKey(pair.Key);
            var e = pair.Value;
            e.style.borderLeftColor = e.style.borderRightColor = e.style.borderTopColor = e.style.borderBottomColor = on ? Focus : Line;
            float width = on ? 4f : (box ? 2f : 1f);
            e.style.borderLeftWidth = e.style.borderRightWidth = e.style.borderTopWidth = e.style.borderBottomWidth = width;
            // 큰 칸은 강조가 아니면 흐리게 (안쪽 작은 칸은 큰 칸을 따라감)
            if (box) e.style.opacity = on ? 1f : 0.35f;
        }
        arrowLayer.MarkDirtyRepaint();

        titleLabel.text = $"{view.id}  {view.title}";
        linesLabel.text = string.Join("\n", current.lines);
        stepLabel.text = view.steps.Length > 1
            ? $"단계 {step + 1}/{view.steps.Length}   ·   Space 다음 단계   ·   ← → 다른 용어"
            : "← → 다른 용어";
    }

    // ───── 그림 만들기 ─────

    VisualElement NewNode(VisualElement root, string key)
    {
        var r = NodeRects[key];
        var node = new VisualElement { name = key, pickingMode = PickingMode.Ignore };
        node.style.position = Position.Absolute;
        node.style.left = r.x;
        node.style.top = r.y;
        node.style.width = r.width;
        node.style.height = r.height;
        node.style.backgroundColor = NodeBg;
        Round(node, 12f);
        node.style.paddingLeft = node.style.paddingRight = 12f;
        node.style.paddingTop = node.style.paddingBottom = 10f;
        root.Add(node);
        parts[key] = node;
        return node;
    }

    void BuildPerson(VisualElement root)
    {
        var node = NewNode(root, "person");
        node.style.alignItems = Align.Center;
        var head = new VisualElement { pickingMode = PickingMode.Ignore };
        head.style.width = head.style.height = 46f;
        head.style.backgroundColor = (Color)new Color32(250, 200, 150, 255);
        Round(head, 23f);
        node.Add(head);
        var body = new VisualElement { pickingMode = PickingMode.Ignore };
        body.style.width = 76f;
        body.style.height = 40f;
        body.style.marginTop = 4f;
        body.style.backgroundColor = (Color)new Color32(90, 140, 255, 255);
        body.style.borderTopLeftRadius = body.style.borderTopRightRadius = 30f;
        node.Add(body);
        Text(node, "사람 (나)", 22f, Ink).style.marginTop = 8f;
    }

    void BuildAi(VisualElement root)
    {
        var node = NewNode(root, "ai");
        Text(node, "AI 도구", 26f, Ink).style.unityFontStyleAndWeight = FontStyle.Bold;
        Text(node, "스킬을 골라 읽고 일한다", 16f, new Color(0.7f, 0.75f, 0.85f)).style.marginBottom = 10f;
        Chip(node, "ai.claude", "Claude Code", 24f, new Color32(204, 120, 80, 255));
        Chip(node, "ai.codex", "Codex", 24f, new Color32(70, 160, 130, 255));
    }

    void BuildPlugin(VisualElement root)
    {
        var node = NewNode(root, "plugin");
        Text(node, "플러그인 상자", 22f, Ink).style.unityFontStyleAndWeight = FontStyle.Bold;
        Text(node, "unity@unity-agent-plugin  ·  스킬(설명서) 30+ 권", 16f, new Color(0.7f, 0.75f, 0.85f)).style.marginBottom = 6f;

        var books = Row(node, "plugin.books");
        foreach (string skill in new[] { "/ui", "/ui-uitk", "/2d-pixel-perfect", "/urp-postprocessing", "/physics-3d-collision", "/initialize-ai-navigation", "/unity-cli", "… 30+" })
            Chip(books, null, skill, 15f, new Color32(60, 72, 100, 255));

        // 스킬 한 권을 펼친 모양: 폴더 = SKILL.md + references/ + scripts/ + assets/
        var book = Row(node, "plugin.book");
        book.style.marginTop = 8f;
        Text(book, "스킬 한 권 = 폴더:", 16f, new Color(0.8f, 0.85f, 0.95f)).style.marginRight = 6f;
        Chip(book, null, "SKILL.md", 15f, new Color32(80, 110, 70, 255));
        Chip(book, "chip.references", "references/", 15f, new Color32(70, 80, 110, 255));
        Chip(book, "chip.scripts", "scripts/", 15f, new Color32(120, 70, 70, 255));
        Chip(book, null, "assets/", 15f, new Color32(70, 80, 110, 255));

        // /unity-cli 는 CLI 프로그램이 아니라 'CLI 쓰는 법'을 적은 스킬
        var cliNote = Text(node, "/unity-cli = CLI 쓰는 법을 담은 스킬 · CLI 프로그램은 따로 설치", 15f, new Color(0.95f, 0.88f, 0.6f));
        cliNote.style.marginTop = 6f;
    }

    void BuildBox(VisualElement root, string key, string title, string body, float titleSize = 24f)
    {
        var node = NewNode(root, key);
        Text(node, title, titleSize, Ink).style.unityFontStyleAndWeight = FontStyle.Bold;
        Text(node, body, 17f, new Color(0.78f, 0.82f, 0.9f)).style.marginTop = 4f;
    }

    void BuildDetail(VisualElement root)
    {
        var panel = new VisualElement { pickingMode = PickingMode.Ignore };
        panel.style.position = Position.Absolute;
        panel.style.left = 40f;
        panel.style.top = 680f;
        panel.style.width = 1170f;
        panel.style.paddingLeft = panel.style.paddingRight = 20f;
        panel.style.paddingTop = panel.style.paddingBottom = 14f;
        panel.style.backgroundColor = new Color(0.06f, 0.07f, 0.09f, 0.9f);
        panel.style.borderLeftWidth = 8f;
        panel.style.borderLeftColor = Focus;
        Round(panel, 6f);
        root.Add(panel);

        titleLabel = Text(panel, "", 28f, Focus);
        titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        titleLabel.style.marginBottom = 8f;
        linesLabel = Text(panel, "", 21f, Ink);
        linesLabel.enableRichText = false;
        stepLabel = Text(panel, "", 16f, new Color(0.6f, 0.64f, 0.72f));
        stepLabel.style.marginTop = 10f;
    }

    VisualElement Row(VisualElement parent, string key)
    {
        var row = new VisualElement { name = key, pickingMode = PickingMode.Ignore };
        row.style.flexDirection = FlexDirection.Row;
        row.style.flexWrap = Wrap.Wrap;
        row.style.alignItems = Align.Center;
        row.style.paddingLeft = row.style.paddingRight = row.style.paddingTop = row.style.paddingBottom = 4f;
        Round(row, 6f);
        parent.Add(row);
        parts[key] = row;
        return row;
    }

    void Chip(VisualElement parent, string key, string text, float size, Color bg)
    {
        var chip = new Label(text) { pickingMode = PickingMode.Ignore, enableRichText = false };
        chip.style.fontSize = size;
        chip.style.color = Color.white;
        chip.style.backgroundColor = bg;
        chip.style.paddingLeft = chip.style.paddingRight = 8f;
        chip.style.paddingTop = chip.style.paddingBottom = 3f;
        chip.style.marginRight = chip.style.marginBottom = 5f;
        Round(chip, 5f);
        parent.Add(chip);
        if (key != null) parts[key] = chip;
    }

    static Label Text(VisualElement parent, string text, float size, Color color)
    {
        var label = new Label(text) { pickingMode = PickingMode.Ignore };
        label.style.fontSize = size;
        label.style.color = color;
        label.style.whiteSpace = WhiteSpace.Normal;
        label.style.marginLeft = label.style.marginRight = label.style.marginTop = label.style.marginBottom = 0f;
        label.style.paddingLeft = label.style.paddingRight = label.style.paddingTop = label.style.paddingBottom = 0f;
        parent.Add(label);
        return label;
    }

    static void Round(VisualElement e, float r)
    {
        e.style.borderTopLeftRadius = e.style.borderTopRightRadius = r;
        e.style.borderBottomLeftRadius = e.style.borderBottomRightRadius = r;
    }

    // 칸 사이 화살표 (강조된 화살표는 노란색 굵게)
    void DrawArrows(MeshGenerationContext context)
    {
        var painter = context.painter2D;
        foreach (var arrow in Arrows)
        {
            var from = NodeRects[arrow[0]];
            var to = NodeRects[arrow[1]];
            Vector2 start = new Vector2(from.xMax, from.center.y);
            Vector2 end = new Vector2(to.xMin, to.center.y);
            bool on = focused.Contains($"arrow.{arrow[0]}-{arrow[1]}");

            painter.strokeColor = on ? Focus : Line;
            painter.fillColor = on ? Focus : Line;
            painter.lineWidth = on ? 6f : 3f;
            painter.BeginPath();
            painter.MoveTo(start);
            painter.LineTo(end);
            painter.Stroke();

            Vector2 dir = (end - start).normalized;
            Vector2 side = new Vector2(-dir.y, dir.x);
            float head = on ? 18f : 13f;
            painter.BeginPath();
            painter.MoveTo(end);
            painter.LineTo(end - dir * head + side * head * 0.55f);
            painter.LineTo(end - dir * head - side * head * 0.55f);
            painter.ClosePath();
            painter.Fill();
        }
    }
}
