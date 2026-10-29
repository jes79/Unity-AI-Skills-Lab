using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using UitkFontAsset = UnityEngine.TextCore.Text.FontAsset;

// AI 스킬 랩 예제 씬을 코드로 만들 때 쓰는 에디터 전용 도구 모음
// - 공용 에셋 준비: 한글 글꼴(Noto Sans KR → UI Toolkit · TMP 글꼴 에셋), 씬용 PanelSettings, URP 2D 렌더러
// - 씬 기본 구성(카메라 · 빛 · EventSystem · UIDocument), 머티리얼, 직렬화 필드 연결
public static class SkillsLabSceneKit
{
    public const string SceneFolder = "Assets/00_Scenes";
    public const string UiFolder = "Assets/02_UI";
    public const string MaterialFolder = "Assets/04_Materials";
    public const string SpriteFolder = "Assets/05_Sprites/Generated";
    public const string FontFolder = "Assets/06_Fonts";

    public const string NotoFontPath = FontFolder + "/NotoSansKR-Regular.otf";
    public const string NotoUitkPath = FontFolder + "/NotoSansKR-Regular_UITK.asset";
    public const string NotoTmpPath = FontFolder + "/NotoSansKR-Regular SDF.asset";
    public const string LiberationPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
    public const string LiberationKrPath = FontFolder + "/LiberationSans SDF (KR Fallback).asset";

    public const string ThemePath = UiFolder + "/SkillsLabTheme.tss";
    public const string TextSettingsPath = UiFolder + "/SkillsLabTextSettings.asset";
    public const string PanelSettingsPath = UiFolder + "/SkillsLabPanelSettings.asset";
    public const string Renderer2DPath = "Assets/Settings/Pixel2D_Renderer.asset";

    public static readonly Color BgColor = new Color32(24, 26, 33, 255);

    // ───────────────────────── 공용 에셋 ─────────────────────────

    public static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }

    // 모든 씬이 쓰는 에셋을 (없으면) 만든다
    public static void EnsureSharedAssets()
    {
        EnsureFonts();
        EnsurePanelSettings();
        EnsureRenderer2D();
        AssetDatabase.SaveAssets();
    }

    public static Font NotoFont => AssetDatabase.LoadAssetAtPath<Font>(NotoFontPath);
    public static UitkFontAsset NotoUitk => AssetDatabase.LoadAssetAtPath<UitkFontAsset>(NotoUitkPath);
    public static TMP_FontAsset NotoTmp => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(NotoTmpPath);
    public static TMP_FontAsset Liberation => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(LiberationPath);
    public static TMP_FontAsset LiberationKr => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(LiberationKrPath);
    public static PanelSettings ScenePanelSettings => AssetDatabase.LoadAssetAtPath<PanelSettings>(PanelSettingsPath);

    // Noto Sans KR(OFL) 하나로 UI Toolkit 글꼴 에셋 · TMP 글꼴 에셋을 동적(Dynamic)으로 만든다
    // 동적 = 필요한 글자만 실행 중에 글꼴 파일에서 그려 넣음 (한글 11,172자를 미리 굽지 않아도 됨)
    static void EnsureFonts()
    {
        var font = NotoFont;
        if (font == null) throw new InvalidOperationException($"{NotoFontPath} 가 없습니다 (Noto Sans KR, OFL)");

        if (NotoUitk == null)
        {
            var asset = UitkFontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024);
            SaveFontAsset(asset, NotoUitkPath, asset.atlasTextures, asset.material);
        }

        if (NotoTmp == null)
        {
            var asset = TMP_FontAsset.CreateFontAsset(font, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024);
            SaveFontAsset(asset, NotoTmpPath, asset.atlasTextures, asset.material);
        }

        // 0304 B: TMP 기본 글꼴(LiberationSans SDF)을 복사해서 예비 글꼴에 Noto Sans KR 을 붙인 것
        // (원본을 고치면 A 화면도 같이 바뀌므로 복사본에 붙인다)
        if (LiberationKr == null)
        {
            if (Liberation == null) throw new InvalidOperationException($"{LiberationPath} 가 없습니다 (TMP Essential Resources 먼저)");
            AssetDatabase.CopyAsset(LiberationPath, LiberationKrPath);
            var copy = LiberationKr;
            copy.fallbackFontAssetTable = new List<TMP_FontAsset>(Liberation.fallbackFontAssetTable) { NotoTmp };
            EditorUtility.SetDirty(copy);
        }
    }

    static void SaveFontAsset(ScriptableObject asset, string path, Texture2D[] atlases, Material material)
    {
        string baseName = Path.GetFileNameWithoutExtension(path);
        asset.name = baseName;
        AssetDatabase.CreateAsset(asset, path);
        for (int i = 0; i < atlases.Length; i++)
        {
            atlases[i].name = $"{baseName} Atlas{(i > 0 ? " " + i : "")}";
            AssetDatabase.AddObjectToAsset(atlases[i], asset);
        }
        material.name = $"{baseName} Material";
        AssetDatabase.AddObjectToAsset(material, asset);
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();
    }

    // 예제 씬 UI Toolkit 패널 설정: 1920x1080 기준, 기본 글꼴 = Noto Sans KR (테마 파일 :root)
    // 강의 화면(LecturePanelSettings, 정렬 100)보다 아래(정렬 0)에 그린다
    static void EnsurePanelSettings()
    {
        EnsureFolder(UiFolder);

        // 테마: 기본 테마 + 글꼴만 Noto Sans KR (글꼴 에셋이 생긴 다음에 써야 경로를 찾는다)
        string theme = "@import url(\"unity-theme://default\");\n\n" +
                       "/* 예제 씬 기본 글꼴: Noto Sans KR (Assets/06_Fonts, OFL) */\n" +
                       ":root {\n    -unity-font-definition: url(\"/" + NotoUitkPath + "\");\n}\n";
        if (!File.Exists(ThemePath) || File.ReadAllText(ThemePath) != theme)
        {
            File.WriteAllText(ThemePath, theme);
            AssetDatabase.ImportAsset(ThemePath, ImportAssetOptions.ForceUpdate);
        }

        // 글자 설정: 테마 글꼴에 없는 글자는 Noto Sans KR 에서 찾는다 (예비 글꼴)
        var textSettings = AssetDatabase.LoadAssetAtPath<PanelTextSettings>(TextSettingsPath);
        if (textSettings == null)
        {
            textSettings = ScriptableObject.CreateInstance<PanelTextSettings>();
            textSettings.fallbackFontAssets = new List<UitkFontAsset> { NotoUitk };
            AssetDatabase.CreateAsset(textSettings, TextSettingsPath);
        }

        if (ScenePanelSettings == null)
        {
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.themeStyleSheet = AssetDatabase.LoadAssetAtPath<ThemeStyleSheet>(ThemePath);
            settings.textSettings = textSettings;
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1920, 1080);
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match = 0.5f;
            settings.sortingOrder = 0;
            AssetDatabase.CreateAsset(settings, PanelSettingsPath);
        }
    }

    // 0303 Pixel Perfect Camera 는 URP 2D 렌더러에서만 돈다 → 2D 렌더러를 만들어 URP 에셋 렌더러 목록에 더한다
    // 돌려주는 값 = 카메라가 고를 렌더러 번호
    public static int EnsureRenderer2D()
    {
        var renderer = AssetDatabase.LoadAssetAtPath<Renderer2DData>(Renderer2DPath);
        if (renderer == null)
        {
            // URP 메뉴(Create > Rendering > URP 2D Renderer)가 부르는 함수와 같은 것 (internal 이라 리플렉션으로 부름)
            var menus = Type.GetType("UnityEditor.Rendering.Universal.Renderer2DMenus, Unity.RenderPipelines.Universal.Editor", true);
            var create = menus.GetMethod("CreateRendererAsset", BindingFlags.NonPublic | BindingFlags.Static);
            var parameters = create.GetParameters();
            object type = Enum.Parse(parameters[1].ParameterType, "_2DRenderer");
            AssetDatabase.DeleteAsset(Renderer2DPath); // 다른 종류로 잘못 만든 파일이 있으면 지움
            renderer = (Renderer2DData)create.Invoke(null, new object[] { Renderer2DPath, type, false, "Renderer" });
        }

        int index = -1;
        foreach (string guid in AssetDatabase.FindAssets("t:UniversalRenderPipelineAsset", new[] { "Assets" })) // 패키지 안 견본 에셋은 건드리지 않음
        {
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(AssetDatabase.GUIDToAssetPath(guid));
            var so = new SerializedObject(pipeline);
            var list = so.FindProperty("m_RendererDataList");
            int found = -1;
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).objectReferenceValue == renderer) found = i;
            if (found < 0)
            {
                found = list.arraySize;
                list.arraySize++;
                list.GetArrayElementAtIndex(found).objectReferenceValue = renderer;
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            if (index >= 0 && index != found)
                Debug.LogWarning($"[SkillsLab] {pipeline.name} 의 2D 렌더러 번호({found})가 다른 URP 에셋({index})과 다릅니다");
            index = Mathf.Max(index, found);
        }
        return index;
    }

    // ───────────────────────── 씬 ─────────────────────────

    // 빈 씬 + 카메라 + (3D 면) 빛 + EventSystem. 카메라를 돌려줌
    public static Camera NewScene(Color background, bool orthographic, Vector3 cameraPosition, Vector3 lookAt, float size = 5f)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.38f, 0.4f, 0.45f);
        RenderSettings.skybox = null;

        var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
        var camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = background;
        camera.orthographic = orthographic;
        if (orthographic) camera.orthographicSize = size;
        else camera.fieldOfView = size;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 200f;
        cameraObject.transform.position = cameraPosition;
        if (lookAt != cameraPosition) cameraObject.transform.LookAt(lookAt);
        camera.GetUniversalAdditionalCameraData().renderPostProcessing = false;

        if (!orthographic)
        {
            var lightObject = new GameObject("Directional Light");
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // UI Toolkit · uGUI 둘 다 이 EventSystem 으로 입력을 받는다 (Input System 용 입력 모듈)
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        return camera;
    }

    // 허브처럼 제목만 있는 씬 (LectureHubBuilder 가 씀)
    public static void NewScene(string title, string description)
    {
        NewScene(BgColor, true, new Vector3(0f, 0f, -10f), new Vector3(0f, 0f, -10f));
        var document = NewUiDocument("Title");
        var sceneTitle = document.gameObject.AddComponent<SceneTitle>();
        Set(sceneTitle, "title", title);
        Set(sceneTitle, "description", description);
    }

    public static UIDocument NewUiDocument(string name, VisualTreeAsset tree = null)
    {
        var go = new GameObject(name);
        var document = go.AddComponent<UIDocument>();
        document.panelSettings = ScenePanelSettings;
        document.visualTreeAsset = tree;
        return document;
    }

    public static void SaveScene(string sceneName)
    {
        EnsureFolder(SceneFolder);
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), $"{SceneFolder}/{sceneName}.unity");
        Debug.Log($"[SkillsLab] {SceneFolder}/{sceneName}.unity 저장");
    }

    // ───────────────────────── 오브젝트 · 머티리얼 ─────────────────────────

    public static GameObject Primitive(PrimitiveType type, string name, Vector3 position, Vector3 scale, Material material, bool keepCollider = true)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.position = position;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = material;
        if (!keepCollider) Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    // URP Lit 머티리얼 (emission 이 0 보다 크면 HDR 로 빛나는 색)
    public static Material LitMaterial(string folder, string name, Color color, float emission = 0f)
    {
        string path = $"{MaterialFolder}/{folder}/{name}.mat";
        var material = LoadOrCreateMaterial(path, "Universal Render Pipeline/Lit");
        material.SetColor("_BaseColor", color);
        material.SetFloat("_Smoothness", 0.3f);
        if (emission > 0f)
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * emission);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
        EditorUtility.SetDirty(material);
        return material;
    }

    public static Material UnlitMaterial(string folder, string name, Color color, string shader = "Universal Render Pipeline/Unlit")
    {
        string path = $"{MaterialFolder}/{folder}/{name}.mat";
        var material = LoadOrCreateMaterial(path, shader);
        material.SetColor("_BaseColor", color);
        EditorUtility.SetDirty(material);
        return material;
    }

    static Material LoadOrCreateMaterial(string path, string shaderName)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        var shader = Shader.Find(shaderName);
        if (shader == null) throw new InvalidOperationException($"셰이더 {shaderName} 를 찾지 못함");
        if (material == null)
        {
            EnsureFolder(Path.GetDirectoryName(path).Replace('\\', '/'));
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }
        else
        {
            material.shader = shader;
        }
        return material;
    }

    // ───────────────────────── 직렬화 필드 연결 ─────────────────────────

    // [SerializeField] private 필드에 값 넣기 (오브젝트 참조 · 글자 · 숫자 · 배열)
    public static void Set(Object target, string field, object value)
    {
        var so = new SerializedObject(target);
        var property = so.FindProperty(field);
        if (property == null) throw new ArgumentException($"{target.GetType().Name} 에 '{field}' 필드가 없음");

        switch (value)
        {
            case Object reference: property.objectReferenceValue = reference; break;
            case string text: property.stringValue = text; break;
            case float number: property.floatValue = number; break;
            case int number: property.intValue = number; break;
            case bool flag: property.boolValue = flag; break;
            case Vector3 vector: property.vector3Value = vector; break;
            case IList list:
                property.arraySize = list.Count;
                for (int i = 0; i < list.Count; i++)
                    property.GetArrayElementAtIndex(i).objectReferenceValue = (Object)list[i];
                break;
            default: throw new ArgumentException($"'{field}' 에 넣을 수 없는 값 {value}");
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // 비교 씬 위쪽 안내 (AbCompareScene 의 필드)
    public static void SetBanner(AbCompareScene scene, string title, string skill, string request, string causeA, string causeB)
    {
        Set(scene, "sceneTitle", title);
        Set(scene, "skillName", skill);
        Set(scene, "request", request);
        Set(scene, "causeA", causeA);
        Set(scene, "causeB", causeB);
    }

    // 저장소 루트 기준 글자 파일 읽기 (줄바꿈은 \n 으로)
    public static string ReadRepoText(string relativePath)
    {
        return File.ReadAllText(relativePath).Replace("\r\n", "\n").TrimEnd('\n');
    }
}
