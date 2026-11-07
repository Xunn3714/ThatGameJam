using System;
using System.Collections.Generic;
using System.IO;
using SeedGame;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Level02ArtFactory
{
    const string Folder = "Assets/_Game/Resources/Level02";
    const string ScenePath = "Assets/_Game/Scenes/Level02_Alpha.unity";
    const string FirstScenePath = "Assets/_Game/Scenes/Level01_Alpha.unity";
    const string RootName = "Level 2 painted scenery";
    static Transform root;
    static readonly List<SpriteRenderer> placedArt = new List<SpriteRenderer>();

    public static void ApplyRequestedArt()
    {
        InstallFirstLevelDoor();
        InstallAndRenderPreview();
    }

    [MenuItem("Seed/Install first level ruins door")]
    public static void InstallFirstLevelDoor()
    {
        if (EditorSceneManager.GetActiveScene().path != FirstScenePath)
            EditorSceneManager.OpenScene(FirstScenePath, OpenSceneMode.Single);
        Level01TransitionSetup.InstallDoorArt();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), FirstScenePath);
        Debug.Log("Installed painted ruins door at the end of level one.");
    }

    struct Placement
    {
        public Vector3 position;
        public Vector3 scale;
        public Quaternion rotation;
    }

    [MenuItem("Seed/Install second level art")]
    public static void InstallInScene()
    {
        if (EditorSceneManager.GetActiveScene().path != ScenePath)
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Install();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), ScenePath);
        Debug.Log("Installed second level art and gate sequence in " + ScenePath);
    }

    [MenuItem("Seed/Render second level art preview")]
    public static void InstallAndRenderPreview()
    {
        InstallInScene();
        var camera = GameObject.Find("Main Camera").GetComponent<Camera>();
        var texture = new RenderTexture(1280, 720, 24);
        var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        var previous = RenderTexture.active;
        var previousPosition = camera.transform.position;
        var previousSize = camera.orthographicSize;
        camera.targetTexture = texture;
        string directory = Path.Combine(Application.dataPath, "../Level02ArtPreview");
        Directory.CreateDirectory(directory);
        float[] centers = { 7.8f, 23.5f, 39.2f, 54.9f };
        try
        {
            for (int i = 0; i < centers.Length; i++)
            {
                camera.transform.position = new Vector3(centers[i], 2.8f, -10f);
                camera.Render();
                RenderTexture.active = texture;
                image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
                image.Apply();
                File.WriteAllBytes(Path.Combine(directory, "section-" + (i + 1) + ".png"),
                    image.EncodeToPNG());
            }
            // The reference is a single 6886 x 1722 panorama. A half-size
            // render at the same aspect ratio makes placement easy to compare.
            var overview = new RenderTexture(3443, 861, 24);
            var overviewImage = new Texture2D(3443, 861, TextureFormat.RGB24, false);
            camera.targetTexture = overview;
            camera.orthographicSize = 7.827f;
            camera.transform.position = new Vector3(31.3f, 6.02f, -10f);
            camera.Render();
            RenderTexture.active = overview;
            overviewImage.ReadPixels(new Rect(0, 0, 3443, 861), 0, 0);
            overviewImage.Apply();
            File.WriteAllBytes(Path.Combine(directory, "overview.png"), overviewImage.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(overviewImage);
            UnityEngine.Object.DestroyImmediate(overview);
            Debug.Log("Rendered second level previews to " + directory);
        }
        finally
        {
            camera.targetTexture = null;
            camera.transform.position = previousPosition;
            camera.orthographicSize = previousSize;
            RenderTexture.active = previous;
            UnityEngine.Object.DestroyImmediate(image);
            UnityEngine.Object.DestroyImmediate(texture);
        }
    }

    public static void Install()
    {
        foreach (var flower in UnityEngine.Object.FindObjectsByType<Level02GateFlower>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (flower.placedStages != null && flower.placedStages.Length == 3)
            {
                Debug.Log("Second level has hand-placed animated art; art installation skipped.");
                return;
            }
        ImportSprites();
        var handPlaced = new Dictionary<string, Placement>();
        foreach (var marker in UnityEngine.Object.FindObjectsByType<ManualArtPlacement>(
            FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (marker.WasMoved)
                handPlaced[marker.name] = new Placement {
                    position = marker.transform.position,
                    scale = marker.transform.localScale,
                    rotation = marker.transform.rotation
                };
        placedArt.Clear();
        var previous = GameObject.Find(RootName);
        if (previous) UnityEngine.Object.DestroyImmediate(previous);
        root = new GameObject(RootName).transform;
        GameObject.Find("Main Camera").GetComponent<Camera>().backgroundColor =
            new Color(.32f, .42f, .47f);

        // Coordinates trace the supplied 6886 x 1722 panorama. Pixel (0,1530)
        // is world (0,0); approximately 110 reference pixels equal one world unit.
        // The named sprites are placed only where their silhouettes appear there.
        GameObject.Find("Main Camera").GetComponent<Camera>().backgroundColor =
            new Color(.41f, .50f, .54f);
        Art("Ruins beyond entrance", "背景石柱朝右", 3.2f, 7.8f, 4.3f, -90);
        Art("Column behind middle arch", "背景石柱朝左", 20.7f, 8.5f, 3.2f, -90);
        Art("Column behind gate", "背景石柱朝右", 49.2f, 7.4f, 2.2f, -90);
        Art("High ruin arch", "弧形b", 23.4f, 10.0f, 12.0f, -84);
        Art("Lower ruin arch", "弧形a", 39.9f, 1.1f, 11.1f, -84);
        Art("Blue atmosphere", "雾气（蓝）", 31.3f, 6.02f, 62.6f, -70);
        Art("Gate rear wheel", "完整的轮毂", 54.2f, 8.0f, 16.4f, -60,
            new Color(.67f, .76f, .79f, .53f));
        Art("Gate lower wheel", "四分之一轮a", 49.9f, -1.5f, 8.2f, -59);
        Art("Gate lower wheel counterpart", "四分之一轮b", 58.8f, -2.1f, 8.6f, -59);
        Art("White atmosphere", "雾气（白）", 31.3f, 6.02f, 62.6f, -40);
        Art("Entrance ceiling A", "背景钟乳石a", 3.9f, 12.6f, 7.8f, -30);
        Art("Corridor ceiling B", "背景钟乳石b", 12.4f, 13f, 9.5f, -30);
        Art("Arch ceiling C", "背景钟乳石c", 21.9f, 12.9f, 8.6f, -30);
        Art("Central ceiling A", "背景钟乳石a(镜像", 33.1f, 12.8f, 10f, -30);
        Art("Gate ceiling B", "背景钟乳石b（镜像", 44.4f, 12.7f, 10.5f, -30);
        Art("End ceiling C", "背景钟乳石c（镜像", 56.2f, 12.6f, 12.8f, -30);
        Art("Water corridor arcade", "廊柱", 11.4f, 2.7f, 16.4f, -25);
        Art("Moss on water corridor", "背景灌木", 11.4f, 1.7f, 13.0f, -24);
        Art("Water corridor vines", "藤蔓", 11.4f, 3.3f, 13f, 4);
        Art("Gate overhead beam", "机关门横梁", 54.7f, 12.1f, 17.5f, -19);
        Art("Gate dark wheel", "机关门主齿轮", 56.7f, 8.1f, 6.7f, -18);
        Art("Gate left small cog", "机关门小齿轮", 46.2f, 10.0f, 1.55f, -17);
        Art("Gate right small cog", "机关门小齿轮", 49.0f, 10.0f, 1.55f, -17);
        // The pale diagonal ray falls between the two arches and the clover.
        var light = Art("Diagonal pale light", "雾气（白）", 31.2f, 6.4f, 8.5f, -20,
            new Color(1f, 1f, 1f, .48f));
        light.transform.rotation = Quaternion.Euler(0, 0, 36f);

        Hide("Ruins floor before shallow water");
        Hide("Ruins floor after shallow water");
        Hide("Shallow water basin - safe lower route");
        Hide("Shallow water visual");
        // Align the collision top with the visible grass cap, not the PNG canvas edge.
        Rect("Ruins floor before shallow water", 3.3f, -1.5f, 6.6f, 2.4f);
        Rect("Ruins floor after shallow water", 43.4f, -1.44f, 42.6f, 2.4f);
        Rect("Shallow water basin - safe lower route", 11.9f, -1.7f, 10.7f, 1f);
        Rect("Shallow water visual", 11.9f, -.95f, 10.7f, .5f);
        Find("Player - seed sketch").transform.position = new Vector3(5f, .55f, 0f);
        Rect("Dim corridor overlay", 12f, 3.4f, 20.2f, 16f);
        var corridorOverlay = Find("Dim corridor overlay").GetComponent<SpriteRenderer>();
        corridorOverlay.color = new Color(.01f, .045f, .065f, .78f);
        Find("Blue algae resonance").transform.position = new Vector3(7.5f, -.3f, 0f);
        Art("Ground at entrance", "草皮", 3.3f, -.62f, 6.6f, 1);
        Art("Ground after water A", "草皮", 24.1f, -.62f, 13.3f, 1);
        Art("Ground after water B", "草皮", 37.4f, -.62f, 13.3f, 1);
        Art("Ground under gate A", "草皮", 50.7f, -.62f, 13.3f, 1);
        Art("Ground under gate B", "草皮", 60.4f, -.62f, 6.1f, 1);
        Art("Shallow water", "水（透明度40%）", 11.9f, -1.1f, 10.7f, 2);
        Art("Shallow water ripples", "水波纹材质", 11.9f, -1.12f, 10.7f, 3);
        float[] stoneX = { 7.5f, 10.6f, 14.1f, 16.6f };
        var algaeScript = Find("Blue algae resonance").GetComponent<Level02Algae>();
        var algaeMarks = new SpriteRenderer[5];
        for (int i = 0; i < 4; i++)
        {
            Hide("Stepping stone " + (i + 1));
            Rect("Stepping stone " + (i + 1), stoneX[i], .04f, 1.15f, .35f);
            Art("River stone " + (i + 1), "过河石头" + (i + 1),
                stoneX[i], -.51f, 1.85f, 3);
            var algae = Replace("Algae glow " + (i + 1), "水草" + (i + 1), 1f, 6);
            algae.transform.position = new Vector3(stoneX[i], -1.15f, 0f);
            algae.color = algaeScript.unlitTint;
            algaeMarks[i] = algae;
        }
        var farBankAlgae = Art("Water plants at far bank", "水草5", 16.9f, -1.18f,
            1.1f, 6, algaeScript.unlitTint);
        algaeMarks[4] = farBankAlgae;
        algaeScript.glowMarks = algaeMarks;
        Art("Entrance arch front", "石门-前", 2.7f, 1.35f, 1.4f, 4);
        var entranceStop = new GameObject("Second level entrance wall");
        entranceStop.transform.SetParent(root);
        entranceStop.transform.position = new Vector3(2.7f, 1.35f, 0f);
        entranceStop.AddComponent<BoxCollider2D>().size = new Vector2(.65f, 3.7f);

        Hide("First broken column");
        Hide("Second broken column");
        Art("First climbing column", "石柱（小）", 22.6f, 1.0f, 2.8f, 3);
        Art("Second climbing column", "石柱（中）", 27.3f, 2.1f, 2.4f, 3);
        Art("Tall climbing column", "石柱（中）", 31.0f, 3.9f, 2.3f, 3);
        var tower = Art("Clover tower", "石柱（大）", 35.0f, 3.6f, 4.2f, 3);
        DecorateGrowth("Hops A on first column", "楼梯植物a", "楼梯植物b");
        DecorateGrowth("Hops B on second column", "楼梯植物b", "楼梯植物c");
        Hide("High gear platform");
        var oxalis = Find("Oxalis glide leaf").GetComponent<Level02Oxalis>();
        oxalis.transform.position = new Vector3(35.2f, 7.65f, 0f);
        oxalis.visual = Replace("Oxalis leaf visual", "三叶草1", 1.8f, 6);
        oxalis.visual.transform.position = new Vector3(35.2f, 7.65f, 0f);
        oxalis.growthFrames = Frames("三叶草", 3);
        var mushroom = Find("Bounce mushroom").GetComponent<Level02Mushroom>();
        mushroom.visual = Replace("Mushroom cap visual", "弹力蘑菇1", 1.6f, 6);
        mushroom.visual.transform.position = new Vector3(39.3f, .55f, 0f);
        mushroom.growthFrames = Frames("弹力蘑菇", 3);

        Hide("Exit raised platform before gate");
        Hide("Exit raised platform after gate");
        Hide("Gate standing mark");
        Art("Stone bridge before gate", "石柱（大）", 43.4f, 1.1f, 4.0f, 3);
        Art("Stone bridge after gate", "草皮", 55.5f, 1.6f, 15.7f, 3);
        // The solid pile stops the player at x≈50.4. Three linked pulses are
        // required to reach the last plant at x=59.2 (stage-two radius 7.75).
        Find("Final stone gate").transform.position = new Vector3(51f, 2.45f, 0f);
        InstallGateArt();
        Art("Foreground at entrance", "前景a", 1.5f, -2.3f, 5.0f, 20);
        Art("Foreground below arch", "前景b", 22.9f, -3.6f, 7.0f, 20);
        Art("Foreground below middle", "前景c", 34.0f, -3.4f, 11f, 20);
        Art("Foreground gate base", "前景d", 45.4f, -2.8f, 1.7f, 20);
        foreach (var renderer in placedArt)
        {
            if (!renderer) continue;
            var marker = renderer.GetComponent<ManualArtPlacement>();
            if (!marker) marker = renderer.gameObject.AddComponent<ManualArtPlacement>();
            marker.RecordGenerated();
            if (!handPlaced.TryGetValue(renderer.name, out var placement)) continue;
            renderer.transform.position = placement.position;
            renderer.transform.localScale = placement.scale;
            renderer.transform.rotation = placement.rotation;
        }
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    static void DecorateGrowth(string rootName, string first, string second)
    {
        var vine = Find(rootName).GetComponent<Level02GrowthVine>();
        vine.bud = Replace(rootName + " dormant bud", "楼梯植物a-1", .78f, 6);
        vine.artSteps = new Level02GrowthStep[vine.platforms.Length];
        for (int i = 0; i < vine.platforms.Length; i++)
        {
            var platform = vine.platforms[i];
            var placeholder = platform.GetComponent<SpriteRenderer>();
            placeholder.enabled = false;
            placeholder.sprite = null;
            for (int childIndex = platform.transform.childCount - 1; childIndex >= 0; childIndex--)
                if (platform.transform.GetChild(childIndex).name == "Growing painted leaf")
                    UnityEngine.Object.DestroyImmediate(platform.transform.GetChild(childIndex).gameObject);
            var child = new GameObject("Growing painted leaf");
            child.transform.SetParent(platform.transform, false);
            child.transform.localPosition = Vector3.zero;
            var art = child.AddComponent<Level02GrowthStep>();
            art.visual = child.AddComponent<SpriteRenderer>();
            art.visual.sortingOrder = 6;
            art.frames = Frames(i == 0 ? first + "-" : second + "-", 3);
            art.worldWidth = 1.85f;
            art.Show(0f);
            vine.artSteps[i] = art;
        }
    }

    static void InstallGateArt()
    {
        var gate = Find("Final stone gate").GetComponent<Level02Gate>();
        // The purple doorway belongs at the entrance. The exit is a wheel,
        // sliding rack and rubble; its growth starts at the rocks' feet.
        gate.gateVisual = Replace("Moving gate art", "机关门齿条", 1.25f, 7);
        gate.gateVisual.transform.position = new Vector3(47.5f, 6.9f, 0f);
        gate.rubble = new Transform[4];
        gate.rubble[0] = Art("Gate rubble below left", "四分之一圆石头a", 49.2f, 1.1f, 4.8f, 7).transform;
        gate.rubble[1] = Art("Gate rubble below right", "四分之一圆石头b", 58.6f, 1.0f, 5.0f, 7).transform;
        gate.rubble[2] = Art("Gate fractured wheel", "四分之一轮a", 54.7f, 6.9f, 5.3f, -59).transform;
        var fallenBeam = Art("Gate fallen beam", "石柱（中）", 50.6f, 2.55f, 1.0f, 7);
        gate.rubble[3] = fallenBeam.transform;
        gate.rubble[3].rotation = Quaternion.Euler(0, 0, 73f);
        gate.growingVines = new Transform[4];
        gate.vineFullScales = new Vector3[4];
        float[] x = { 49.7f, 52.0f, 55.9f, 59.2f };
        string[] vineSprites = { "楼梯植物a-3", "楼梯植物b-3", "楼梯植物c-3", "楼梯植物a-3" };
        for (int i = 0; i < gate.growingVines.Length; i++)
        {
            var vine = Art("Gate pushing vine " + (i + 1), vineSprites[i], x[i], 2.75f,
                2.2f, 8);
            gate.growingVines[i] = vine.transform;
            gate.vineFullScales[i] = vine.transform.localScale;
            vine.transform.localScale = new Vector3(vine.transform.localScale.x, .02f, 1f);
        }
        for (int i = 0; i < gate.flowers.Length; i++)
        {
            var flower = gate.flowers[i];
            flower.transform.position = new Vector3(x[i], 2.8f, 0f);
            flower.visual = Replace("Gate flower glow " + (i + 1), "楼梯植物a-1",
                .65f, 9);
            flower.visual.transform.position = new Vector3(x[i], 2.8f, 0f);
            flower.growthFrames = Frames("楼梯植物a-", 3);
        }
    }

    static Sprite[] Frames(string prefix, int count)
    {
        var sprites = new Sprite[count];
        for (int i = 0; i < count; i++) sprites[i] = Sprite(prefix + (i + 1));
        return sprites;
    }

    static Sprite Sprite(string name)
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(Folder + "/" + name + ".png");
        if (!sprite) throw new Exception("Missing second-level sprite: " + name);
        return sprite;
    }

    static SpriteRenderer Art(string name, string spriteName, float x, float y,
        float width, int order, Color? tint = null)
    {
        var go = new GameObject(name);
        go.transform.SetParent(root);
        go.transform.position = new Vector3(x, y, 0f);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = Sprite(spriteName);
        renderer.sortingOrder = order;
        renderer.color = tint ?? Color.white;
        // Imported PNG dimensions at 200 pixels/unit are the art's native size.
        go.transform.localScale = Vector3.one;
        placedArt.Add(renderer);
        return renderer;
    }

    static SpriteRenderer Replace(string objectName, string spriteName,
        float width, int order, Color? tint = null)
    {
        var renderer = Find(objectName).GetComponent<SpriteRenderer>();
        renderer.sprite = Sprite(spriteName);
        renderer.color = tint ?? Color.white;
        renderer.sortingOrder = order;
        renderer.transform.localScale = Vector3.one;
        renderer.enabled = true;
        placedArt.Add(renderer);
        return renderer;
    }

    static GameObject Find(string name)
    {
        var go = GameObject.Find(name);
        if (!go) throw new Exception("Second-level scene object missing: " + name);
        return go;
    }

    static void Rect(string name, float x, float y, float width, float height)
    {
        var go = Find(name);
        go.transform.position = new Vector3(x, y, 0f);
        go.transform.localScale = new Vector3(width, height, 1f);
    }

    static void Hide(string name)
    {
        var renderer = Find(name).GetComponent<SpriteRenderer>();
        if (renderer)
        {
            renderer.enabled = false;
            renderer.sprite = null;
        }
    }

    static void ImportSprites()
    {
        GenerateMechanismArt();
        foreach (var path in Directory.GetFiles(Folder, "*.png"))
        {
            var assetPath = path.Replace('\\', '/');
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) continue;
            if (importer.textureType == TextureImporterType.Sprite &&
                Mathf.Approximately(importer.spritePixelsPerUnit, 200f) &&
                importer.maxTextureSize == 4096) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 200f;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();
        }
    }

    // These four flat silhouettes are visible in the supplied composite and
    // gate keyframes, but were not included among the separate PNG layers.
    // They are drawn deterministically; no additional AI artwork is used.
    static void GenerateMechanismArt()
    {
        MechanismGear("机关门主齿轮", 512, true);
        MechanismGear("机关门小齿轮", 256, false);
        string beamPath = Folder + "/机关门横梁.png";
        if (!File.Exists(beamPath))
        {
            var texture = new Texture2D(2048, 176, TextureFormat.RGBA32, false);
            var pixels = new Color32[2048 * 176];
            var blue = new Color32(34, 86, 115, 255);
            var edge = new Color32(22, 70, 98, 255);
            for (int y = 0; y < 176; y++)
                for (int x = 0; x < 2048; x++)
                {
                    bool crenel = y < 49 && x % 244 < 171;
                    bool solid = y >= 49 && y < 163;
                    if (crenel || solid) pixels[y * 2048 + x] = y > 146 ? edge : blue;
                }
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(beamPath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(beamPath);
        }
        string rackPath = Folder + "/机关门齿条.png";
        if (!File.Exists(rackPath))
        {
            var texture = new Texture2D(160, 1024, TextureFormat.RGBA32, false);
            var pixels = new Color32[160 * 1024];
            var blue = new Color32(85, 132, 158, 255);
            var dark = new Color32(33, 79, 107, 255);
            for (int y = 0; y < 1024; y++)
                for (int x = 0; x < 160; x++)
                {
                    bool spine = x >= 27 && x < 133;
                    bool tooth = y % 94 < 47 && x >= 9 && x < 151;
                    if (spine || tooth) pixels[y * 160 + x] = y % 94 < 8 ? dark : blue;
                }
            texture.SetPixels32(pixels);
            texture.Apply();
            File.WriteAllBytes(rackPath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(rackPath);
        }
    }

    static void MechanismGear(string name, int size, bool dark)
    {
        string path = Folder + "/" + name + ".png";
        if (File.Exists(path)) return;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color32[size * size];
        var body = dark ? new Color32(31, 66, 88, 255) : new Color32(182, 205, 214, 255);
        var ring = new Color32(94, 144, 166, 255);
        var centre = new Color32(42, 80, 104, 255);
        float middle = size * .5f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x + .5f - middle;
                float dy = y + .5f - middle;
                float radius = Mathf.Sqrt(dx * dx + dy * dy) / middle;
                float angle = Mathf.Atan2(dy, dx) + Mathf.PI;
                float teeth = (angle * 10f / (2f * Mathf.PI)) % 1f;
                float outer = teeth > .18f && teeth < .82f ? .98f : .83f;
                if (radius > outer) continue;
                var colour = body;
                if (dark && radius < .42f) colour = ring;
                if (dark && radius < .29f) colour = centre;
                if (!dark && radius < .36f) colour = centre;
                pixels[y * size + x] = colour;
            }
        texture.SetPixels32(pixels);
        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path);
    }
}
