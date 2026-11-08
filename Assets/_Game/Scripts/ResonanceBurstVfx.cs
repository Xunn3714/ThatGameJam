using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SeedGame
{
    // The layered wave shows the current resonance range without lighting the player.
    // Gameplay range stays in SeedPlayer; this component owns presentation only.
    [DisallowMultipleComponent]
    public sealed class ResonanceBurstVfx : MonoBehaviour
    {
        const int RingSegments = 96;
        // The pulse checks plant roots, while vine artwork reaches past them.
        // Keep the visible wave inside the gameplay radius so its edge does
        // not imply that a root just beyond the range was activated.
        const float VisualRadiusInset = .45f;
        static readonly Color InnerColor = new Color(.42f, .83f, .87f, 1f);
        static readonly Color MiddleColor = new Color(.68f, .63f, .92f, 1f);
        static readonly Color EdgeColor = new Color(1f, .79f, .28f, 1f);
        static readonly int Opacity = Shader.PropertyToID("_Opacity");

        Material glowMaterial;
        Texture2D whiteBandTexture;
        Sprite whiteBandSprite;
        readonly List<GameObject> activeBursts = new List<GameObject>();
        float previousRadius;

        void Awake() => EnsureResources();

        public void Play(Vector3 center, float targetRadius, int stage)
        {
            if (targetRadius <= 0f) return;
            EnsureResources();
            if (!glowMaterial) return;
            float visualRadius = Mathf.Max(.16f, targetRadius - VisualRadiusInset);
            float startRadius = stage > 1 ? previousRadius : .16f;
            previousRadius = visualRadius;
            StopAllCoroutines();
            foreach (var burst in activeBursts)
                if (burst) Destroy(burst);
            activeBursts.Clear();
            StartCoroutine(AnimateBurst(center, startRadius, visualRadius, Mathf.Clamp(stage, 1, 4)));
        }

        IEnumerator AnimateBurst(Vector3 center, float startRadius, float targetRadius, int stage)
        {
            var root = new GameObject("Resonance call burst");
            root.transform.position = center;
            activeBursts.Add(root);

            var whiteBand = new GameObject("Soft white resonance light").AddComponent<SpriteRenderer>();
            whiteBand.transform.SetParent(root.transform, false);
            whiteBand.sprite = whiteBandSprite;
            whiteBand.sortingOrder = 18;
            var softBand = CreateRing(root.transform, "Cool inner resonance band", 19);
            var paleCrest = CreateRing(root.transform, "Violet resonance crest", 20);
            var outerEdge = CreateRing(root.transform, "Warm yellow resonance edge", 21);
            var properties = new MaterialPropertyBlock();

            float strength = 1f + (stage - 1) * .04f;
            float elapsed = 0f;
            const float duration = 1.65f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float travel = Mathf.Clamp01((elapsed - .035f) / 1.15f);
                float appear = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / .12f));
                float fade = 1f - Mathf.SmoothStep(0f, 1f,
                    Mathf.Clamp01((elapsed - .42f) / 1.23f));
                float opacity = appear * fade;
                float radius = Mathf.Lerp(startRadius, targetRadius, EaseOutCubic(travel));

                float reveal = Mathf.SmoothStep(0f, 1f,
                    Mathf.Clamp01((radius - .3f) / .5f));
                // Restore the broad white wave, with a dark centre and a much
                // lower peak alpha than the original near-opaque version.
                whiteBand.transform.localScale = Vector3.one * radius;
                whiteBand.color = new Color(1f, 1f, 1f, opacity * reveal * .22f);
                float innerWidth = Mathf.Lerp(.21f, .15f, travel) * strength;
                float middleWidth = Mathf.Lerp(.15f, .11f, travel) * strength;
                float outerWidth = Mathf.Lerp(.12f, .09f, travel) * strength;
                // Neighbouring coloured bands overlap slightly so no clear line
                // appears between the blue, violet and yellow parts of the wave.
                float middleRadius = radius - (outerWidth + middleWidth) * .45f;
                float innerRadius = middleRadius - (middleWidth + innerWidth) * .45f;
                SetRing(softBand, Mathf.Max(.06f, innerRadius), innerWidth,
                    InnerColor, opacity * reveal * .35f, properties);
                SetRing(paleCrest, Mathf.Max(.1f, middleRadius), middleWidth,
                    MiddleColor, opacity * reveal * .35f, properties);
                SetRing(outerEdge, radius, outerWidth,
                    EdgeColor, opacity * reveal * .47f, properties);
                yield return null;
            }

            activeBursts.Remove(root);
            Destroy(root);
        }

        LineRenderer CreateRing(Transform parent, string name, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var ring = go.AddComponent<LineRenderer>();
            ring.useWorldSpace = false;
            ring.loop = true;
            ring.positionCount = RingSegments;
            ring.numCornerVertices = 2;
            ring.numCapVertices = 2;
            ring.sharedMaterial = glowMaterial;
            ring.sortingOrder = sortingOrder;
            return ring;
        }

        static void SetRing(LineRenderer ring, float radius, float width, Color color,
            float alpha, MaterialPropertyBlock properties)
        {
            ring.widthMultiplier = width;
            ring.startColor = color;
            ring.endColor = color;
            properties.SetFloat(Opacity, Mathf.Clamp01(alpha));
            ring.SetPropertyBlock(properties);
            for (int i = 0; i < RingSegments; i++)
            {
                float angle = i * Mathf.PI * 2f / RingSegments;
                ring.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f));
            }
        }

        void EnsureResources()
        {
            if (!glowMaterial)
            {
                Shader shader = Resources.Load<Shader>("Shaders/ResonanceGlow");
                if (!shader)
                {
                    Debug.LogError("Missing resonance glow shader.");
                    enabled = false;
                    return;
                }
                glowMaterial = new Material(shader) { name = "Runtime resonance glow" };
            }
            if (whiteBandSprite) return;
            const int size = 256;
            var pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + .5f - size * .5f) / (size * .5f);
                    float dy = (y + .5f - size * .5f) / (size * .5f);
                    float distance = Mathf.Sqrt(dx * dx + dy * dy);
                    float inner = Mathf.SmoothStep(0f, 1f,
                        Mathf.InverseLerp(.54f, .8f, distance));
                    float outer = 1f - Mathf.SmoothStep(0f, 1f,
                        Mathf.InverseLerp(.85f, 1f, distance));
                    pixels[y * size + x] = new Color32(255, 255, 255,
                        (byte)Mathf.RoundToInt(255f * inner * outer));
                }
            whiteBandTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Runtime soft white resonance band",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            whiteBandTexture.SetPixels32(pixels);
            whiteBandTexture.Apply();
            whiteBandSprite = Sprite.Create(whiteBandTexture,
                new Rect(0, 0, size, size), new Vector2(.5f, .5f), size * .5f);
        }

        static float EaseOutCubic(float value) => 1f - Mathf.Pow(1f - value, 3f);

        void OnDestroy()
        {
            for (int i = activeBursts.Count - 1; i >= 0; i--)
                if (activeBursts[i]) Destroy(activeBursts[i]);
            activeBursts.Clear();
            if (glowMaterial) Destroy(glowMaterial);
            if (whiteBandSprite) Destroy(whiteBandSprite);
            if (whiteBandTexture) Destroy(whiteBandTexture);
        }
    }
}
