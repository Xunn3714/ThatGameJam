using UnityEngine;

namespace SeedGame
{
    // Level 1 composition only. Player, plant and camera behaviour remains reusable in other scenes.
    public static class Level01ArtFactory
    {
        static Sprite Art(string path) => Resources.Load<Sprite>("Level01/" + path);

        public static void Apply(Transform group, SeedPlayer player)
        {
            Camera.main.backgroundColor = new Color(231f / 255f, 228f / 255f, 219f / 255f);
            RemoveNamed("Distant violet hills");
            RemoveNamed("Middle pale garden");
            RemoveNamed("Distant mother tree");
            RemoveNamed("Starting earth top rim");
            RemoveNamed("After gap earth top rim");
            for (int i = 0; i < 12; i++) RemoveNamed("Simple midground tree " + i);

            Ground("Starting earth", 25);
            Ground("After gap earth", 17);
            HideCollisionSprite("Jump tutorial stump");
            HideCollisionSprite("Practice wall");
            Picture("Stump artwork", group, "跳跃树桩", 8, .32f, 1.55f, .9f, 1);
            Picture("Wall artwork", group, "墙", 20.65f, .8f, 2.2f, 1.8f, 1);

            var plants = player.plants;
            if (plants != null && plants.Length >= 3)
            {
                var leaves = plants[0];
                if (leaves.grownRoot)
                {
                    Remove(leaves.grownRoot);
                    leaves.grownRoot = null;
                }
                float aScale = 1.6f * 1.12f / Art("植物a/a3").bounds.size.x;
                SetDormant(leaves, "植物a/a1", aScale);
                var a = leaves.dormantVisual.GetComponent<SpriteRenderer>();
                a.transform.position = new Vector3(15.1f, .20f, 0);
                var b = SleepingPlant("B", group, "植物b/b1", "植物b/b3", 16.7f, .22f, 1.6f);
                var c = SleepingPlant("C", group, "植物c/c1", "植物c/c3", 18.6f, .30f, 1.75f);
                leaves.stepArtRenderers = new[] { a, b, c };
                leaves.plantFrames = null;
                leaves.stepFramesA = Frames("植物a/a");
                leaves.stepFramesB = Frames("植物b/b");
                leaves.stepFramesC = Frames("植物c/c");
                leaves.growthFrameSeconds = .14f;
                Bridge(plants[1], "植物d/d", 1.2f, .27f, false);
                Bridge(plants[2], "植物f/f", 4.1f, .64f, true);
            }

            var sky = Layer("Level01 Art - Sky", group, 0, 1);
            Picture("Sky gradient", sky, "天空", 2, 4.25f, 21.1f, 8.5f, -40);

            var far = Layer("Level01 Art - Far mountains and mother tree", group, .25f, 1);
            float[] hillX = { 4.5f, 22.3f, 40.1f };
            for (int i = 0; i < 3; i++)
            {
                var sprite = Art("远景山丘" + (i + 1));
                float height = Height(sprite, 18);
                Picture("Far hill " + (i + 1), far, sprite, hillX[i], -.2f + height * .5f, 18, height, -30);
            }
            var mother = Art("母树（远景）");
            Picture("Mother tree", far, mother, 3.7f, 4.32f, 8, Height(mother, 8), -27);

            var middle = Layer("Level01 Art - Middle garden", group, .55f, 1);
            float[] middleX = { 3, 9.5f, 13.5f, 16, 27, 33.5f, 38.5f, 42 };
            float[] middleW = { 9.6f, 7.5f, 6, 4.5f, 8.5f, 7, 6, 5 };
            for (int i = 0; i < middleX.Length; i++)
            {
                var sprite = Art("中景" + ((i % 4) + 1));
                float height = Height(sprite, middleW[i]);
                Picture("Middle garden " + (i + 1), middle, sprite, middleX[i], -.1f + height * .5f,
                    middleW[i], height, -20 - (i % 2));
            }

            var trees = Layer("Level01 Art - Middle trees", group, .7f, 1);
            for (int i = 0; i < 12; i++)
            {
                var sprite = Art("树木" + ((i % 8) + 1));
                float[] heights = { 3.6f, 4.5f, 2.8f, 3.2f, 4, 2.5f, 3.6f, 4.5f };
                float height = heights[i % heights.Length];
                float width = height * sprite.bounds.size.x / sprite.bounds.size.y;
                Picture("Middle tree " + (i + 1), trees, sprite, 2 + i * 4.1f,
                    height * .5f - .12f, width, height, -17);
            }

            var front = Layer("Level01 Art - Foreground", group, 1.18f, 1);
            float[] rockX = { -2, 2, 6, 10, 14, 18, 22, 26, 30, 34, 38, 42, 46, 50, 54 };
            for (int i = 0; i < rockX.Length; i++)
            {
                var sprite = Art(i % 2 == 0 ? "前景石堆a" : "前景石堆b");
                float height = Height(sprite, 4.6f);
                Picture("Foreground rocks " + (i + 1), front, sprite, rockX[i],
                    -1.6f + height * .5f, 4.6f, height, 11);
            }
            string[] leafPaths = { "前景叶片a（浅色）", "前景叶片c（深色）", "前景叶片b（浅色）", "前景叶片d（深色）" };
            float[] leafX = { -1, 4, 9, 14, 19, 24, 29, 34, 39, 44, 49, 54 };
            for (int i = 0; i < leafX.Length; i++)
            {
                var sprite = Art(leafPaths[i % leafPaths.Length]);
                float height = Height(sprite, 1.4f);
                Picture("Foreground leaves " + (i + 1), front, sprite, leafX[i],
                    -1.15f + height * .5f, 1.4f, height, 12);
            }
        }

        static Sprite[] Frames(string prefix) => new[] {
            Art(prefix + "1"), Art(prefix + "1_5_ai"), Art(prefix + "2"),
            Art(prefix + "2_5_ai"), Art(prefix + "3")
        };
        static float Height(Sprite sprite, float width) => width * sprite.bounds.size.y / sprite.bounds.size.x;

        static Transform Layer(string name, Transform parent, float horizontal, float vertical)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var layer = go.AddComponent<ParallaxLayer>();
            layer.cameraTarget = Camera.main.transform;
            layer.horizontalScreenSpeed = horizontal;
            layer.verticalScreenSpeed = vertical;
            return go.transform;
        }

        static SpriteRenderer Picture(string name, Transform parent, string path,
            float x, float y, float width, float height, int order) =>
            Picture(name, parent, Art(path), x, y, width, height, order);

        static SpriteRenderer Picture(string name, Transform parent, Sprite sprite,
            float x, float y, float width, float height, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(x, y, 0);
            go.transform.localScale = new Vector3(width / sprite.bounds.size.x, height / sprite.bounds.size.y, 1);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            return renderer;
        }

        static void Ground(string name, float width)
        {
            var go = GameObject.Find(name);
            var collider = go.GetComponent<BoxCollider2D>();
            go.transform.localScale = Vector3.one;
            go.transform.position += new Vector3(0, .4f, 0);
            collider.offset = new Vector2(0, -.4f);
            collider.size = new Vector2(width, 2.4f);
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = Art("地板");
            renderer.color = Color.white;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = new Vector2(width, 2.4f);
        }

        static void HideCollisionSprite(string name)
        {
            var renderer = GameObject.Find(name).GetComponent<SpriteRenderer>();
            renderer.enabled = false;
            if (Application.isPlaying) Object.Destroy(renderer);
            else Object.DestroyImmediate(renderer);
        }

        static void SetDormant(ResonancePlant plant, string path, float scale)
        {
            var renderer = plant.dormantVisual.GetComponent<SpriteRenderer>();
            renderer.sprite = Art(path);
            renderer.color = Color.white;
            renderer.transform.localScale = new Vector3(scale, scale, 1);
            var center = renderer.transform.position;
            renderer.transform.position = new Vector3(center.x, spriteHeight(renderer.sprite, scale) * .5f - .08f, center.z);
        }

        static float spriteHeight(Sprite sprite, float scale) => sprite.bounds.size.y * scale;

        static SpriteRenderer SleepingPlant(string label, Transform parent, string initial, string grown,
            float x, float y, float platformWidth)
        {
            var sprite = Art(initial);
            float scale = platformWidth * 1.12f / Art(grown).bounds.size.x;
            return Picture("Sleeping plant " + label, parent, sprite, x, y,
                sprite.bounds.size.x * scale, sprite.bounds.size.y * scale, 3);
        }

        static void Bridge(ResonancePlant plant, string prefix, float width, float contactTop, bool fromRight)
        {
            SetDormant(plant, prefix + "1", fromRight ? .34f : .42f);
            var collider = plant.grownRoot.GetComponentInChildren<BoxCollider2D>(true);
            var renderer = collider.GetComponent<SpriteRenderer>();
            var final = Art(prefix + "3");
            renderer.sprite = final;
            renderer.color = Color.white;
            float scale = width / final.bounds.size.x;
            renderer.transform.localScale = new Vector3(scale, scale, 1);
            float artCenterY = final.bounds.size.y * scale * .5f - .08f;
            var center = renderer.transform.position;
            renderer.transform.position = new Vector3(center.x, artCenterY, center.z);
            collider.size = new Vector2(width / scale, .18f / scale);
            collider.offset = new Vector2(0, (contactTop - .09f - artCenterY) / scale);
            var highlight = plant.grownRoot.transform.Find("Vine top highlight");
            if (highlight) Remove(highlight.gameObject);
            plant.bridgeFrames = Frames(prefix);
            plant.bridgeRenderer = renderer;
            plant.bridgeGrowsFromRight = fromRight;
            plant.growthFrameSeconds = .14f;
        }

        static void RemoveNamed(string name)
        {
            var go = GameObject.Find(name);
            if (go) Remove(go);
        }

        static void Remove(GameObject go)
        {
            if (Application.isPlaying) Object.Destroy(go);
            else Object.DestroyImmediate(go);
        }
    }
}
