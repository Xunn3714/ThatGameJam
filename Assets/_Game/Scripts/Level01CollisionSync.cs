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
        }

        static void Replace(string prototypeName, string artworkName)
        {
            var prototype = GameObject.Find(prototypeName);
            var artwork = GameObject.Find(artworkName);
            if (!artwork || !artwork.GetComponent<SpriteRenderer>()) return;

            if (!artwork.GetComponent<Collider2D>() &&
                !artwork.GetComponent<Level02SolidArt>())
                artwork.AddComponent<Level02SolidArt>();
            var artCollider = artwork.GetComponent<Collider2D>();
            if (!artCollider) return;
            artCollider.enabled = true;
            var oldCollider = prototype ? prototype.GetComponent<Collider2D>() : null;
            if (oldCollider) oldCollider.enabled = false;
        }
    }
}
