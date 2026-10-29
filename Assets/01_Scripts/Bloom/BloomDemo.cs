using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

// 0401 블룸이 안 보임 (/urp-postprocessing)
// 씬에는 처음부터 Global Volume 이 있고 그 프로필에 Bloom 이 켜져 있다 (A/B 같음)
// - A: 카메라의 Rendering > Post Processing 이 꺼져 있음 → Volume 효과가 이 카메라에 안 그려져 빛 번짐 없음
// - B: 같은 카메라의 Post Processing 켬 → 밝은 구슬 주변으로 빛이 번짐
public class BloomDemo : AbCompareScene
{
    [SerializeField] Camera viewCamera;
    [SerializeField] Volume globalVolume;
    [SerializeField] Transform[] orbs;
    [SerializeField] float bobHeight = 0.25f;   // 구슬이 위아래로 떠다니는 높이
    [SerializeField] float bobSpeed = 1.2f;

    UniversalAdditionalCameraData cameraData;
    Vector3[] startPositions;
    Label infoLabel;

    protected override void BuildSceneUi()
    {
        cameraData = viewCamera.GetUniversalAdditionalCameraData();
        startPositions = new Vector3[orbs.Length];
        for (int i = 0; i < orbs.Length; i++)
            startPositions[i] = orbs[i].position;

        var panel = AddPanel(Root, 150f, 860f, 1050f);
        infoLabel = AddText(panel, "", 20f, new Color(0.85f, 0.9f, 0.95f));
    }

    protected override void ShowSide(bool useSkill)
    {
        cameraData.renderPostProcessing = useSkill;

        // 실제 Volume·카메라 값을 읽어서 보여 준다
        string bloomText = "Bloom 없음";
        if (globalVolume.sharedProfile != null && globalVolume.sharedProfile.TryGet(out Bloom bloom) && bloom.active)
            bloomText = $"Bloom 있음 (Intensity {bloom.intensity.value:0.0}, Threshold {bloom.threshold.value:0.0})";
        infoLabel.text =
            $"Global Volume ({globalVolume.sharedProfile.name}): {bloomText}\n" +
            $"Main Camera › Rendering › Post Processing: {(cameraData.renderPostProcessing ? "켬" : "꺼짐")}";
    }

    protected override void Update()
    {
        base.Update();
        for (int i = 0; i < orbs.Length; i++)
            orbs[i].position = startPositions[i] + Vector3.up * Mathf.Sin(Time.time * bobSpeed + i) * bobHeight;
    }
}
