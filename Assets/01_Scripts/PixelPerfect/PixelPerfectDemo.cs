using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

// 0303 픽셀아트 흐림·떨림 (/2d-pixel-perfect)
// 같은 도트 그림(코드로 만든 PNG)을 가져오기 설정만 다르게 두 벌 들여왔다 (Assets/05_Sprites/Generated)
// - A: Bilinear 필터 · 압축 · PPU 13 (타일은 16 → 불일치) · Pixel Perfect Camera 끔 → 흐리고, 걸을 때 떨림
// - B: Point 필터 · 압축 없음 · PPU 16 맞춤 · Pixel Perfect Camera(URP 2D) 켬 → 선명하고 안정
public class PixelPerfectDemo : AbCompareScene
{
    [SerializeField] PixelWalker walker;
    [SerializeField] Sprite[] heroA = new Sprite[2];
    [SerializeField] Sprite[] heroB = new Sprite[2];
    [SerializeField] SpriteRenderer[] tiles;
    [SerializeField] Sprite tileA;
    [SerializeField] Sprite tileB;
    [SerializeField] Camera viewCamera;
    [SerializeField] PixelPerfectCamera pixelCamera;
    [SerializeField] float orthoSizeA = 4.1f;    // A 카메라 크기: 도트 1칸이 화면 픽셀 정수 배로 떨어지지 않는 값 (B 는 Pixel Perfect Camera 가 정함)

    Label infoLabel;

    protected override void BuildSceneUi()
    {
        var panel = AddPanel(Root, 150f, 880f, 1050f);
        infoLabel = AddText(panel, "", 20f, new Color(0.85f, 0.9f, 0.95f));
    }

    protected override void ShowSide(bool useSkill)
    {
        var hero = useSkill ? heroB : heroA;
        walker.SetFrames(hero[0], hero[1]);
        foreach (var tile in tiles)
            tile.sprite = useSkill ? tileB : tileA;

        pixelCamera.enabled = useSkill;
        if (!useSkill)
            viewCamera.orthographicSize = orthoSizeA;

        // 실제로 들어온 그림 설정을 그대로 읽어서 보여 준다
        var texture = hero[0].texture;
        infoLabel.text =
            $"캐릭터 그림: 필터 {texture.filterMode} · 형식 {texture.format} · PPU {hero[0].pixelsPerUnit:0}   /   타일 PPU {(useSkill ? tileB : tileA).pixelsPerUnit:0}\n" +
            $"Pixel Perfect Camera: {(useSkill ? $"켬 (Assets PPU {pixelCamera.assetsPPU}, 기준 해상도 {pixelCamera.refResolutionX}x{pixelCamera.refResolutionY}, {pixelCamera.gridSnapping})" : "끔 (카메라가 소수점 위치로 따라감)")}";
    }
}
