using System.IO;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Kit = SkillsLabSceneKit;

// 메뉴 Tools > Skills Lab > Build All Scenes
// AI 스킬 랩의 예제 씬 10개를 코드로 다시 만든다 (Assets/00_Scenes) + 강의 허브 + Build Settings
// 배치 모드: Unity -batchmode -projectPath . -executeMethod SkillsLabSceneBuilder.BuildAll -quit
public static class SkillsLabSceneBuilder
{
    [MenuItem("Tools/Skills Lab/Build All Scenes")]
    public static void BuildAll()
    {
        if (!Application.isBatchMode && !UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        Kit.EnsureSharedAssets();
        BuildSkillMap();
        BuildUIRouter();
        BuildButtonTransition();
        BuildPixelPerfect();
        BuildKoreanFont();
        BuildBloom();
        BuildFastBullet();
        BuildTrigger();
        BuildNavMesh();
        BuildClassRules();
        AssetDatabase.SaveAssets();

        LectureHubBuilder.Build(); // 허브 + Build Settings (허브 맨 앞 + 예제 씬 10개)
    }

    // ───────────────────────── 00 스킬 지도 ─────────────────────────

    static void BuildSkillMap()
    {
        Kit.NewScene(new Color32(20, 23, 32, 255), true, new Vector3(0f, 0f, -10f), new Vector3(0f, 0f, -10f));
        var document = Kit.NewUiDocument("SkillMap");
        var view = document.gameObject.AddComponent<SkillMapView>();
        Kit.Set(view, "document", document);
        Kit.SaveScene("00_SkillMap");
    }

    // ───────────────────────── 0301 UI 시스템 고르기 ─────────────────────────

    static void BuildUIRouter()
    {
        Kit.NewScene(new Color32(28, 40, 58, 255), true, new Vector3(0f, 0f, -10f), new Vector3(0f, 0f, -10f));

        // A: uGUI 로 따로 만든 설정 창 (옛 튜토리얼 모양 그대로: 기본 스프라이트 · 기본 글꼴 · 고정 픽셀 크기)
        var canvasObject = new GameObject("SettingsCanvas_A", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasObject.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;

        var resources = new DefaultControls.Resources
        {
            standard = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd"),
            background = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd"),
            inputField = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd"),
            knob = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd"),
            checkmark = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd"),
            dropdown = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd"),
            mask = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UIMask.psd"),
        };

        var panel = DefaultControls.CreatePanel(resources);
        panel.name = "SettingsPanel";
        panel.transform.SetParent(canvasObject.transform, false);
        var panelRect = (RectTransform)panel.transform;
        TopLeft(panelRect, new Vector2(200f, -270f), new Vector2(520f, 380f));
        panel.GetComponent<UnityEngine.UI.Image>().color = new Color(0.86f, 0.86f, 0.86f, 0.95f);

        LegacyText(panel.transform, "Title", "Settings 설정", new Vector2(16f, -10f), new Vector2(300f, 30f), 16);
        LegacyText(panel.transform, "MusicLabel", "배경음 Music", new Vector2(20f, -70f), new Vector2(200f, 30f), 14);
        Place(DefaultControls.CreateSlider(resources), panel.transform, new Vector2(70f, -76f), new Vector2(380f, 20f));
        LegacyText(panel.transform, "SfxLabel", "효과음 SFX", new Vector2(20f, -130f), new Vector2(200f, 30f), 14);
        Place(DefaultControls.CreateSlider(resources), panel.transform, new Vector2(70f, -136f), new Vector2(380f, 20f));
        var toggle = DefaultControls.CreateToggle(resources);
        toggle.GetComponentInChildren<Text>().text = "진동 Vibration";
        Place(toggle, panel.transform, new Vector2(20f, -190f), new Vector2(200f, 20f));
        var close = DefaultControls.CreateButton(resources);
        close.GetComponentInChildren<Text>().text = "Close 닫기";
        Place(close, panel.transform, new Vector2(20f, -330f), new Vector2(160f, 30f));

        // HUD + B 설정 화면 (UI Toolkit)
        var document = Kit.NewUiDocument("UI");
        var demo = document.gameObject.AddComponent<UIRouterDemo>();
        Kit.SetBanner(demo, "0301 · UI 시스템 고르기", "/ui",
            "설정 화면 만들어줘",
            "프로젝트가 쓰는 UI 방식을 보지 않고 옛 튜토리얼처럼 uGUI Canvas 를 새로 만듦 → HUD(UI Toolkit)와 모양·글꼴이 다르고 겹친다",
            "/ui 가 먼저 이 프로젝트가 UI Toolkit 을 쓰는 것을 확인 → UXML · USS(HudTheme.uss)로 HUD 와 같은 테마");
        Kit.Set(demo, "hudTree", AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{Kit.UiFolder}/UIRouter/Hud.uxml"));
        Kit.Set(demo, "settingsTree", AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{Kit.UiFolder}/UIRouter/SettingsWindow.uxml"));
        Kit.Set(demo, "settingsCanvasA", canvasObject);
        Kit.Set(demo, "closeButtonA", close.GetComponent<UnityEngine.UI.Button>());
        Kit.SaveScene("0301_UIRouter");
    }

    static void TopLeft(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    static void Place(GameObject go, Transform parent, Vector2 position, Vector2 size)
    {
        go.transform.SetParent(parent, false);
        TopLeft((RectTransform)go.transform, position, size);
    }

    static void LegacyText(Transform parent, string name, string text, Vector2 position, Vector2 size, int fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        TopLeft((RectTransform)go.transform, position, size);
        var label = go.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = text;
        label.fontSize = fontSize;
        label.color = Color.black;
    }

    // ───────────────────────── 0302 버튼 전환 ─────────────────────────

    static void BuildButtonTransition()
    {
        Kit.NewScene(new Color32(26, 30, 42, 255), true, new Vector3(0f, 0f, -10f), new Vector3(0f, 0f, -10f));
        string folder = $"{Kit.UiFolder}/ButtonTransition";
        var document = Kit.NewUiDocument("UI", AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{folder}/Buttons.uxml"));
        var demo = document.gameObject.AddComponent<ButtonTransitionDemo>();
        Kit.SetBanner(demo, "0302 · 버튼 전환이 툭 돌아감", "/ui-uitk",
            "버튼에 마우스를 올리면 부드럽게 커지게 해줘",
            "transition 을 :hover 에만 선언 → 올릴 때만 전환이 있고, 뗄 때는 기본 모양으로 바로 돌아감",
            "transition 을 기본 클래스(.lab-button)에 선언 → 들어갈 때도 나갈 때도 같은 0.4초 전환");
        Kit.Set(demo, "styleA", AssetDatabase.LoadAssetAtPath<StyleSheet>($"{folder}/ButtonTransition_A.uss"));
        Kit.Set(demo, "styleB", AssetDatabase.LoadAssetAtPath<StyleSheet>($"{folder}/ButtonTransition_B.uss"));
        Kit.Set(demo, "sourceA", ButtonRules(Kit.ReadRepoText($"{folder}/ButtonTransition_A.uss")));
        Kit.Set(demo, "sourceB", ButtonRules(Kit.ReadRepoText($"{folder}/ButtonTransition_B.uss")));
        Kit.SaveScene("0302_ButtonTransition");
    }

    // USS 원문에서 버튼 규칙 두 개(.lab-button { } 와 .lab-button:hover { })만 잘라 낸다
    static string ButtonRules(string uss)
    {
        int start = uss.IndexOf(".lab-button {", System.StringComparison.Ordinal);
        int end = uss.IndexOf(".button-hint", System.StringComparison.Ordinal);
        return uss.Substring(start, end - start).TrimEnd();
    }

    // ───────────────────────── 0303 픽셀 퍼펙트 ─────────────────────────

    const int ArtSize = 16;

    static void BuildPixelPerfect()
    {
        int renderer2D = Kit.EnsureRenderer2D();
        var camera = Kit.NewScene(new Color32(120, 180, 235, 255), true, new Vector3(1.6f, 1.2f, -10f), new Vector3(1.6f, 1.2f, -10f), 4.1f);
        camera.GetUniversalAdditionalCameraData().SetRenderer(renderer2D);
        var pixelCamera = camera.gameObject.AddComponent<PixelPerfectCamera>();
        pixelCamera.assetsPPU = 16;
        pixelCamera.refResolutionX = 240;
        pixelCamera.refResolutionY = 135;
        pixelCamera.gridSnapping = PixelPerfectCamera.GridSnapping.UpscaleRenderTexture;
        pixelCamera.cropFrame = PixelPerfectCamera.CropFrame.None;

        // 같은 그림을 두 벌 저장해서 가져오기 설정만 다르게 (A: 흔한 기본값 / B: 픽셀아트용)
        Kit.EnsureFolder(Kit.SpriteFolder);
        var heroA = new Sprite[2];
        var heroB = new Sprite[2];
        for (int frame = 0; frame < 2; frame++)
        {
            var pixels = HeroPixels(frame);
            heroA[frame] = SavePixelSprite($"PixelHero_A_{frame}", pixels, false, 13);
            heroB[frame] = SavePixelSprite($"PixelHero_B_{frame}", pixels, true, 16);
        }
        var tilePixels = TilePixels();
        var tileA = SavePixelSprite("PixelTile_A", tilePixels, false, 16);
        var tileB = SavePixelSprite("PixelTile_B", tilePixels, true, 16);

        var spriteMaterial = AssetDatabase.LoadAssetAtPath<Material>("Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");

        // 땅 두 줄 + 떠 있는 블록
        var tiles = new System.Collections.Generic.List<SpriteRenderer>();
        var ground = new GameObject("Ground");
        for (int x = -12; x <= 14; x++)
        {
            for (int y = 0; y < 2; y++)
                tiles.Add(NewSprite($"Tile_{x}_{y}", ground.transform, tileA, new Vector3(x, -2f - y, 0f), spriteMaterial, 0));
        }
        foreach (var x in new[] { -3, -2, 4, 5, 6 })
            tiles.Add(NewSprite($"Block_{x}", ground.transform, tileA, new Vector3(x, 2.5f, 0f), spriteMaterial, 0));

        var hero = NewSprite("PixelHero", null, heroA[0], new Vector3(0f, -1f, 0f), spriteMaterial, 1);
        var walker = hero.gameObject.AddComponent<PixelWalker>();
        Kit.Set(walker, "body", hero);
        Kit.Set(walker, "frames", heroA);
        Kit.Set(walker, "followCamera", camera.transform);

        var document = Kit.NewUiDocument("UI");
        var demo = document.gameObject.AddComponent<PixelPerfectDemo>();
        Kit.SetBanner(demo, "0303 · 픽셀아트 흐림·떨림", "/2d-pixel-perfect",
            "도트 캐릭터가 걸어 다니는 2D 화면 만들어줘",
            "그림 가져오기 기본값(Bilinear · 압축 · PPU 13 ≠ 타일 16) + 카메라가 소수점 위치로 따라감 → 번지고, 걸을 때 픽셀이 흔들림",
            "Point 필터 · 압축 없음 · PPU 16 으로 통일 + URP 2D Pixel Perfect Camera → 도트가 정수 배로 선명하고 안정");
        Kit.Set(demo, "walker", walker);
        Kit.Set(demo, "heroA", heroA);
        Kit.Set(demo, "heroB", heroB);
        Kit.Set(demo, "tiles", tiles);
        Kit.Set(demo, "tileA", tileA);
        Kit.Set(demo, "tileB", tileB);
        Kit.Set(demo, "viewCamera", camera);
        Kit.Set(demo, "pixelCamera", pixelCamera);
        Kit.SaveScene("0303_PixelPerfect");
    }

    static SpriteRenderer NewSprite(string name, Transform parent, Sprite sprite, Vector3 position, Material material, int order)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sharedMaterial = material;
        renderer.sortingOrder = order;
        return renderer;
    }

    // 16x16 도트 기사 (frame 0/1 = 다리 모양만 다름)
    static Color32[] HeroPixels(int frame)
    {
        string[] rows =
        {
            "................",
            ".....HHHHHH.....",
            "....HHHHHHHH....",
            "....HSSSSSSH....",
            "....SSESSESS....",
            "....SSSSSSSS....",
            ".....SSMMSS.....",
            "...BBRRRRRRBB...",
            "..SBRRRYYRRRBS..",
            "..S.RRRYYRRR.S..",
            "....RRRRRRRR....",
            "....DDDDDDDD....",
            frame == 0 ? "....LLL..LLL...." : "....LLL...LLL...",
            frame == 0 ? "....LLL..LLL...." : "...LLL....LLL...",
            frame == 0 ? "...KKKK..KKKK..." : "...KKK.....KKK..",
            "................",
        };
        var palette = new System.Collections.Generic.Dictionary<char, Color32>
        {
            { '.', new Color32(0, 0, 0, 0) },
            { 'H', new Color32(120, 60, 30, 255) },
            { 'S', new Color32(250, 200, 150, 255) },
            { 'E', new Color32(30, 30, 60, 255) },
            { 'M', new Color32(200, 90, 90, 255) },
            { 'B', new Color32(40, 60, 140, 255) },
            { 'R', new Color32(220, 50, 60, 255) },
            { 'Y', new Color32(255, 220, 60, 255) },
            { 'D', new Color32(90, 60, 40, 255) },
            { 'L', new Color32(50, 70, 160, 255) },
            { 'K', new Color32(40, 30, 30, 255) },
        };
        return ToPixels(rows, palette);
    }

    // 16x16 풀밭 흙 타일
    static Color32[] TilePixels()
    {
        var pixels = new Color32[ArtSize * ArtSize];
        for (int y = 0; y < ArtSize; y++)
        {
            for (int x = 0; x < ArtSize; x++)
            {
                int top = ArtSize - 1 - y; // 위에서 몇 번째 줄
                Color32 c;
                if (top < 3) c = (x + top) % 5 == 0 ? new Color32(60, 160, 60, 255) : new Color32(90, 200, 80, 255);
                else if (top == 3) c = new Color32(50, 120, 50, 255);
                else c = ((x * 7 + top * 3) % 11 == 0) ? new Color32(120, 80, 50, 255) : new Color32(160, 110, 70, 255);
                if (x == 0 || top == ArtSize - 1) c = new Color32(110, 70, 45, 255);
                pixels[y * ArtSize + x] = c;
            }
        }
        return pixels;
    }

    static Color32[] ToPixels(string[] rows, System.Collections.Generic.Dictionary<char, Color32> palette)
    {
        var pixels = new Color32[ArtSize * ArtSize];
        for (int row = 0; row < ArtSize; row++)
            for (int x = 0; x < ArtSize; x++)
                pixels[(ArtSize - 1 - row) * ArtSize + x] = palette[rows[row][x]];
        return pixels;
    }

    // PNG 로 저장하고 가져오기 설정을 정한다
    // pixelArt = true: Point · 압축 없음 · 밉맵 없음 / false: Bilinear · 압축(보통 품질) · 밉맵 (새 그림의 흔한 기본값)
    static Sprite SavePixelSprite(string name, Color32[] pixels, bool pixelArt, int pixelsPerUnit)
    {
        string path = $"{Kit.SpriteFolder}/{name}.png";
        var texture = new Texture2D(ArtSize, ArtSize, TextureFormat.RGBA32, false);
        texture.SetPixels32(pixels);
        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.filterMode = pixelArt ? FilterMode.Point : FilterMode.Bilinear;
        importer.textureCompression = pixelArt ? TextureImporterCompression.Uncompressed : TextureImporterCompression.Compressed;
        importer.mipmapEnabled = !pixelArt;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // ───────────────────────── 0304 한글 글꼴 ─────────────────────────

    static void BuildKoreanFont()
    {
        Kit.NewScene(new Color32(30, 34, 46, 255), true, new Vector3(0f, 0f, -10f), new Vector3(0f, 0f, -10f));

        var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        var texts = new[]
        {
            TmpText(canvasObject.transform, "HudText", "체력 100 / 골드 250", new Vector2(150f, -300f), 96f),
            TmpText(canvasObject.transform, "LevelText", "레벨 7 · 경험치 1,250", new Vector2(150f, -450f), 60f),
            TmpText(canvasObject.transform, "MenuText", "상점 · 가방 · 설정 (Shop · Bag · Settings)", new Vector2(150f, -570f), 50f),
        };

        var document = Kit.NewUiDocument("UI");
        var demo = document.gameObject.AddComponent<KoreanFontDemo>();
        Kit.SetBanner(demo, "0304 · 한글이 네모로 나옴", "/optimize-text-mesh-pro",
            "HUD 에 '체력 100 / 골드 250' 을 TextMeshPro 로 띄워줘",
            "TMP 기본 글꼴(LiberationSans SDF)과 그 예비 글꼴에 한글 글자가 없음 → 한글 자리가 □ 로 나온다",
            "한글 글꼴(Noto Sans KR, OFL)을 동적 글꼴 에셋으로 만들어 기본 글꼴의 Fallback 에 붙임 → 영문은 그대로, 한글은 채워짐");
        Kit.Set(demo, "texts", texts);
        Kit.Set(demo, "fontA", Kit.Liberation);
        Kit.Set(demo, "fontB", Kit.LiberationKr);
        Kit.SaveScene("0304_KoreanFont");
    }

    static TMP_Text TmpText(Transform parent, string name, string text, Vector2 position, float size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        TopLeft((RectTransform)go.transform, position, new Vector2(1050f, size * 1.4f));
        var label = go.AddComponent<TextMeshProUGUI>();
        label.font = Kit.Liberation;
        label.text = text;
        label.fontSize = size;
        label.color = Color.white;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        return label;
    }

    // ───────────────────────── 0401 블룸 ─────────────────────────

    static void BuildBloom()
    {
        var camera = Kit.NewScene(new Color32(6, 8, 14, 255), false, new Vector3(1.8f, 2.2f, -8f), new Vector3(1.8f, 0.9f, 0f), 50f);
        Object.DestroyImmediate(GameObject.Find("Directional Light"));
        RenderSettings.ambientLight = new Color(0.12f, 0.13f, 0.16f);

        Kit.Primitive(PrimitiveType.Plane, "Floor", Vector3.zero, new Vector3(3f, 1f, 3f), Kit.LitMaterial("Bloom", "Floor", new Color(0.12f, 0.13f, 0.16f)));

        Color[] colors =
        {
            new Color(1f, 0.35f, 0.2f), new Color(1f, 0.8f, 0.2f), new Color(0.3f, 1f, 0.45f), new Color(0.25f, 0.6f, 1f), new Color(0.85f, 0.35f, 1f),
        };
        var orbs = new Transform[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            var material = Kit.LitMaterial("Bloom", $"Orb_{i}", colors[i], 6f);
            orbs[i] = Kit.Primitive(PrimitiveType.Sphere, $"Orb_{i}", new Vector3(-2.6f + i * 1.3f, 1.1f, 0f), Vector3.one * 0.7f, material, false).transform;
        }

        // Global Volume + Bloom (A/B 같음)
        string profilePath = $"{Kit.MaterialFolder}/Bloom/BloomProfile.asset";
        AssetDatabase.DeleteAsset(profilePath);
        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(profile, profilePath);
        var bloom = profile.Add<Bloom>(true);
        bloom.intensity.Override(2.5f);
        bloom.threshold.Override(1f);
        bloom.scatter.Override(0.7f);
        bloom.name = "Bloom";
        AssetDatabase.AddObjectToAsset(bloom, profile);
        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();

        var volumeObject = new GameObject("Global Volume");
        var volume = volumeObject.AddComponent<Volume>();
        volume.isGlobal = true;
        volume.sharedProfile = profile;

        var document = Kit.NewUiDocument("UI");
        var demo = document.gameObject.AddComponent<BloomDemo>();
        Kit.SetBanner(demo, "0401 · 블룸이 안 보임", "/urp-postprocessing",
            "빛나는 구슬에 블룸 넣어줘",
            "Volume 에 Bloom 은 넣었지만 카메라의 Post Processing 스위치가 꺼져 있음 → 이 카메라에는 Volume 효과가 그려지지 않는다",
            "Global Volume(Bloom) + 카메라 Rendering › Post Processing 켬 → 밝은(HDR) 구슬 주변으로 빛이 번진다");
        Kit.Set(demo, "viewCamera", camera);
        Kit.Set(demo, "globalVolume", volume);
        Kit.Set(demo, "orbs", orbs);
        Kit.SaveScene("0401_Bloom");
    }

    // ───────────────────────── 0402 빠른 총알 ─────────────────────────

    static void BuildFastBullet()
    {
        Kit.NewScene(new Color32(30, 36, 48, 255), false, new Vector3(-0.8f, 2.2f, -12f), new Vector3(-0.8f, 2.2f, 0f), 45f);

        Kit.Primitive(PrimitiveType.Plane, "Floor", Vector3.zero, new Vector3(3f, 1f, 1f), Kit.LitMaterial("FastBullet", "Floor", new Color(0.3f, 0.32f, 0.36f)));
        var wall = Kit.Primitive(PrimitiveType.Cube, "ThinWall", new Vector3(0f, 2f, 0f), new Vector3(0.1f, 4f, 2f), Kit.LitMaterial("FastBullet", "Wall", new Color(0.4f, 0.75f, 1f)));
        Kit.Primitive(PrimitiveType.Cube, "Gun", new Vector3(-8.1f, 1.5f, 0f), new Vector3(1.2f, 0.45f, 0.45f), Kit.LitMaterial("FastBullet", "Gun", new Color(0.2f, 0.2f, 0.22f)), false);

        var muzzle = new GameObject("Muzzle").transform;
        muzzle.position = new Vector3(-7.45f, 1.5f, 0f);

        // 총알 원본 (꺼 둠 → 쏠 때마다 복사)
        var bullet = Kit.Primitive(PrimitiveType.Sphere, "BulletTemplate", muzzle.position, Vector3.one * 0.3f, Kit.LitMaterial("FastBullet", "Bullet", new Color(1f, 0.6f, 0.2f), 2f));
        var body = bullet.AddComponent<Rigidbody>();
        body.useGravity = false;
        body.interpolation = RigidbodyInterpolation.None;
        var trail = bullet.AddComponent<TrailRenderer>();
        trail.time = 0.12f;
        trail.startWidth = 0.25f;
        trail.endWidth = 0f;
        trail.sharedMaterial = Kit.UnlitMaterial("FastBullet", "Trail", new Color(1f, 0.7f, 0.3f));
        var fastBullet = bullet.AddComponent<FastBullet>();
        bullet.SetActive(false);

        var hitMark = Kit.Primitive(PrimitiveType.Sphere, "HitMarkTemplate", Vector3.zero, Vector3.one * 0.35f, Kit.UnlitMaterial("FastBullet", "HitMark", new Color(1f, 0.2f, 0.2f)), false);
        hitMark.SetActive(false);

        var document = Kit.NewUiDocument("UI");
        var demo = document.gameObject.AddComponent<FastBulletDemo>();
        Kit.SetBanner(demo, "0402 · 빠른 총알이 벽을 뚫음", "/physics-3d-collision",
            "총알이 벽에 맞으면 멈추게 해줘",
            "Collision Detection = Discrete(기본값): 0.02초마다 위치만 검사 → 한 번에 벽 두께보다 멀리 가면 벽을 건너뛴다",
            "Collision Detection = Continuous Dynamic: 지나온 길을 검사(연속 충돌) → 이 데모 속도(최대 250 m/s)에서는 벽을 놓치지 않는다 (실제 게임은 크기·속도·물리 설정에 따라 확인 필요)");
        Kit.Set(demo, "bulletTemplate", fastBullet);
        Kit.Set(demo, "muzzle", muzzle);
        Kit.Set(demo, "wall", wall.transform);
        Kit.Set(demo, "hitMarkTemplate", hitMark);
        Kit.SaveScene("0402_FastBullet");
    }

    // ───────────────────────── 0403 트리거 ─────────────────────────

    static void BuildTrigger()
    {
        Kit.NewScene(new Color32(30, 36, 48, 255), false, new Vector3(2.5f, 8f, -9f), new Vector3(2.5f, 0f, 0.5f), 45f);

        Kit.Primitive(PrimitiveType.Plane, "Floor", Vector3.zero, new Vector3(1.6f, 1f, 0.8f), Kit.LitMaterial("Trigger", "Floor", new Color(0.3f, 0.34f, 0.4f)));
        var player = Kit.Primitive(PrimitiveType.Cube, "Player", new Vector3(-5.5f, 0.5f, 0f), Vector3.one, Kit.LitMaterial("Trigger", "Player", new Color(0.3f, 0.55f, 1f)));

        var document = Kit.NewUiDocument("UI");
        var demo = document.gameObject.AddComponent<TriggerDemo>();

        var coinMaterial = Kit.LitMaterial("Trigger", "Coin", new Color(1f, 0.8f, 0.2f));
        for (int i = 0; i < 3; i++)
        {
            var zone = new GameObject($"Coin_{i + 1}");
            zone.transform.position = new Vector3(-3f + i * 3f, 0.75f, 0f);
            var box = zone.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = new Vector3(1f, 1.5f, 1.4f);
            var coin = Kit.Primitive(PrimitiveType.Cylinder, "CoinMesh", zone.transform.position, new Vector3(0.8f, 0.06f, 0.8f), coinMaterial, false);
            coin.transform.SetParent(zone.transform, true);
            coin.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            var coinZone = zone.AddComponent<CoinZone>();
            Kit.Set(coinZone, "demo", demo);
            Kit.Set(coinZone, "coinRenderer", coin.GetComponent<Renderer>());
        }

        Kit.SetBanner(demo, "0403 · 트리거가 안 불림", "/physics-3d-collision",
            "플레이어가 동전을 지나가면 OnTriggerEnter 로 점수 올려줘",
            "플레이어·동전 둘 다 Collider 만 있고 Rigidbody 가 없음 + transform 으로 이동 → 트리거 검사가 일어나지 않아 OnTriggerEnter 0번",
            "움직이는 쪽(플레이어)에 Rigidbody(Is Kinematic) 를 붙임 → 위치를 직접 옮겨도 트리거 검사가 일어나 불린다");
        Kit.Set(demo, "player", player.transform);
        Kit.SaveScene("0403_Trigger");
    }

    // ───────────────────────── 0404 길찾기 ─────────────────────────

    static void BuildNavMesh()
    {
        var camera = Kit.NewScene(new Color32(30, 36, 48, 255), false, new Vector3(3f, 17f, -11f), new Vector3(3f, 0f, 0.5f), 50f);

        // 바닥·장애물: 옛 방식처럼 Static 표시도 해 둔다 (A 의 '안내만 따른 상태')
        var level = new GameObject("Level");
        var floor = Kit.Primitive(PrimitiveType.Plane, "Floor", Vector3.zero, new Vector3(2f, 1f, 2f), Kit.LitMaterial("NavMesh", "Floor", new Color(0.32f, 0.36f, 0.42f)));
        floor.transform.SetParent(level.transform, true);
        var wallMaterial = Kit.LitMaterial("NavMesh", "Obstacle", new Color(0.85f, 0.45f, 0.3f));
        var obstacles = new[]
        {
            (new Vector3(-1.5f, 1f, 0f), new Vector3(11f, 2f, 0.8f)),
            (new Vector3(4.5f, 1f, 4f), new Vector3(0.8f, 2f, 6f)),
            (new Vector3(-5f, 1f, 5f), new Vector3(2f, 2f, 2f)),
            (new Vector3(6f, 1f, -5f), new Vector3(3f, 2f, 2f)),
        };
        for (int i = 0; i < obstacles.Length; i++)
        {
            var o = Kit.Primitive(PrimitiveType.Cube, $"Obstacle_{i}", obstacles[i].Item1, obstacles[i].Item2, wallMaterial);
            o.transform.SetParent(level.transform, true);
        }
        foreach (var t in level.GetComponentsInChildren<Transform>()) t.gameObject.isStatic = true;

        var agentObject = Kit.Primitive(PrimitiveType.Capsule, "Agent", new Vector3(-6f, 1f, -2.5f), Vector3.one, Kit.LitMaterial("NavMesh", "Agent", new Color(0.3f, 0.8f, 1f)), false);
        var agent = agentObject.AddComponent<NavMeshAgent>();
        agent.speed = 5f;
        agent.angularSpeed = 720f;
        agent.acceleration = 20f;
        agent.radius = 0.5f;

        // B: NavMeshSurface 로 굽기 (씬을 만들 때 굽고, 구운 데이터를 에셋으로 저장)
        var surfaceObject = new GameObject("NavMeshSurface");
        var surface = surfaceObject.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.All;
        surface.BuildNavMesh();
        string dataFolder = $"{Kit.SceneFolder}/0404_NavMesh";
        Kit.EnsureFolder(dataFolder);
        string dataPath = $"{dataFolder}/NavMesh-NavMeshSurface.asset";
        AssetDatabase.DeleteAsset(dataPath);
        AssetDatabase.CreateAsset(surface.navMeshData, dataPath);

        // 시작은 둘 다 꺼 둠 → NavMeshDemo 가 A/B 에 맞춰 켠다 (씬이 열릴 때 경고가 두 번 나지 않게)
        surface.enabled = false;
        agent.enabled = false;

        // 구운 뒤에 만든 것들 (굽기에 섞이지 않게)
        var marker = Kit.Primitive(PrimitiveType.Cylinder, "TargetMarker", new Vector3(6f, 0.05f, 5f), new Vector3(0.8f, 0.05f, 0.8f), Kit.UnlitMaterial("NavMesh", "Target", new Color(1f, 0.85f, 0.2f)), false);
        var viewObject = new GameObject("NavMeshView", typeof(MeshFilter), typeof(MeshRenderer));
        viewObject.transform.position = new Vector3(0f, 0.04f, 0f);
        var viewRenderer = viewObject.GetComponent<MeshRenderer>();
        viewRenderer.sharedMaterial = Kit.UnlitMaterial("NavMesh", "NavMeshView", new Color(0.25f, 0.7f, 0.9f));
        viewRenderer.shadowCastingMode = ShadowCastingMode.Off;
        var lineObject = new GameObject("PathLine");
        var line = lineObject.AddComponent<LineRenderer>();
        line.widthMultiplier = 0.18f;
        line.positionCount = 0;
        line.useWorldSpace = true;
        line.sharedMaterial = Kit.UnlitMaterial("NavMesh", "PathLine", new Color(1f, 0.9f, 0.3f));

        var document = Kit.NewUiDocument("UI");
        var demo = document.gameObject.AddComponent<NavMeshDemo>();
        Kit.SetBanner(demo, "0404 · 길찾기 설정", "/initialize-ai-navigation",
            "클릭한 곳으로 캐릭터가 장애물을 피해 걸어가게 해줘",
            "옛 튜토리얼처럼 Static 표시 + 'Navigation 창에서 Bake' 안내만 → NavMeshSurface 로 굽지 않아 NavMesh 없음 → 에이전트가 경고 후 멈춤",
            "AI Navigation 패키지의 NavMeshSurface 컴포넌트로 굽기 → NavMesh 위의 에이전트가 장애물을 돌아서 간다");
        Kit.Set(demo, "surface", surface);
        Kit.Set(demo, "agent", agent);
        Kit.Set(demo, "targetMarker", marker.transform);
        Kit.Set(demo, "viewCamera", camera);
        Kit.Set(demo, "navMeshView", viewObject.GetComponent<MeshFilter>());
        Kit.Set(demo, "pathLine", line);
        Kit.SaveScene("0404_NavMesh");
    }

    // ───────────────────────── 0501 우리 반 규칙 스킬 ─────────────────────────

    static void BuildClassRules()
    {
        Kit.NewScene(new Color32(24, 28, 38, 255), false, new Vector3(0f, 0f, -10f), new Vector3(0f, 0f, 0f), 40f);

        var coin = Kit.Primitive(PrimitiveType.Cylinder, "Coin", new Vector3(-1.2f, -0.6f, 0f), new Vector3(1.4f, 0.1f, 1.4f), Kit.LitMaterial("ClassRules", "Coin", new Color(1f, 0.8f, 0.2f), 0.4f), false);
        coin.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        var pop = coin.AddComponent<ScorePop>();

        var document = Kit.NewUiDocument("UI");
        var demo = document.gameObject.AddComponent<ClassRulesDemo>();

        // 실행 결과: A · B 결과 코드를 실제로 붙여 둔다 (ClassRulesDemo 가 하나만 켬)
        var runA = new GameObject("Run_A_ScoreManagerA").AddComponent<ScoreManagerA>();
        runA.Pop = pop;
        runA.enabled = false;
        var runB = new GameObject("Run_B_ScoreCounterB").AddComponent<ScoreCounterB>();
        Kit.Set(runB, "scoreDocument", document);
        Kit.Set(runB, "scoreStyle", AssetDatabase.LoadAssetAtPath<StyleSheet>($"{Kit.UiFolder}/ClassRules/ScoreBoard.uss"));
        Kit.Set(runB, "coinPop", pop);
        runB.enabled = false;

        // 저장소의 실제 파일 원문
        string skill = Kit.ReadRepoText(".claude/skills/class-rules/SKILL.md");
        int close = skill.IndexOf("\n---", 3, System.StringComparison.Ordinal);
        Kit.Set(demo, "skillFrontMatter", skill.Substring(0, close + 4));
        Kit.Set(demo, "skillBody", skill.Substring(close + 4).Trim('\n'));
        Kit.Set(demo, "codeA", Kit.ReadRepoText("Assets/01_Scripts/ClassRules/ScoreManagerA.cs"));
        Kit.Set(demo, "codeB", Kit.ReadRepoText("Assets/01_Scripts/ClassRules/ScoreCounterB.cs"));
        Kit.Set(demo, "runnerA", runA);
        Kit.Set(demo, "runnerB", runB);
        Kit.Set(demo, "coin", coin);
        Kit.SetBanner(demo, "0501 · 우리 반 규칙 스킬 (class-rules)", "class-rules (이 저장소의 프로젝트 스킬)",
            "Space 를 누르면 점수가 10 오르는 점수판 만들어줘",
            "규칙 스킬 없이: public 필드 · 숫자를 코드에 바로 · Update 마다 FindObjectOfType · OnGUI · Debug.Log 를 남김",
            "class-rules 스킬대로: 숫자는 [SerializeField] · 필드 camelCase · 한글 주석 한 줄 · Debug.Log 없음 · 새 UI 는 UI Toolkit");
        Kit.SaveScene("0501_ClassRules");
    }
}
