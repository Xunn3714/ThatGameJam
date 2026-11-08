using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SeedGame
{
    // The two painted states stay registered to the same canvas. A soft circle
    // reveals the revived state from the player's resonance position.
    public sealed class Level03RebirthSequence : MonoBehaviour
    {
        public SeedPlayer player;
        public float landscapeSeconds = 5.5f;
        public float leavesSeconds = 4.5f;
        public bool HasStarted { get; private set; }
        public bool IsComplete { get; private set; }

        readonly List<Material> landscapeMaterials = new List<Material>();
        readonly List<Material> leafMaterials = new List<Material>();
        int initialPulse;

        void Awake()
        {
            var shader = Resources.Load<Shader>("Shaders/Level03RadialReveal");
            if (!shader)
            {
                Debug.LogError("Level 3 reveal shader is missing.");
                enabled = false;
                return;
            }
            // The player's walking animation swaps sprites. Sharing this custom
            // material with scene art lets that changing texture appear on the
            // grass. The faded art and player keep Unity's normal sprite shader;
            // each revealed static sprite gets its own bound texture.
            foreach (var renderer in FindObjectsByType<SpriteRenderer>())
            {
                if (renderer.name.StartsWith("L03 Reborn "))
                    AddRevealMaterial(renderer, shader, landscapeMaterials);
                else if (renderer.name.StartsWith("L03 Leaf "))
                    AddRevealMaterial(renderer, shader, leafMaterials);
            }
        }

        static void AddRevealMaterial(SpriteRenderer renderer, Shader shader,
                                      List<Material> materials)
        {
            var material = new Material(shader) { name = renderer.name + " reveal" };
            material.SetTexture("_MainTex", renderer.sprite.texture);
            material.SetFloat("_RevealRadius", -1f);
            renderer.sharedMaterial = material;
            materials.Add(material);
        }

        void Start()
        {
            if (!player) player = FindAnyObjectByType<SeedPlayer>();
            if (player) initialPulse = player.PulseSerial;
            GameAudio.SetRebornWind(false);
        }

        void Update()
        {
            if (!HasStarted && player && player.PulseSerial > initialPulse)
                StartCoroutine(Rebirth(player.transform.position));
        }

        IEnumerator Rebirth(Vector3 origin)
        {
            HasStarted = true;
            if (landscapeMaterials.Count == 0 || leafMaterials.Count == 0) yield break;
            SetCenter(landscapeMaterials, origin);
            GameAudio.PlayPlantGrowth(this);
            // At the far corner the circle must reach the entire 4096 x 1714 canvas.
            float radius = 1f + Mathf.Max(
                Vector2.Distance(origin, new Vector2(-10.24f, 4.285f)),
                Vector2.Distance(origin, new Vector2(10.24f, 4.285f)),
                Vector2.Distance(origin, new Vector2(10.24f, -4.285f)));
            yield return Expand(landscapeMaterials, radius, landscapeSeconds);
            SetRadius(landscapeMaterials, 50f);
            GameAudio.SetRebornWind(true);

            // Branches carry the new leaves from the trunk towards their tips.
            SetCenter(leafMaterials, new Vector4(8.5f, 2.7f, 0f, 0f));
            StartCoroutine(PlayLeafGrowthSounds());
            yield return Expand(leafMaterials, 17f, leavesSeconds);
            SetRadius(leafMaterials, 50f);
            IsComplete = true;
        }

        IEnumerator PlayLeafGrowthSounds()
        {
            for (int i = 0; i < 4; i++)
            {
                GameAudio.PlayPlantGrowth(this);
                yield return new WaitForSeconds(leavesSeconds / 4f);
            }
        }

        static void SetCenter(List<Material> materials, Vector4 center)
        {
            foreach (var material in materials) material.SetVector("_RevealCenter", center);
        }

        static void SetRadius(List<Material> materials, float radius)
        {
            foreach (var material in materials) material.SetFloat("_RevealRadius", radius);
        }

        static IEnumerator Expand(List<Material> materials, float finalRadius, float seconds)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / seconds));
                SetRadius(materials, Mathf.Lerp(-.2f, finalRadius, t));
                yield return null;
            }
        }

        void OnDestroy()
        {
            foreach (var material in landscapeMaterials) if (material) Destroy(material);
            foreach (var material in leafMaterials) if (material) Destroy(material);
            GameAudio.SetRebornWind(false);
        }
    }
}
