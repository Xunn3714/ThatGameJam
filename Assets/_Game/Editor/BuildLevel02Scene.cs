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

    [MenuItem("Seed/Build second level placeholder")]
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
        follow.minX = 0; follow.maxX = 49; follow.minY = -1.6f; follow.maxY = 12;

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
        art.transform.localPosition = new Vector3(.146f, .107f, 0);
        art.AddComponent<SpriteRenderer>().sprite = hero;
        var player = playerObject.AddComponent<SeedPlayer>();
        player.feet = feet;
        follow.target = playerObject.transform;

        Block("Ruins floor before shallow water", 5f, -1.2f, 10f, 2.4f, new Color(.23f,.32f,.32f), true);
        Block("Ruins floor after shallow water", 27.5f, -1.2f, 27f, 2.4f, new Color(.23f,.32f,.32f), true);
        Block("Shallow water visual", 14f, -.35f, 8f, .55f, new Color(.15f,.34f,.42f), false);
        Block("Shallow water basin - safe lower route", 14f, -1.7f, 8f, 1f,
            new Color(.17f,.25f,.27f), true);
        for (int i = 0; i < 4; i++)
            Block("Stepping stone " + (i + 1), 11f + i * 2f, .04f, 1.1f, .35f,
                new Color(.42f,.48f,.45f), true);
        Block("Left gear column", 28f, 1.1f, 2.2f, 2.2f, new Color(.32f,.4f,.37f), true);
        Block("Right gear column", 34f, 1.9f, 2.2f, 3.8f, new Color(.32f,.4f,.37f), true);
        Block("High gear platform", 37f, 3.8f, 4f, .4f, new Color(.42f,.48f,.45f), true);
        Block("Gear 3 placeholder", 37f, 4.55f, .7f, .7f, new Color(1f,.76f,.25f), false, true);
        Block("Oxalis glide start placeholder", 39f, 4.1f, .8f, .8f, new Color(.55f,.72f,.48f), false);
        Block("Mushroom alternate path placeholder", 42f, .55f, 1.6f, .55f, new Color(.43f,.65f,.57f), true);
        Block("Exit raised platform", 45.5f, 1.65f, 6f, .35f, new Color(.42f,.48f,.45f), true);
        Block("Final stone gate placeholder", 48f, 3f, 1.1f, 3f, new Color(.35f,.45f,.42f), true);

        Directory.CreateDirectory(Root + "/Scenes");
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), Second);
        EditorBuildSettings.scenes = new[] {
            new EditorBuildSettingsScene(First, true),
            new EditorBuildSettingsScene(Second, true)
        };
        AssetDatabase.SaveAssets();
        Debug.Log("Second level placeholder built: " + Second);
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

    static GameObject Block(string name, float x, float y, float width, float height,
        Color tint, bool solid, bool gear = false)
    {
        var go = new GameObject(name);
        go.transform.position = new Vector3(x, y, 0);
        go.transform.localScale = new Vector3(width, height, 1);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = square;
        renderer.color = tint;
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
