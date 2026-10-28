using UnityEngine;
using SeedGame;

public static class AlphaSceneFactory
{
    static Sprite square;
    static Sprite character;
    static Transform group;
    public static void Build(Sprite squareSprite, Sprite characterSprite)
    {
        square = squareSprite; character = characterSprite;
        group = new GameObject("First level alpha - editable objects").transform;
        CameraAndBackdrop();
        var player = CreatePlayer();
        Camera.main.GetComponent<SeedCamera>().target = player.transform;
        Ground("Starting earth", 0, 25, -2.4f, 2.4f);
        Ground("After gap earth", 30, 47, -2.4f, 2.4f);
        Solid("Jump tutorial stump", 8.0f, .45f, 1.15f, .9f, new Color(.28f,.27f,.34f));
        Solid("Practice wall", 19.2f, .9f, 1.45f, 1.8f, new Color(.27f,.3f,.36f));
        var first = Plant("01 - climb vine", 13.7f, .18f, new [] {
            new Vector4(15.1f,.65f,1.6f,.3f), new Vector4(16.7f,1.15f,1.6f,.3f),
            new Vector4(18.3f,1.65f,1.6f,.3f), new Vector4(20f,2.25f,1.75f,.3f)
        });
        var nearBridge = BridgeVine("02 - near-bank vine", 26f, .2f, 25.6f, 1.2f);
        var farBridge = BridgeVine("03 - far-bank vine", 30.8f, .2f, 28.15f, 3.9f);
        Trigger<Checkpoint>("Checkpoint before bridge", 22.3f, 1.2f, 1.4f, 2.4f);
        Trigger<Checkpoint>("Checkpoint after bridge", 32.1f, 1.2f, 1.4f, 2.4f);
        Decorative("Distant mother tree", 1.5f, 5.3f, 3f, 10f, new Color(.38f,.37f,.52f,.5f), -15);
        Decorative("Gear destination glow", 45.5f, 1.75f, 1f, 1f, new Color(1f,.74f,.34f), 1);
        Trigger<LevelExit>("End of alpha", 45.5f, 1.0f, 1.3f, 2.0f);
        var tutorial = new GameObject("Level 1 tutorial only").AddComponent<FirstLevelTutorial>();
        tutorial.player = player; tutorial.firstPlant = first;
        var flow = new GameObject("Reusable level completion").AddComponent<LevelFlow>();
        flow.player = player; flow.completionMessage = "第一关 Alpha 完成！按 R 重新开始";
        player.plants = new [] { first, nearBridge, farBridge };
    }
    static void CameraAndBackdrop()
    {
        var go = new GameObject("Main Camera"); go.tag = "MainCamera";
        var c = go.AddComponent<Camera>(); c.orthographic = true; c.clearFlags = CameraClearFlags.SolidColor;
        c.backgroundColor = new Color(.37f,.36f,.58f);
        go.AddComponent<AudioListener>();
        var follow = go.AddComponent<SeedCamera>();
        follow.minX = 0; follow.maxX = 47; follow.minY = -3; follow.maxY = 12;
        go.transform.position = new Vector3(2f, 2.1f, -10f);
        Decorative("Distant violet hills", 23.5f, 3.3f, 47, 7, new Color(.52f,.48f,.68f), -20);
        Decorative("Middle pale garden", 23.5f, 1.25f, 47, 4.2f, new Color(.66f,.59f,.69f), -18);
        for (int i = 0; i < 12; i++)
            Decorative("Simple midground tree " + i, 2 + i * 4.1f, 2.0f + (i%3)*.3f,
                .45f, 3.4f + (i%3), new Color(.43f,.44f,.55f,.66f), -16);
    }

    static SeedPlayer CreatePlayer()
    {
        var go = new GameObject("Player - seed sketch"); go.transform.position = new Vector3(2, .7f, 0);
        var body = go.AddComponent<Rigidbody2D>(); body.gravityScale = 2.7f;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        var col = go.AddComponent<BoxCollider2D>(); col.size = new Vector2(.58f, 1.1f);
        var feet = new GameObject("Feet ground probe").transform; feet.SetParent(go.transform);
        feet.localPosition = new Vector3(0, -.59f, 0);
        var art = new GameObject("Original character draft, no animation"); art.transform.SetParent(go.transform);
        art.transform.localPosition = new Vector3(.146f, .107f, 0);
        var sr = art.AddComponent<SpriteRenderer>();
        sr.sprite = character;
        sr.sortingOrder = 5;
        var player = go.AddComponent<SeedPlayer>(); player.feet = feet;
        return player;
    }

    static ResonancePlant Plant(string name, float x, float y, Vector4[] leaves)
    {
        var root = new GameObject(name); root.transform.position = new Vector3(x,y,0);
        var plant = root.AddComponent<ResonancePlant>();
        plant.generatePlaceholderSteps = true;
        plant.oneWayPlatforms = true;
        plant.placeholderSteps = leaves;
        var dormant = Decorative("Curled sleeping bud", x, y+.35f, .55f, .7f,
            new Color(.39f,.46f,.42f), 3);
        plant.dormantVisual = dormant;
        var grown = new GameObject("Grown platforms and leaves"); grown.transform.SetParent(root.transform);
        plant.grownRoot = grown;
        foreach (var leaf in leaves)
        {
            var slab = Solid("Solid leaf platform", leaf.x, leaf.y, leaf.z, leaf.w,
                new Color(.43f,.65f,.47f));
            slab.transform.SetParent(grown.transform);
            var stalk = Decorative("Thin stem", (x+leaf.x)*.5f, (y+leaf.y)*.5f,
                .10f, Vector2.Distance(new Vector2(x,y), new Vector2(leaf.x,leaf.y)),
                new Color(.35f,.54f,.43f), 2);
            stalk.transform.SetParent(grown.transform);
        }
        grown.SetActive(false);
        return plant;
    }

    static ResonancePlant BridgeVine(string name, float x, float y, float bridgeX, float bridgeWidth)
    {
        var root = new GameObject(name); root.transform.position = new Vector3(x, y, 0);
        var plant = root.AddComponent<ResonancePlant>();
        plant.dormantVisual = Decorative("Curled sleeping bridge vine", x, y + .35f,
            .55f, .7f, new Color(.39f, .46f, .42f), 3);
        var grown = new GameObject("Grown continuous vine bridge");
        grown.transform.SetParent(root.transform);
        plant.grownRoot = grown;
        var bridge = Solid("Continuous vine crossing", bridgeX, .35f, bridgeWidth, .3f,
            new Color(.42f, .65f, .44f));
        bridge.transform.SetParent(grown.transform);
        var highlight = Decorative("Vine top highlight", bridgeX, .53f, bridgeWidth, .07f,
            new Color(.66f, .82f, .54f), 2);
        highlight.transform.SetParent(grown.transform);
        grown.SetActive(false);
        return plant;
    }

    static void Ground(string name, float left, float right, float bottom, float height)
    {
        Solid(name, (left+right)*.5f, bottom+height*.5f, right-left, height,
            new Color(.18f,.21f,.28f));
        Decorative(name+" top rim", (left+right)*.5f, bottom+height+.04f,
            right-left, .12f, new Color(.52f,.57f,.49f), 1);
    }
    static GameObject Solid(string name, float x, float y, float w, float h, Color color)
    {
        var go = Decorative(name,x,y,w,h,color,0);
        go.layer = 6;
        var col = go.AddComponent<BoxCollider2D>(); col.size = Vector2.one;
        return go;
    }
    static GameObject Decorative(string name, float x, float y, float w, float h, Color color, int order)
    {
        var go = new GameObject(name); go.transform.SetParent(group);
        go.transform.position = new Vector3(x,y,0); go.transform.localScale = new Vector3(w,h,1);
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = square; sr.color = color; sr.sortingOrder = order;
        return go;
    }
    static T Trigger<T>(string name, float x, float y, float w, float h) where T : Component
    {
        var go = new GameObject(name); go.transform.position = new Vector3(x,y,0);
        var col = go.AddComponent<BoxCollider2D>(); col.size = new Vector2(w,h); col.isTrigger = true;
        return go.AddComponent<T>();
    }
}
