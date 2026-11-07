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
    }
}
