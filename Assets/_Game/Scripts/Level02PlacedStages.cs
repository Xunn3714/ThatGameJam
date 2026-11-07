using UnityEngine;

namespace SeedGame
{
    // Each frame keeps the position and scale set by the artist in the scene.
    public static class Level02PlacedStages
    {
        public static void Show(SpriteRenderer[] stages, int index)
        {
            if (stages == null) return;
            for (int i = 0; i < stages.Length; i++)
                if (stages[i]) stages[i].enabled = i == index;
        }

        public static InteractableHighlight HighlightFirst(SpriteRenderer[] stages)
        {
            if (stages == null || stages.Length == 0 || !stages[0]) return null;
            var renderer = stages[0];
            var highlight = renderer.GetComponent<InteractableHighlight>();
            if (!highlight) highlight = renderer.gameObject.AddComponent<InteractableHighlight>();
            highlight.Initialize(renderer, new Color(.72f, 1f, .68f));
            return highlight;
        }
    }
}
