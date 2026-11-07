using System.Collections.Generic;
using UnityEngine;

namespace SeedGame
{
    public sealed class Level02Algae : Level02ResonanceTarget
    {
        public SpriteRenderer darkness;
        public SpriteRenderer[] glowMarks;
        public Color unlitTint = new Color(.035f, .09f, .13f);
        public Color litTint = Color.white;
        readonly List<InteractableHighlight> highlights = new List<InteractableHighlight>();
        readonly List<SpriteRenderer> softGlows = new List<SpriteRenderer>();
        readonly List<SpriteRenderer> areaGlows = new List<SpriteRenderer>();
        static Sprite areaGlowSprite;
        bool lit;
        float progress;

        void Awake()
        {
            if (darkness)
            {
                var color = darkness.color;
                color.a = .78f;
                darkness.color = color;
            }
            if (glowMarks == null) return;
            foreach (var mark in glowMarks)
                if (mark) mark.color = unlitTint;
        }

        void Start()
        {
            if (glowMarks == null) return;
            if (!areaGlowSprite) CreateAreaGlowSprite();
            foreach (var mark in glowMarks)
            {
                if (!mark) continue;
                var highlight = mark.gameObject.AddComponent<InteractableHighlight>();
                highlight.Initialize(mark, new Color(.2f, .72f, 1f));
                highlight.ConfigurePulse(1.05f, .42f, 5);
                highlights.Add(highlight);

                var glowObject = new GameObject("Blue algae breathing glow");
                glowObject.transform.SetParent(mark.transform, false);
                var glow = glowObject.AddComponent<SpriteRenderer>();
                glow.sprite = mark.sprite;
                glow.sortingLayerID = mark.sortingLayerID;
                glow.sortingOrder = mark.sortingOrder + 4;
                glow.color = new Color(.55f, .9f, 1f, .12f);
                softGlows.Add(glow);

                var areaObject = new GameObject("Blue algae area glow");
                areaObject.transform.SetParent(mark.transform, false);
                areaObject.transform.localScale = new Vector3(4.2f, 3.1f, 1f);
                var area = areaObject.AddComponent<SpriteRenderer>();
                area.sprite = areaGlowSprite;
                area.sortingLayerID = mark.sortingLayerID;
                area.sortingOrder = mark.sortingOrder + 2;
                areaGlows.Add(area);
            }
        }

        public override void OnResonance(SeedPlayer player, int pulseSerial)
        {
            lit = true;
            foreach (var highlight in highlights)
                if (highlight) highlight.Hide();
        }

        void Update()
        {
            for (int i = 0; i < softGlows.Count; i++)
            {
                var glow = softGlows[i];
                if (!glow) continue;
                float breath = .5f + .5f * Mathf.Sin(Time.time * 1.05f + i * .7f);
                float alpha = lit ? Mathf.Lerp(.18f, .28f, progress)
                    : Mathf.Lerp(.08f, .22f, breath);
                glow.color = new Color(.55f, .9f, 1f, alpha);
                if (i < areaGlows.Count && areaGlows[i])
                {
                    float areaAlpha = lit ? Mathf.Lerp(.21f, .31f, progress)
                        : Mathf.Lerp(.12f, .27f, breath);
                    areaGlows[i].color = new Color(.18f, .68f, 1f, areaAlpha);
                }
            }
            if (!lit) return;
            progress = Mathf.MoveTowards(progress, 1f, Time.deltaTime / 1.2f);
            if (darkness)
            {
                var color = darkness.color;
                color.a = Mathf.Lerp(.78f, .06f, progress);
                darkness.color = color;
            }
            if (glowMarks != null)
                foreach (var mark in glowMarks)
                    if (mark) mark.color = Color.Lerp(unlitTint, litTint, progress);
        }

        static void CreateAreaGlowSprite()
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Blue algae area glow texture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };
            var pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = (x + .5f - size * .5f) / (size * .5f);
                    float dy = (y + .5f - size * .5f) / (size * .5f);
                    float radius = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Pow(Mathf.Clamp01(1f - radius), 1.8f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            areaGlowSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size),
                new Vector2(.5f, .5f), size);
            areaGlowSprite.name = "Blue algae area glow";
            areaGlowSprite.hideFlags = HideFlags.HideAndDontSave;
        }
    }
}
