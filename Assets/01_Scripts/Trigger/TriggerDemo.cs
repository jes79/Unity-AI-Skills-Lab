using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// 0403 트리거가 안 불림 (/physics-3d-collision)
// 플레이어 큐브를 transform.position 으로 좌우로 움직여 동전 영역(트리거) 3개를 지나가게 한다
// - A: 플레이어도 동전도 Collider 만 있고 Rigidbody 가 없음 → 물리 엔진이 둘 다 '움직이지 않는 물체'로 봐서 OnTriggerEnter 가 안 불림
// - B: 움직이는 플레이어에 Rigidbody(Is Kinematic)를 붙임 → 불림
public class TriggerDemo : AbCompareScene
{
    [SerializeField] Transform player;
    [SerializeField] float speed = 3f;      // 플레이어 이동 빠르기 (m/s)
    [SerializeField] float range = 5.5f;    // 가운데에서 좌우로 가는 거리

    float direction = 1f;
    int calls;
    string lastCoin = "-";
    Label infoLabel, countLabel;

    protected override void BuildSceneUi()
    {
        var panel = AddPanel(Root, 150f, 820f, 1050f);
        infoLabel = AddText(panel, "", 20f, new Color(0.85f, 0.9f, 0.95f));
        countLabel = AddText(panel, "", 30f, Color.white);
        countLabel.style.marginTop = 6f;
    }

    protected override void ShowSide(bool useSkill)
    {
        var body = player.GetComponent<Rigidbody>();
        if (useSkill && body == null)
        {
            body = player.gameObject.AddComponent<Rigidbody>();
            body.isKinematic = true;   // 물리로 밀리지 않고, 내가 옮기는 대로 움직임
            body.useGravity = false;
        }
        else if (!useSkill && body != null)
        {
            Destroy(body);
        }

        calls = 0;
        lastCoin = "-";
        infoLabel.text = useSkill
            ? "Player: Box Collider + Rigidbody (Is Kinematic)  ·  동전: Box Collider (Is Trigger)\n움직이는 쪽에 Rigidbody 가 있어 트리거 검사를 한다"
            : "Player: Box Collider 만 (Rigidbody 없음)  ·  동전: Box Collider (Is Trigger)\n둘 다 Rigidbody 가 없으면 트리거 검사를 하지 않는다";
        RefreshCount();
    }

    public void ReportTrigger(string coinName)
    {
        calls++;
        lastCoin = coinName;
        RefreshCount();
    }

    void RefreshCount()
    {
        countLabel.text = $"OnTriggerEnter 호출 {calls}번   ·   마지막: {lastCoin}   (R = 0으로)";
    }

    protected override void Update()
    {
        base.Update();
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            calls = 0;
            RefreshCount();
        }

        // 물리를 쓰지 않고 위치를 직접 바꾸는 흔한 이동 코드
        var position = player.position;
        position.x += direction * speed * Time.deltaTime;
        if (Mathf.Abs(position.x) > range)
        {
            position.x = Mathf.Sign(position.x) * range;
            direction = -direction;
        }
        player.position = position;
    }
}
