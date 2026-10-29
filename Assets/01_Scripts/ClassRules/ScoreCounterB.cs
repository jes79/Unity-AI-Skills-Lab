using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// Space 를 누를 때마다 점수를 올리고 UI Toolkit 글자로 보여 주는 점수판
public class ScoreCounterB : MonoBehaviour
{
    [SerializeField] UIDocument scoreDocument;
    [SerializeField] StyleSheet scoreStyle;
    [SerializeField] ScorePop coinPop;
    [SerializeField] int pointsPerPress = 10;
    [SerializeField] int goalScore = 100;

    int currentScore;
    Label scoreLabel;

    void OnEnable()
    {
        scoreLabel = new Label();
        scoreLabel.AddToClassList("score-label");
        scoreDocument.rootVisualElement.styleSheets.Add(scoreStyle);
        scoreDocument.rootVisualElement.Add(scoreLabel);
        RefreshLabel();
    }

    void OnDisable()
    {
        scoreLabel?.RemoveFromHierarchy();
    }

    void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.spaceKey.wasPressedThisFrame) return;

        currentScore += pointsPerPress;
        coinPop.Play();
        RefreshLabel();
    }

    void RefreshLabel()
    {
        scoreLabel.text = currentScore >= goalScore
            ? $"점수 {currentScore}  ·  목표 달성!"
            : $"점수 {currentScore} / {goalScore}";
    }
}
