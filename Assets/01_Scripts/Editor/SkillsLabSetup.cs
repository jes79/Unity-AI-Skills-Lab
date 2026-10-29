using System.IO;
using UnityEditor;
using UnityEngine;

// 처음 한 번: TextMeshPro 필수 리소스(TMP Settings · LiberationSans SDF · 셰이더) 가져오기
// 메뉴 Tools > Skills Lab > Import TMP Essentials (Window > TextMeshPro > Import TMP Essential Resources 와 같은 패키지)
// 배치 모드에서는 가져오기가 끝나기 전에 종료되므로 명령줄 -importPackage 로 가져온다:
//   Unity -batchmode -quit -projectPath . -importPackage "Library/PackageCache/com.unity.ugui@<해시>/Package Resources/TMP Essential Resources.unitypackage"
// (이 저장소에는 이미 Assets/TextMesh Pro 가 들어 있어 보통은 할 필요 없음)
public static class SkillsLabSetup
{
    const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

    [MenuItem("Tools/Skills Lab/Import TMP Essentials")]
    public static void ImportTmpEssentials()
    {
        if (File.Exists(TmpSettingsPath))
        {
            Debug.Log("[SkillsLabSetup] TMP Essential Resources 가 이미 있음");
            return;
        }
        // TMP 는 uGUI 패키지 안에 있다 (패키지 실제 폴더는 Library/PackageCache 아래)
        var ugui = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.ugui");
        string package = Path.Combine(ugui.resolvedPath, "Package Resources", "TMP Essential Resources.unitypackage");
        UnityEditor.AssetPackage.Package.Import(package, false);
        AssetDatabase.Refresh();
        Debug.Log($"[SkillsLabSetup] TMP Essential Resources 가져옴 ({package}): {File.Exists(TmpSettingsPath)}");
    }
}
