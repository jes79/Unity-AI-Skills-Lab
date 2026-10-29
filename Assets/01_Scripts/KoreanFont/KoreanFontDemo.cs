using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

// 0304 한글이 네모로 나옴 (/optimize-text-mesh-pro)
// 같은 TextMeshPro 글자들에 글꼴 에셋만 바꿔 끼운다
// - A: 기본 글꼴 LiberationSans SDF 만 → 한글 글자가 없어서 □(빠진 글자 표시)로 나옴
// - B: LiberationSans SDF (KR Fallback) — 같은 글꼴에 Noto Sans KR 동적(Dynamic) 글꼴 에셋을 예비(fallback)로 붙임
//      → 영문·숫자는 원래 글꼴, 한글은 Noto Sans KR 에서 필요할 때 채워 넣음 (Assets/06_Fonts, OFL 라이선스)
public class KoreanFontDemo : AbCompareScene
{
    [SerializeField] TMP_Text[] texts;
    [SerializeField] TMP_FontAsset fontA;   // LiberationSans SDF (TMP 기본)
    [SerializeField] TMP_FontAsset fontB;   // LiberationSans SDF (KR Fallback)

    Label infoLabel;

    protected override void BuildSceneUi()
    {
        var panel = AddPanel(Root, 150f, 800f, 1050f);
        infoLabel = AddText(panel, "", 20f, new Color(0.85f, 0.9f, 0.95f));
    }

    protected override void ShowSide(bool useSkill)
    {
        var font = useSkill ? fontB : fontA;
        foreach (var text in texts)
            text.font = font;

        // 지금 글꼴 에셋의 예비 글꼴 목록을 그대로 읽어서 보여 준다
        var fallbacks = new StringBuilder();
        foreach (var fallback in font.fallbackFontAssetTable)
        {
            if (fallback == null) continue;
            if (fallbacks.Length > 0) fallbacks.Append(", ");
            fallbacks.Append(fallback.name).Append(" (").Append(fallback.atlasPopulationMode).Append(')');
        }
        infoLabel.text =
            $"TextMeshPro 글꼴 에셋: {font.name}\n" +
            $"Fallback Font Assets: {(fallbacks.Length > 0 ? fallbacks.ToString() : "(없음)")}";
    }
}
