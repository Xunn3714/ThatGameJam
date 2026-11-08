using System.Collections.Generic;
using UnityEngine;

namespace SeedGame
{
    // Level 1 was built with invisible prototype colliders next to its artwork.
    // Use the same sprite-attached collision as Level 2 for the two solid props.
    public static class Level01CollisionSync
    {
        public static void Install()
        {
            Replace("Jump tutorial stump", "Stump artwork");
            Replace("Practice wall", "Wall artwork");
            if (!GameObject.Find("Vine crossing seam"))
            {
                var near = GameObject.Find("02 - near-bank vine");
                var far = GameObject.Find("03 - far-bank vine");
                if (near && far)
                {
                    var seam = new GameObject("Vine crossing seam");
                    seam.layer = 6;
                    var connector = seam.AddComponent<Level01VineSeam>();
                    connector.near = near.GetComponent<ResonancePlant>();
                    connector.far = far.GetComponent<ResonancePlant>();
                }
            }
        }

        static void Replace(string prototypeName, string artworkName)
        {
            var prototype = GameObject.Find(prototypeName);
            var artwork = GameObject.Find(artworkName);
            var renderer = artwork ? artwork.GetComponent<SpriteRenderer>() : null;
            if (!renderer) return;

            if (artworkName == "Wall artwork")
                TraceBrokenPillar(artwork, renderer);
            else if (!artwork.GetComponent<Collider2D>() &&
                !artwork.GetComponent<Level02SolidArt>())
                artwork.AddComponent<Level02SolidArt>();
            var artCollider = artwork.GetComponent<Collider2D>();
            if (!artCollider) return;
            artCollider.enabled = true;
            var oldCollider = prototype ? prototype.GetComponent<Collider2D>() : null;
            if (oldCollider) oldCollider.enabled = false;
        }

        static void TraceBrokenPillar(GameObject artwork, SpriteRenderer renderer)
        {
            var sprite = renderer.sprite;
            if (!sprite) return;
            artwork.layer = 6;
            var collider = artwork.GetComponent<PolygonCollider2D>();
            if (!collider) collider = artwork.AddComponent<PolygonCollider2D>();
            // Use the sprite's alpha outline. The previous hand-drawn polygon
            // enclosed the empty triangle beneath the sloping stone.
            var path = new List<Vector2>();
            var outlines = new List<Vector2[]>();
            var areas = new List<float>();
            float largestArea = 0f;
            for (int i = 0; i < sprite.GetPhysicsShapeCount(); i++)
            {
                path.Clear();
                sprite.GetPhysicsShape(i, path);
                if (path.Count < 3) continue;
                float twiceArea = 0f;
                for (int j = 0; j < path.Count; j++)
                {
                    var a = path[j];
                    var b = path[(j + 1) % path.Count];
                    twiceArea += a.x * b.y - b.x * a.y;
                }
                float area = Mathf.Abs(twiceArea);
                largestArea = Mathf.Max(largestArea, area);
                outlines.Add(path.ToArray());
                areas.Add(area);
            }
            if (outlines.Count > 0)
            {
                // Keep the upright and the diagonal; ignore small detached rubble.
                int kept = 0;
                for (int i = 0; i < outlines.Count; i++)
                    if (areas[i] >= largestArea * .08f) kept++;
                collider.pathCount = kept;
                int pathIndex = 0;
                for (int i = 0; i < outlines.Count; i++)
                    if (areas[i] >= largestArea * .08f)
                        collider.SetPath(pathIndex++, outlines[i]);
            }
            else
                Debug.LogError("Wall artwork has no sprite physics outline.", artwork);
        }
    }

    // Keep each vine's artwork-shaped walking edge and join only their tips.
    public sealed class Level01VineSeam : MonoBehaviour
    {
        public ResonancePlant near;
        public ResonancePlant far;

        void Update()
        {
            if (!near || !far || !near.BridgeFullyGrown || !far.BridgeFullyGrown) return;
            var a = near.bridgeRenderer ? near.bridgeRenderer.GetComponent<EdgeCollider2D>() : null;
            var b = far.bridgeRenderer ? far.bridgeRenderer.GetComponent<EdgeCollider2D>() : null;
            if (!a || !b || !a.enabled || !b.enabled || a.pointCount < 2 || b.pointCount < 2) return;
            transform.position = Vector3.zero;
            JoinRightBank(b);
            // The finished sprites meet near x=26. Reach a little way into
            // both edges to avoid a physics seam at their exact endpoints.
            float left = a.bounds.max.x - .22f;
            float right = b.bounds.min.x + .22f;
            if (right <= left) { enabled = false; return; }
            float leftHeight = HeightAt(a, left);
            float rightHeight = HeightAt(b, right);
            var bridge = gameObject.AddComponent<EdgeCollider2D>();
            bridge.points = new[] {
                new Vector2(left, leftHeight),
                new Vector2(right, rightHeight)
            };
            MakeOneWay(bridge);
            enabled = false;
        }

        void JoinRightBank(EdgeCollider2D vine)
        {
            var ground = GameObject.Find("After gap earth")?.GetComponent<Collider2D>();
            if (!ground) return;
            // The right vine's visible end stops just short of the bank.
            // Overlap both surfaces slightly so walking back left cannot fall
            // through their join.
            float bankX = ground.bounds.min.x + .32f;
            float vineX = Mathf.Min(vine.bounds.max.x - .18f, bankX - .22f);
            var join = new GameObject("Far vine to right bank");
            join.layer = 6;
            join.transform.SetParent(transform, false);
            var edge = join.AddComponent<EdgeCollider2D>();
            edge.points = new[] {
                new Vector2(vineX, HeightAt(vine, vineX)),
                new Vector2(bankX, ground.bounds.max.y + .02f)
            };
            MakeOneWay(edge);
        }

        static void MakeOneWay(Collider2D collider)
        {
            var effector = collider.gameObject.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.useOneWayGrouping = true;
            effector.surfaceArc = 175f;
            effector.useSideFriction = false;
            effector.useSideBounce = false;
            collider.usedByEffector = true;
        }

        static float HeightAt(EdgeCollider2D edge, float worldX)
        {
            var points = edge.points;
            Vector3 closest = edge.transform.TransformPoint(points[0] + edge.offset);
            float closestDistance = Mathf.Abs(closest.x - worldX);
            for (int i = 1; i < points.Length; i++)
            {
                Vector3 next = edge.transform.TransformPoint(points[i] + edge.offset);
                float distance = Mathf.Abs(next.x - worldX);
                if (distance < closestDistance) { closest = next; closestDistance = distance; }
                Vector3 previous = edge.transform.TransformPoint(points[i - 1] + edge.offset);
                if (worldX >= Mathf.Min(previous.x, next.x) &&
                    worldX <= Mathf.Max(previous.x, next.x))
                {
                    float width = next.x - previous.x;
                    float t = Mathf.Abs(width) < .0001f ? 0f : (worldX - previous.x) / width;
                    return Mathf.Lerp(previous.y, next.y, t);
                }
            }
            return closest.y;
        }
    }
}
