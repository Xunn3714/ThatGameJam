using UnityEngine;

namespace SeedGame
{
    public static class Level01TransitionSetup
    {
        public static void Install()
        {
            if (GameObject.Find("Gear 2 - ruins approach"))
            {
                InstallDoorArt();
                return;
            }
            var glow = GameObject.Find("Gear destination glow");
            var exitObject = GameObject.Find("End of alpha");
            if (!glow || !exitObject)
            {
                Debug.LogError("First-level gear or exit marker is missing.");
                return;
            }

            // Reuse the existing art placeholder, without rewriting the user's scene layout.
            glow.name = "Gear 2 - ruins approach";
            glow.transform.position = new Vector3(41.3f, 1.55f, 0f);
            AddPickup(glow);
            var firstGear = Object.Instantiate(glow, glow.transform.parent);
            firstGear.name = "Gear 1 - stump";
            firstGear.transform.position = new Vector3(8f, 1.7f, 0f);

            exitObject.name = "Ruins entrance trigger";
            exitObject.transform.position = new Vector3(46.45f, 1.1f, 0f);
            var exit = exitObject.GetComponent<LevelExit>();
            exit.destinationScene = "Level02_Alpha";
            exit.requiredGears = 2;

            InstallDoorArt();
        }

        public static void InstallDoorArt()
        {
            // Keep any subsequent hand placement in the Scene view.
            if (GameObject.Find("Ruins entrance painted art") ||
                (GameObject.Find("Ruins entrance rear half") &&
                 GameObject.Find("Ruins entrance front half"))) return;
            foreach (var name in new[] { "Ruins left jamb", "Ruins right jamb",
                "Ruins lintel", "Ruins doorway shadow" })
            {
                var oldPart = GameObject.Find(name);
                if (oldPart) Object.DestroyImmediate(oldPart);
            }
            var rear = Resources.Load<Sprite>("Level02/石门-后");
            var front = Resources.Load<Sprite>("Level02/石门-前");
            if (!rear || !front)
            {
                Debug.LogError("Missing ruins entrance door sprites.");
                return;
            }
            var group = new GameObject("Ruins entrance painted art").transform;
            DoorHalf("Ruins entrance rear half", group, rear, 46.4f, 1.3f, 3f, 2);
            DoorHalf("Ruins entrance front half", group, front, 47.2f, 1.35f, 1.4f, 9);
        }

        static void AddPickup(GameObject go)
        {
            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = .72f;
            collider.isTrigger = true;
            go.AddComponent<GearPickup>();
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = Resources.Load<Sprite>("UI/Gear") ?? renderer.sprite;
            renderer.color = Color.white;
            renderer.sortingOrder = 8;
        }

        static void DoorHalf(string name, Transform group, Sprite sprite,
            float x, float y, float width, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(group);
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = Vector3.one * (width / sprite.bounds.size.x);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
        }
    }
}
