using System.IO;
using SeedGame;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class BuildLevel02Scene
{
    const string Root = "Assets/_Game";
    const string First = Root + "/Scenes/Level01_Alpha.unity";
    const string Second = Root + "/Scenes/Level02_Alpha.unity";
    static Sprite square;

    [MenuItem("Seed/Build second level alpha")]
    public static void Build()
    {
        PrepareAiFrames();
        square = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Art/Placeholder/white.png");
        var hero = AssetDatabase.LoadAssetAtPath<Sprite>(Root + "/Art/Character/主角形象草稿.PNG");
        if (!square || !hero) throw new System.Exception("Placeholder sprites are missing");
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(2f, 2.4f, -10f);
        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5.2f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.07f, .13f, .16f);
        cameraObject.AddComponent<AudioListener>();
        var follow = cameraObject.AddComponent<SeedCamera>();
        follow.minX = 0; follow.maxX = 60; follow.minY = -1.6f; follow.maxY = 12;
        follow.deadZoneX = 0f;

        var playerObject = new GameObject("Player - seed sketch");
        playerObject.transform.position = new Vector3(2f, .7f, 0f);
        var body = playerObject.AddComponent<Rigidbody2D>();
        body.gravityScale = 2.7f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        playerObject.AddComponent<BoxCollider2D>().size = new Vector2(.58f, 1.1f);
        var feet = new GameObject("Feet ground probe").transform;
        feet.SetParent(playerObject.transform);
        feet.localPosition = new Vector3(0, -.59f, 0);
        var art = new GameObject("Original character draft");
        art.transform.SetParent(playerObject.transform);
        art.transform.localPosition = new Vector3(.05f, .09f, 0);
        art.transform.localScale = Vector3.one * .82f;
        var heroRenderer = art.AddComponent<SpriteRenderer>();
        heroRenderer.sprite = Resources.Load<Sprite>("Character/Walk1") ?? hero;
        heroRenderer.sortingOrder = 5;
        var player = playerObject.AddComponent<SeedPlayer>();
        player.feet = feet;
        var animation = art.AddComponent<SeedCharacterAnimation>();
        animation.player = player;
        animation.frames = new[] {
            Resources.Load<Sprite>("Character/Walk1"),
            Resources.Load<Sprite>("Character/Walk2"),
            Resources.Load<Sprite>("Character/Walk3")
        };
        follow.target = playerObject.transform;

        Block("Ruins floor before shallow water", 5f, -1.2f, 10f, 2.4f, new Color(.23f,.32f,.32f), true);
        Block("Ruins floor after shallow water", 39f, -1.2f, 42f, 2.4f, new Color(.23f,.32f,.32f), true);
        Block("Shallow water basin - safe lower route", 14f, -1.7f, 8f, 1f,
            new Color(.17f,.25f,.27f), true);
        Block("Shallow water visual", 14f, -.95f, 8f, .5f, new Color(.15f,.34f,.42f), false);
        for (int i = 0; i < 4; i++)
            Block("Stepping stone " + (i + 1), 11f + i * 2f, .04f, 1.15f, .35f,
                new Color(.42f,.48f,.45f), true);
        var darkness = Block("Dim corridor overlay", 14f, 2.5f, 10f, 6f,
            new Color(.02f,.08f,.11f,.42f), false).GetComponent<SpriteRenderer>();
        darkness.sortingOrder = 4;
        var glow = new SpriteRenderer[4];
        for (int i = 0; i < glow.Length; i++)
        {
            glow[i] = Block("Algae glow " + (i + 1), 10.7f + i * 2f, -.55f, .75f, .16f,
                new Color(.23f,.44f,.42f), false).GetComponent<SpriteRenderer>();
            glow[i].sortingOrder = 6;
        }
        var algae = new GameObject("Blue algae resonance");
        algae.transform.position = new Vector3(10.5f, -.3f, 0f);
        var algaeScript = algae.AddComponent<Level02Algae>();
        algaeScript.darkness = darkness;
        algaeScript.glowMarks = glow;

        Block("First broken column", 25.5f, 1f, 2.6f, 2f, new Color(.32f,.40f,.37f), true);
        Block("Second broken column", 31.5f, 1.6f, 2.4f, 3.2f, new Color(.32f,.40f,.37f), true);
        var vineA = GrowingVine("Hops A on first column", 20.8f, .35f,
            new Vector3(22.3f, .92f, 1.8f), new Vector3(24.2f, 1.53f, 1.8f));
        var vineB = GrowingVine("Hops B on second column", 26.2f, 2.25f,
            new Vector3(28.1f, 2.66f, 1.8f), new Vector3(30f, 3.25f, 1.8f));
        Block("High gear platform", 33.5f, 3.4f, 3.6f, .35f,
            new Color(.42f,.48f,.45f), true);
        Block("Gear 3 - mandatory", 33.5f, 4.2f, .8f, .8f,
            new Color(1f,.76f,.25f), false, true);
        var oxalisRoot = new GameObject("Oxalis glide leaf");
        oxalisRoot.transform.position = new Vector3(35.2f, 4f, 0f);
        var oxalis = oxalisRoot.AddComponent<Level02Oxalis>();
        oxalis.visual = Block("Oxalis leaf visual", 35.2f, 4.25f, 1f, .55f,
            new Color(.42f,.68f,.47f), false).GetComponent<SpriteRenderer>();
        oxalis.visual.sortingOrder = 6;

        var mushroom = new GameObject("Bounce mushroom");
        mushroom.transform.position = new Vector3(41.6f, .5f, 0f);
        mushroom.layer = 6;
        mushroom.AddComponent<BoxCollider2D>().size = new Vector2(1.8f, .6f);
        var mushroomScript = mushroom.AddComponent<Level02Mushroom>();
        mushroomScript.visual = Block("Mushroom cap visual", 41.6f, .5f, 1.8f, .65f,
            new Color(.35f,.55f,.49f), false).GetComponent<SpriteRenderer>();
        mushroomScript.visual.sortingOrder = 6;

        Block("Exit raised platform before gate", 45f, 2f, 4f, .4f,
            new Color(.42f,.48f,.45f), true);
        Block("Exit raised platform after gate", 53.5f, 2f, 13f, .4f,
            new Color(.42f,.48f,.45f), true);
        Block("Gate standing mark", 46.25f, 2.25f, .45f, .08f,
            new Color(.81f,.78f,.47f), false);
        var gateObject = Block("Final stone gate", 47.5f, 2.45f, .7f, 4.9f,
            new Color(.35f,.45f,.42f), true);
        gateObject.GetComponent<SpriteRenderer>().enabled = false;
        var gate = gateObject.AddComponent<Level02Gate>();
        gate.player = player;
        gate.gateCollider = gateObject.GetComponent<Collider2D>();
        gate.gateVisual = Block("Moving gate art", 47.5f, 2.45f, .7f, 4.9f,
            new Color(.35f,.45f,.42f), false).GetComponent<SpriteRenderer>();
        gate.gateVisual.sortingOrder = 4;
        gate.flowers = new Level02GateFlower[4];
        float[] flowerX = { 48.4f, 50.5f, 53f, 54.7f };
        for (int i = 0; i < 4; i++)
        {
            var root = new GameObject("Gate flower " + (i + 1));
            root.transform.position = new Vector3(flowerX[i], 3f, 0f);
            var flower = root.AddComponent<Level02GateFlower>();
            flower.gate = gate;
            flower.visual = Block("Gate flower glow " + (i + 1), flowerX[i], 3f,
                .8f, 1f, new Color(.43f,.65f,.49f), false).GetComponent<SpriteRenderer>();
            flower.visual.sortingOrder = 6;
            gate.flowers[i] = flower;
        }
        var finishObject = new GameObject("Level 2 exit trigger");
        finishObject.transform.position = new Vector3(58.2f, 3f, 0f);
        finishObject.AddComponent<BoxCollider2D>().isTrigger = true;
        finishObject.GetComponent<BoxCollider2D>().size = new Vector2(1.3f, 2f);
        var finish = finishObject.AddComponent<LevelExit>();
        finish.requiredGears = 3;
        finish.requiredGate = gate;
        var flow = new GameObject("Level 2 completion").AddComponent<LevelFlow>();
        flow.player = player;
        flow.completionMessage = "第二关 Alpha 完成！按 R 重新开始";
        var guide = new GameObject("Level 2 contextual guide").AddComponent<Level02Guide>();
        guide.player = player;
        var checkpoint = new GameObject("Checkpoint after shallow water");
        checkpoint.transform.position = new Vector3(19f, .8f, 0f);
        checkpoint.AddComponent<BoxCollider2D>().isTrigger = true;
        checkpoint.GetComponent<BoxCollider2D>().size = new Vector2(1.2f, 1.6f);
        checkpoint.AddComponent<Checkpoint>();

        Directory.CreateDirectory(Root + "/Scenes");
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), Second);
        EditorBuildSettings.scenes = new[] {
            new EditorBuildSettingsScene(First, true),
            new EditorBuildSettingsScene(Second, true)
        };
        AssetDatabase.SaveAssets();
        Debug.Log("Second level playable alpha built: " + Second);
    }

    [MenuItem("Seed/Prepare AI in-between frames")]
    public static void PrepareAiFrames()
    {
        foreach (var group in new[] { "a", "b", "c", "d", "f" })
        {
            for (int gap = 1; gap <= 2; gap++)
            {
                string prefix = Root + "/Resources/Level01/植物" + group + "/" + group;
                string path = prefix + gap + "_5_ai.png";
                var image = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                var left = AssetDatabase.LoadAssetAtPath<Texture2D>(prefix + gap + ".png");
                var right = AssetDatabase.LoadAssetAtPath<Texture2D>(prefix + (gap + 1) + ".png");
                if (!image || !left || !right) throw new System.Exception("Missing animation image: " + path);
                var importer = (TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsToUnits = 200f * image.width / (left.width + right.width);
                importer.alphaIsTransparency = true;
                importer.maxTextureSize = 4096;
                importer.SaveAndReimport();
            }
        }
    }

    [MenuItem("Seed/Add gear and ruins entrance to first level")]
    public static void AddFirstLevelTransition()
    {
        EditorSceneManager.OpenScene(First, OpenSceneMode.Single);
        Level01TransitionSetup.Install();
        var leaves = GameObject.Find("01 - climb vine").GetComponent<ResonancePlant>();
        leaves.stepFramesA = PlantFrames("a");
        leaves.stepFramesB = PlantFrames("b");
        leaves.stepFramesC = PlantFrames("c");
        leaves.growthFrameSeconds = .10f;
        var near = GameObject.Find("02 - near-bank vine").GetComponent<ResonancePlant>();
        near.bridgeFrames = PlantFrames("d");
        near.growthFrameSeconds = .11f;
        var far = GameObject.Find("03 - far-bank vine").GetComponent<ResonancePlant>();
        far.bridgeFrames = PlantFrames("f");
        far.growthFrameSeconds = .11f;
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), First);
        AssetDatabase.SaveAssets();
        Debug.Log("First level now contains both gear pickups and the ruins entrance.");
    }

    static Sprite[] PlantFrames(string letter)
    {
        string prefix = Root + "/Resources/Level01/植物" + letter + "/" + letter;
        var sprites = new[] {
            AssetDatabase.LoadAssetAtPath<Sprite>(prefix + "1.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>(prefix + "1_5_ai.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>(prefix + "2.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>(prefix + "2_5_ai.png"),
            AssetDatabase.LoadAssetAtPath<Sprite>(prefix + "3.png")
        };
        foreach (var sprite in sprites)
            if (!sprite) throw new System.Exception("Missing sprite for " + prefix);
        return sprites;
    }

    static Level02GrowthVine GrowingVine(string name, float x, float y, params Vector3[] steps)
    {
        var root = new GameObject(name);
        root.transform.position = new Vector3(x, y, 0f);
        var vine = root.AddComponent<Level02GrowthVine>();
        vine.bud = Block(name + " dormant bud", x, y, .75f, .8f,
            new Color(.33f,.49f,.39f), false).GetComponent<SpriteRenderer>();
        vine.platforms = new GameObject[steps.Length];
        for (int i = 0; i < steps.Length; i++)
        {
            var step = steps[i];
            var platform = Block(name + " leaf step " + (i + 1), step.x, step.y,
                step.z, .28f, new Color(.47f,.73f,.53f), true);
            platform.GetComponent<SpriteRenderer>().sortingOrder = 6;
            var collider = platform.GetComponent<BoxCollider2D>();
            collider.usedByEffector = true;
            var effector = platform.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.surfaceArc = 175f;
            platform.SetActive(false);
            vine.platforms[i] = platform;
        }
        return vine;
    }

    static GameObject Block(string name, float x, float y, float width, float height,
        Color tint, bool solid, bool gear = false)
    {
        var go = new GameObject(name);
        go.transform.position = new Vector3(x, y, 0);
        go.transform.localScale = new Vector3(width, height, 1);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = gear ? Resources.Load<Sprite>("UI/Gear") ?? square : square;
        renderer.color = gear ? Color.white : tint;
        if (solid)
        {
            go.layer = 6;
            go.AddComponent<BoxCollider2D>();
        }
        if (gear)
        {
            go.AddComponent<BoxCollider2D>().isTrigger = true;
            go.AddComponent<GearPickup>();
        }
        return go;
    }
}
