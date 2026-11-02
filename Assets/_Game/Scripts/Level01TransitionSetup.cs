using UnityEngine;

namespace SeedGame
{
    public static class Level01TransitionSetup
    {
        public static void Install()
        {
            if (GameObject.Find("Gear 2 - ruins approach")) return;
            var glow = GameObject.Find("Gear destination glow");
            var exitObject = GameObject.Find("End of alpha");
            if (!glow || !exitObject)
            {
                Debug.LogError("First-level gear or exit marker is missing.");
                return;
            }

            // Reuse the existing art placeholder, without rewriting the user's scene layout.
            glow.name = "Gear 2 - ruins approach";
            glow.transform.position = new Vector3(44.7f, 1.55f, 0f);
            AddPickup(glow);
            var firstGear = Object.Instantiate(glow, glow.transform.parent);
            firstGear.name = "Gear 1 - stump";
            firstGear.transform.position = new Vector3(8f, 1.7f, 0f);

            exitObject.name = "Ruins entrance trigger";
            exitObject.transform.position = new Vector3(46.45f, 1.1f, 0f);
            var exit = exitObject.GetComponent<LevelExit>();
            exit.destinationScene = "Level02_Alpha";
            exit.requiredGears = 2;

            var sprite = glow.GetComponent<SpriteRenderer>().sprite;
            DoorPart("Ruins left jamb", sprite, 45.75f, 1.1f, .25f, 2.2f);
            DoorPart("Ruins right jamb", sprite, 47.05f, 1.1f, .25f, 2.2f);
            DoorPart("Ruins lintel", sprite, 46.4f, 2.32f, 1.55f, .25f);
            DoorPart("Ruins doorway shadow", sprite, 46.4f, 1.05f, 1.05f, 2.1f,
                new Color(.08f, .17f, .19f));
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

        static void DoorPart(string name, Sprite sprite, float x, float y, float width, float height,
            Color? tint = null)
        {
            var go = new GameObject(name);
            go.transform.position = new Vector3(x, y, 0f);
            go.transform.localScale = new Vector3(width, height, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = tint ?? new Color(.24f, .38f, .38f);
            renderer.sortingOrder = tint.HasValue ? 0 : 2;
        }
    }
}
