using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// 비교 씬 공통 부품: 키 1 = A, 2 = B, Tab = A ↔ B
// 화면 위에 지금 어느 쪽인지 큰 글씨 + 원인 한 줄을 보여 준다
// - A = 스킬 없이 흔히 나오는 결과(재현), B = 스킬을 쓴 결과(재현)
//   둘 다 '우리가 재현한 것'이지 AI 가 실제로 만든 결과가 아니다 (화면에도 그렇게 적는다)
// - 씬마다 이 클래스를 상속해서 ShowSide(useSkill) 에 A/B 차이를 실제로 만든다
[RequireComponent(typeof(UIDocument))]
public abstract class AbCompareScene : MonoBehaviour
{
    public const string TitleA = "A · 스킬 없이 흔히 나오는 결과(재현)";
    public const string TitleB = "B · 스킬을 쓴 결과(재현)";

    static readonly Color ColorA = new Color32(255, 140, 90, 255);
    static readonly Color ColorB = new Color32(110, 220, 140, 255);

    [Header("화면 위 안내")]
    [SerializeField] string sceneTitle = "0000 · 제목";       // 예: 0302 · 버튼 전환이 툭 돌아감
    [SerializeField] string skillName = "/skill";              // 예: /ui-uitk
    [SerializeField] string request = "";                     // AI 에게 한 요청 (예: 버튼에 마우스를 올리면 커지게 해줘)
    [SerializeField, TextArea] string causeA = "";             // A 가 왜 그렇게 되는지 한 줄
    [SerializeField, TextArea] string causeB = "";             // B 가 무엇을 바꿨는지 한 줄
    [SerializeField] bool startWithSkill;                      // 처음에 B 로 시작할지 (기본은 A 부터)

    // 지금 B(스킬을 쓴 결과)를 보여 주는 중인가
    protected bool UseSkill { get; private set; }

    // 씬마다 자기 UI 를 붙이는 곳 (배너와 같은 UIDocument)
    protected VisualElement Root { get; private set; }

    Label sideLabel, causeLabel;
    VisualElement banner;

    protected virtual void OnEnable()
    {
        Root = GetComponent<UIDocument>().rootVisualElement;
        BuildSceneUi();   // 씬마다 자기 UI 먼저 (배너가 그 위에 그려지게)
        BuildBanner();
        SetSide(startWithSkill);
    }

    // 씬마다 자기 UI 를 Root 에 붙이는 곳 (ShowSide 보다 먼저 한 번 불림)
    protected virtual void BuildSceneUi() { }

    protected virtual void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;
        if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame) SetSide(false);
        else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame) SetSide(true);
        else if (keyboard.tabKey.wasPressedThisFrame) SetSide(!UseSkill);
    }

    public void SetSide(bool useSkill)
    {
        UseSkill = useSkill;
        sideLabel.text = useSkill ? TitleB : TitleA;
        sideLabel.style.color = useSkill ? ColorB : ColorA;
        banner.style.borderLeftColor = useSkill ? ColorB : ColorA;
        causeLabel.text = "원인 · " + (useSkill ? causeB : causeA);
        ShowSide(useSkill);
    }

    // A(false) / B(true) 차이를 실제로 만든다
    protected abstract void ShowSide(bool useSkill);

    // 배너: 왼쪽 위 '목록' 버튼과 오른쪽 위 강의 진행 막대를 피해서 위쪽 가운데-왼쪽에 둔다
    void BuildBanner()
    {
        banner = new VisualElement { name = "AbBanner", pickingMode = PickingMode.Ignore };
        banner.style.position = Position.Absolute;
        banner.style.left = 150f;
        banner.style.top = 14f;
        banner.style.width = 1050f;
        banner.style.paddingLeft = 18f;
        banner.style.paddingRight = 18f;
        banner.style.paddingTop = 10f;
        banner.style.paddingBottom = 12f;
        banner.style.backgroundColor = new Color(0.06f, 0.07f, 0.09f, 0.88f);
        banner.style.borderLeftWidth = 8f;
        banner.style.borderTopRightRadius = banner.style.borderBottomRightRadius = 6f;
        Root.Add(banner);

        AddText(banner, $"{sceneTitle}   ·   스킬 {skillName}", 20f, new Color(0.75f, 0.8f, 0.9f));
        if (!string.IsNullOrEmpty(request))
            AddText(banner, $"요청: \"{request}\"", 18f, new Color(0.95f, 0.88f, 0.6f));
        sideLabel = AddText(banner, "", 40f, Color.white);
        sideLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        sideLabel.style.marginTop = 2f;
        causeLabel = AddText(banner, "", 21f, new Color(0.92f, 0.93f, 0.96f));
        AddText(banner, "1 = A   ·   2 = B   ·   Tab = A ↔ B", 16f, new Color(0.6f, 0.64f, 0.72f)).style.marginTop = 4f;
    }

    // 씬 UI 용 글자 하나 (자식 클래스도 같이 씀)
    protected static Label AddText(VisualElement parent, string text, float size, Color color)
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

    // 씬 UI 용 반투명 상자 (위치는 1920x1080 기준 픽셀)
    protected static VisualElement AddPanel(VisualElement parent, float left, float top, float width)
    {
        var panel = new VisualElement { pickingMode = PickingMode.Ignore };
        panel.style.position = Position.Absolute;
        panel.style.left = left;
        panel.style.top = top;
        panel.style.width = width;
        panel.style.paddingLeft = panel.style.paddingRight = 16f;
        panel.style.paddingTop = panel.style.paddingBottom = 12f;
        panel.style.backgroundColor = new Color(0.06f, 0.07f, 0.09f, 0.82f);
        panel.style.borderTopLeftRadius = panel.style.borderTopRightRadius = 6f;
        panel.style.borderBottomLeftRadius = panel.style.borderBottomRightRadius = 6f;
        parent.Add(panel);
        return panel;
    }
}
