using UnityEngine;
using UnityEngine.UIElements;

// 0302 버튼 전환이 툭 돌아감 (/ui-uitk)
// 같은 버튼 3개(Buttons.uxml)에 USS 파일만 바꿔 끼운다
// - A: ButtonTransition_A.uss — transition 을 .lab-button:hover 에만 선언 → 올릴 땐 부드럽게 커지고, 뗄 땐 툭 돌아감
// - B: ButtonTransition_B.uss — transition 을 기본 클래스 .lab-button 에 선언 → 올릴 때도 뗄 때도 부드러움
public class ButtonTransitionDemo : AbCompareScene
{
    [Header("USS 두 벌 (Assets/02_UI/ButtonTransition)")]
    [SerializeField] StyleSheet styleA;
    [SerializeField] StyleSheet styleB;

    [Header("화면에 보여 줄 USS 원문의 버튼 규칙 부분 (씬을 만들 때 파일에서 잘라 넣음)")]
    [SerializeField, TextArea(5, 30)] string sourceA;
    [SerializeField, TextArea(5, 30)] string sourceB;

    Label fileLabel, sourceLabel;

    protected override void BuildSceneUi()
    {
        // 지금 끼운 USS 원문을 왼쪽 아래에 (강의 ③ 단계 카드·말풍선이 있는 오른쪽은 비워 둠)
        var panel = AddPanel(Root, 150f, 520f, 1050f);
        fileLabel = AddText(panel, "", 20f, new Color(1f, 0.83f, 0.47f));
        fileLabel.style.marginBottom = 6f;
        sourceLabel = AddText(panel, "", 17f, new Color(0.85f, 0.9f, 0.95f));
        sourceLabel.enableRichText = false;              // USS 원문 그대로 (꺾쇠·중괄호를 글자로)
        sourceLabel.style.whiteSpace = WhiteSpace.PreWrap; // 들여쓰기 공백도 그대로
    }

    protected override void ShowSide(bool useSkill)
    {
        Root.styleSheets.Remove(useSkill ? styleA : styleB);
        Root.styleSheets.Add(useSkill ? styleB : styleA);

        fileLabel.text = useSkill
            ? "지금 끼운 USS: ButtonTransition_B.uss  (transition 이 기본 클래스 .lab-button 에 있음)"
            : "지금 끼운 USS: ButtonTransition_A.uss  (transition 이 .lab-button:hover 에만 있음)";
        sourceLabel.text = useSkill ? sourceB : sourceA;
    }
}
