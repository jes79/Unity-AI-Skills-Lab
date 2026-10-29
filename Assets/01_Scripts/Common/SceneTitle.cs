using UnityEngine;
using UnityEngine.UIElements;

// 화면 위 가운데 제목 + 설명 한 줄 (허브 씬 LectureHub 에서 씀)
[RequireComponent(typeof(UIDocument))]
public class SceneTitle : MonoBehaviour
{
    [SerializeField] string title = "제목";
    [SerializeField] string description = "";
    [SerializeField] float titleSize = 56f;
    [SerializeField] float descriptionSize = 26f;

    void OnEnable()
    {
        var box = new VisualElement { pickingMode = PickingMode.Ignore };
        box.style.position = Position.Absolute;
        box.style.left = box.style.right = 0f;
        box.style.top = 40f;
        box.style.alignItems = Align.Center;
        GetComponent<UIDocument>().rootVisualElement.Add(box);

        var head = new Label(title) { pickingMode = PickingMode.Ignore };
        head.style.fontSize = titleSize;
        head.style.color = (Color)new Color32(235, 238, 245, 255);
        head.style.unityFontStyleAndWeight = FontStyle.Bold;
        box.Add(head);

        var sub = new Label(description) { pickingMode = PickingMode.Ignore };
        sub.style.fontSize = descriptionSize;
        sub.style.color = (Color)new Color32(150, 156, 170, 255);
        box.Add(sub);
    }
}
