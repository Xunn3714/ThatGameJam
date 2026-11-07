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
        float pulseSpeed = 3.2f;
        float intensity = 1f;
        int sortingOffset = 1;
        static readonly int GlowColor = Shader.PropertyToID("_GlowColor");
        static readonly int CorePixels = Shader.PropertyToID("_CorePixels");
        static readonly int GlowPixels = Shader.PropertyToID("_GlowPixels");
        static readonly int CoreOpacity = Shader.PropertyToID("_CoreOpacity");
        static readonly int GlowOpacity = Shader.PropertyToID("_GlowOpacity");
        static readonly int SpriteUVRect = Shader.PropertyToID("_SpriteUVRect");
        static readonly int SpriteBounds = Shader.PropertyToID("_SpriteBounds");
        static readonly int PixelWorldSize = Shader.PropertyToID("_PixelWorldSize");
        static readonly int GeometryPaddingPixels = Shader.PropertyToID("_GeometryPaddingPixels");

        void Awake()
        {
            // Keep the per-renderer property block valid even if this component is
            // added from a scene object before its owner calls Initialize.
            properties = new MaterialPropertyBlock();
        }

        public void Initialize(SpriteRenderer target, Color color)
        {
            if (!target)
            {
                Debug.LogWarning("Interactable highlight needs a SpriteRenderer target.", this);
                enabled = false;
                return;
            }

            source = target;
            tint = color;
            if (properties == null) properties = new MaterialPropertyBlock();
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
        }

        public void Hide()
        {
            visible = false;
            if (outline) outline.enabled = false;
        }

        public void Show() => visible = true;

        public void ConfigurePulse(float speed, float strength, int orderOffset = 1)
        {
            pulseSpeed = Mathf.Max(.1f, speed);
            intensity = Mathf.Clamp01(strength);
            sortingOffset = orderOffset;
        }

        void LateUpdate()
        {
            if (!outline || !source) return;
            // Defensive recovery for components restored after an editor/domain reload.
            if (properties == null) properties = new MaterialPropertyBlock();
            outline.enabled = visible && source.enabled && source.sprite;
            if (!outline.enabled) return;
            outline.sprite = source.sprite;
            outline.flipX = source.flipX;
            outline.flipY = source.flipY;
            outline.sortingOrder = source.sortingOrder + sortingOffset;
            float pulse = .5f + .5f * Mathf.Sin(Time.time * pulseSpeed);
            pulse = pulse * pulse * (3f - 2f * pulse);
            properties.SetColor(GlowColor, new Color(tint.r, tint.g, tint.b, tint.a));
            properties.SetFloat(CorePixels, 2.25f);
            properties.SetFloat(GlowPixels, Mathf.Lerp(5.75f, 8.5f, pulse));
            properties.SetFloat(CoreOpacity, .92f * intensity);
            properties.SetFloat(GlowOpacity, Mathf.Lerp(.2f, .56f, pulse) * intensity);
            ApplySpriteGeometry(source.sprite);
            outline.SetPropertyBlock(properties);
        }

        void ApplySpriteGeometry(Sprite sprite)
        {
            Rect textureRect = sprite.textureRect;
            Texture2D texture = sprite.texture;
            Bounds bounds = sprite.bounds;
            properties.SetVector(SpriteUVRect, new Vector4(
                textureRect.x / texture.width, textureRect.y / texture.height,
                textureRect.width / texture.width, textureRect.height / texture.height));
            properties.SetVector(SpriteBounds, new Vector4(
                bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y));
            properties.SetVector(PixelWorldSize, new Vector4(
                bounds.size.x / textureRect.width, bounds.size.y / textureRect.height, 0f, 0f));
            properties.SetFloat(GeometryPaddingPixels, 10f);
        }

        void OnDestroy()
        {
            if (outlineMaterial) Destroy(outlineMaterial);
        }
    }
}
