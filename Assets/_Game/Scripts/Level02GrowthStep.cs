using UnityEngine;

namespace SeedGame
{
    public sealed class Level02GrowthStep : MonoBehaviour
    {
        public SpriteRenderer visual;
        public Sprite[] frames;
        public float worldWidth = 1.8f;

        public void Show(float progress)
        {
            if (!visual || frames == null || frames.Length == 0) return;
            visual.sprite = frames[Mathf.Min(frames.Length - 1,
                Mathf.FloorToInt(progress * frames.Length))];
            float scale = worldWidth / Mathf.Max(.01f, visual.sprite.bounds.size.x);
            var parentScale = transform.parent.lossyScale;
            float grow = Mathf.SmoothStep(.15f, 1f, progress);
            visual.transform.localScale = new Vector3(scale * grow / parentScale.x,
                scale * grow / parentScale.y, 1f);
        }
    }
}
