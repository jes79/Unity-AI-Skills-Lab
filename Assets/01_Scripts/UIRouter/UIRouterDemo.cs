using UnityEngine;
using UnityEngine.UIElements;

// 0301 UI 시스템 고르기 (/ui)
// 이 프로젝트에는 이미 UI Toolkit HUD(Hud.uxml + HudTheme.uss)가 있다. 요청은 "설정 화면 만들어줘"
// - A: 프로젝트를 보지 않고 옛 튜토리얼처럼 uGUI Canvas 를 새로 만든 설정 창 (씬의 SettingsCanvas_A)
//      → HUD 와 모양·글꼴이 다르고, 고정 픽셀 크기라 HUD 와 겹친다
// - B: /ui 가 먼저 UI 시스템을 확인 → UI Toolkit(SettingsWindow.uxml) + 같은 HudTheme.uss
public class UIRouterDemo : AbCompareScene
{
    [Header("원래 있던 HUD (UI Toolkit)")]
    [SerializeField] VisualTreeAsset hudTree;

    [Header("B · UI Toolkit 설정 화면")]
    [SerializeField] VisualTreeAsset settingsTree;

    [Header("A · uGUI 로 따로 만든 설정 창")]
    [SerializeField] GameObject settingsCanvasA;
    [SerializeField] UnityEngine.UI.Button closeButtonA;

    VisualElement settingsB;
    Label stackLabel;
    bool settingsOpen = true;   // 설정 화면이 열려 있는가 (HUD '설정' 버튼으로 다시 열기)

    protected override void BuildSceneUi()
    {
        var hud = hudTree.Instantiate();
        hud.pickingMode = PickingMode.Ignore;
        hud.style.position = Position.Absolute;
        hud.style.left = hud.style.top = hud.style.right = hud.style.bottom = 0f;
        Root.Add(hud);
        hud.Q<Button>("SettingsButton").clicked += OpenSettings;

        settingsB = settingsTree.Instantiate();
        settingsB.pickingMode = PickingMode.Ignore;
        settingsB.style.position = Position.Absolute;
        settingsB.style.left = settingsB.style.top = settingsB.style.right = settingsB.style.bottom = 0f;
        Root.Add(settingsB);
        settingsB.Q<Button>("CloseButton").clicked += CloseSettings;

        closeButtonA.onClick.AddListener(CloseSettings);

        // 지금 화면에 어떤 UI 시스템이 몇 개 섞여 있는지 (왼쪽 아래)
        var panel = AddPanel(Root, 150f, 860f, 1050f);
        stackLabel = AddText(panel, "", 20f, new Color(0.85f, 0.9f, 0.95f));
    }

    protected override void ShowSide(bool useSkill)
    {
        Refresh();
        stackLabel.text = useSkill
            ? "이 화면의 UI 시스템: UI Toolkit 하나 (HUD · 설정 화면 모두 UXML + HudTheme.uss)\n설정 화면 = SettingsWindow.uxml, HUD 와 같은 글꼴·색·둥근 모서리"
            : "이 화면의 UI 시스템: UI Toolkit(HUD) + uGUI Canvas(설정 창) 두 가지가 섞임\n설정 창 = Canvas · Image · Text · Slider, 고정 픽셀 크기(Constant Pixel Size)라 HUD 위에 겹침";
    }

    void OpenSettings()
    {
        settingsOpen = true;
        Refresh();
    }

    void CloseSettings()
    {
        settingsOpen = false;
        Refresh();
    }

    void Refresh()
    {
        settingsCanvasA.SetActive(settingsOpen && !UseSkill);
        settingsB.style.display = settingsOpen && UseSkill ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
