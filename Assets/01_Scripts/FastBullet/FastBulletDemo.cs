using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

// 0402 빠른 총알이 벽을 뚫음 (/physics-3d-collision)
// 두께 0.1m 벽에 총알을 쏜다 (Space). 물리는 0.02초마다 한 번 계산된다 (Fixed Timestep)
// - A: Rigidbody.collisionDetectionMode = Discrete → 한 번에 벽 두께보다 멀리 가면 벽을 건너뛰어 뚫고 지나감
// - B: ContinuousDynamic (연속 충돌) → 지나온 길 전체를 검사해서 빨라도 맞음
public class FastBulletDemo : AbCompareScene
{
    [SerializeField] FastBullet bulletTemplate;    // 꺼 둔 총알 하나를 복사해서 쏜다
    [SerializeField] Transform muzzle;             // 총구 위치
    [SerializeField] Transform wall;
    [SerializeField] GameObject hitMarkTemplate;   // 맞은 자리 표시
    [SerializeField] float[] speeds = { 15f, 30f, 60f, 100f, 150f, 250f };  // ↑ / ↓ 로 고르는 총알 속도 (m/s)
    [SerializeField] int speedIndex = 4;                                     // 처음 속도 = 150 m/s

    float CurrentSpeed => speeds[speedIndex];

    int hits, passes;
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
        hits = passes = 0;
        RefreshText();
    }

    protected override void Update()
    {
        base.Update();
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.spaceKey.wasPressedThisFrame) Fire();
        if (keyboard.upArrowKey.wasPressedThisFrame) ChangeSpeed(+1);
        if (keyboard.downArrowKey.wasPressedThisFrame) ChangeSpeed(-1);
    }

    void ChangeSpeed(int step)
    {
        speedIndex = Mathf.Clamp(speedIndex + step, 0, speeds.Length - 1);
        RefreshText();
    }

    void Fire()
    {
        var bullet = Instantiate(bulletTemplate, muzzle.position, Quaternion.identity);
        bullet.gameObject.SetActive(true);
        var mode = UseSkill ? CollisionDetectionMode.ContinuousDynamic : CollisionDetectionMode.Discrete;
        bullet.Fire(this, CurrentSpeed, mode, wall.position.x + wall.localScale.x * 0.5f + 0.5f);
    }

    public void ReportHit(Vector3 point)
    {
        hits++;
        var mark = Instantiate(hitMarkTemplate, point, Quaternion.identity);
        mark.SetActive(true);
        Destroy(mark, 3f);
        RefreshText();
    }

    public void ReportPass()
    {
        passes++;
        RefreshText();
    }

    void RefreshText()
    {
        float step = Time.fixedDeltaTime;
        infoLabel.text =
            $"총알 Rigidbody › Collision Detection: {(UseSkill ? "Continuous Dynamic" : "Discrete")}\n" +
            $"속도 {CurrentSpeed:0} m/s · 물리 한 번({step:0.00}초)에 {CurrentSpeed * step:0.0} m 이동 · 벽 두께 {wall.localScale.x:0.0} m   (Space 발사 · ↑↓ 속도)";
        countLabel.text = $"맞음 {hits}   ·   뚫고 지나감 {passes}";
    }
}
