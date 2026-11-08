using System.Collections.Generic;
using UnityEngine;

namespace SeedGame
{
    // Keeps collision attached to the artist-positioned sprite in either level.
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class Level02SolidArt : MonoBehaviour
    {
        void Awake()
        {
            gameObject.layer = 6; // Same ground layer as the rest of the level.
            var sprite = GetComponent<SpriteRenderer>().sprite;
            if (!sprite) return;
            if (GetComponent<Collider2D>()) return;

            // The imported fallback PhysicsShape rounds the stair tops upward.
            // These three outlines sample the actual opaque pixels of the artist's
            // PNGs. In particular, the large column keeps its open stone arch.
            if (AddClimbingColumnCollider(sprite)) return;

            int pathCount = sprite.GetPhysicsShapeCount();
            if (pathCount == 0)
            {
                var box = gameObject.AddComponent<BoxCollider2D>();
                box.offset = new Vector2(sprite.bounds.center.x, sprite.bounds.center.y);
                box.size = new Vector2(sprite.bounds.size.x, sprite.bounds.size.y);
                return;
            }

            var collider = gameObject.AddComponent<PolygonCollider2D>();
            collider.pathCount = pathCount;
            var points = new List<Vector2>();
            for (int path = 0; path < pathCount; path++)
            {
                points.Clear();
                sprite.GetPhysicsShape(path, points);
                collider.SetPath(path, points);
            }
        }

        bool AddClimbingColumnCollider(Sprite sprite)
        {
            byte[] top;
            byte[] bottom;
            float firstPixel;
            float lastPixel;
            float imageWidth;
            switch (gameObject.name)
            {
                case "First climbing column":
                    imageWidth = 745f;
                    firstPixel = 0f;
                    lastPixel = 744f;
                    top = new byte[] { 81, 69, 69, 65, 65, 65, 64, 61, 48, 48, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 31, 28, 28, 11, 11, 11, 11, 11, 11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 154 };
                    bottom = new byte[] { 111, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 182 };
                    break;
                case "Tall climbing column":
                    imageWidth = 903f;
                    firstPixel = 6f;
                    lastPixel = 902f;
                    top = new byte[] { 48, 18, 18, 0, 0, 0, 0, 0, 0, 0, 0, 0, 13, 13, 13, 13, 13, 13, 13, 26, 26, 26, 26, 26, 26, 26, 26, 47, 47, 47, 47, 62, 62, 196, 206, 206, 206, 206, 206, 206, 206 };
                    bottom = new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 253, 253, 253, 253, 253, 253, 253, 227 };
                    break;
                case "Stone bridge before gate":
                    imageWidth = 2028f;
                    firstPixel = 6f;
                    lastPixel = 2026f;
                    top = new byte[] { 39, 28, 28, 23, 23, 23, 23, 16, 15, 15, 15, 15, 15, 18, 12, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 2 };
                    bottom = new byte[] { 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 254, 255, 255, 255, 254, 254, 254, 254, 254, 253, 21, 21, 23, 25, 28, 34, 42, 254, 255, 255, 255, 254, 254, 14, 14, 14, 14 };
                    break;
                default:
                    return false;
            }

            var outline = new List<Vector2>(top.Length * 4);
            for (int i = 0; i < top.Length; i++)
            {
                float pixelX = Mathf.Lerp(firstPixel, lastPixel, i / (float)(top.Length - 1));
                float x = sprite.bounds.min.x + pixelX / (imageWidth - 1f) * sprite.bounds.size.x;
                float y = sprite.bounds.max.y - top[i] / 255f * sprite.bounds.size.y;
                outline.Add(new Vector2(x, y - .035f)); // Sink feet into the painted rim.
                if (i == top.Length - 1 || top[i + 1] == top[i]) continue;
                float nextY = sprite.bounds.max.y - top[i + 1] / 255f * sprite.bounds.size.y - .035f;
                float nextPixelX = Mathf.Lerp(firstPixel, lastPixel, (i + 1f) / (top.Length - 1));
                float nextX = sprite.bounds.min.x + nextPixelX / (imageWidth - 1f) * sprite.bounds.size.x;
                // A vertical step stays inside the visible pixels; a diagonal
                // across a tall drop would create an invisible standing slope.
                outline.Add(top[i + 1] > top[i] ? new Vector2(x, nextY) : new Vector2(nextX, y - .035f));
            }
            for (int i = bottom.Length - 1; i >= 0; i--)
            {
                float pixelX = Mathf.Lerp(firstPixel, lastPixel, i / (float)(bottom.Length - 1));
                float x = sprite.bounds.min.x + pixelX / (imageWidth - 1f) * sprite.bounds.size.x;
                float y = sprite.bounds.max.y - bottom[i] / 255f * sprite.bounds.size.y + .035f;
                outline.Add(new Vector2(x, y));
                if (i == 0 || bottom[i - 1] == bottom[i]) continue;
                float nextY = sprite.bounds.max.y - bottom[i - 1] / 255f * sprite.bounds.size.y + .035f;
                float nextPixelX = Mathf.Lerp(firstPixel, lastPixel, (i - 1f) / (bottom.Length - 1));
                float nextX = sprite.bounds.min.x + nextPixelX / (imageWidth - 1f) * sprite.bounds.size.x;
                outline.Add(bottom[i - 1] < bottom[i] ? new Vector2(x, nextY) : new Vector2(nextX, y));
            }

            var shape = gameObject.AddComponent<PolygonCollider2D>();
            shape.pathCount = 1;
            shape.SetPath(0, outline.ToArray());
            return true;
        }
    }
}
