using UnityEngine;

namespace SeedGame
{
    // A second SpriteRenderer draws only the sprite's alpha edge, so the cue follows animated frames.
    public sealed class InteractableHighlight : MonoBehaviour
    {
        SpriteRenderer source;
        SpriteRenderer outline;
        Material outlineMaterial;
        MaterialPropertyBlock properties;
        Color tint;
        bool visible = true;
        static readonly int GlowColor = Shader.PropertyToID("_GlowColor");

        public void Initialize(SpriteRenderer target, Color color)
        {
            source = target;
            tint = color;
            var shader = Resources.Load<Shader>("Shaders/InteractableOutline");
            if (!shader)
            {
                Debug.LogError("Missing interactable outline shader.");
                enabled = false;
                return;
            }
            outlineMaterial = new Material(shader);
            var child = new GameObject("Interactable outline");
            child.transform.SetParent(target.transform, false);
            outline = child.AddComponent<SpriteRenderer>();
            outline.sharedMaterial = outlineMaterial;
            outline.sprite = target.sprite;
            outline.sortingLayerID = target.sortingLayerID;
            outline.sortingOrder = target.sortingOrder + 1;
            properties = new MaterialPropertyBlock();
        }

        public void Hide()
        {
            visible = false;
            if (outline) outline.enabled = false;
        }

        void LateUpdate()
        {
            if (!outline || !source) return;
            outline.enabled = visible && source.enabled && source.sprite;
            if (!outline.enabled) return;
            outline.sprite = source.sprite;
            outline.flipX = source.flipX;
            outline.flipY = source.flipY;
            outline.sortingOrder = source.sortingOrder + 1;
            float alpha = .28f + .62f * (.5f + .5f * Mathf.Sin(Time.time * 4.2f));
            properties.SetColor(GlowColor, new Color(tint.r, tint.g, tint.b, tint.a * alpha));
            outline.SetPropertyBlock(properties);
        }

        void OnDestroy()
        {
            if (outlineMaterial) Destroy(outlineMaterial);
        }
    }
}
