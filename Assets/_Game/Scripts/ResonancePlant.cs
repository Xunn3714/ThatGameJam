using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SeedGame
{
    public sealed class ResonancePlant : MonoBehaviour
    {
        public GameObject dormantVisual;
        public GameObject grownRoot;
        public bool isActive;
        public bool generatePlaceholderSteps;
        public bool oneWayPlatforms;
        public Vector4[] placeholderSteps;
        public float[] stepColliderWidths;
        public Sprite[] plantFrames;
        public Sprite[] stepFramesA;
        public Sprite[] stepFramesB;
        public Sprite[] stepFramesC;
        public SpriteRenderer[] stepArtRenderers;
        public float stepArtGroundY;
        public Sprite[] bridgeFrames;
        public SpriteRenderer bridgeRenderer;
        public Collider2D bridgeCollider;
        public bool bridgeGrowsFromRight;
        public float bridgeVisualBaseY = -.08f;
        public bool keepDormantAsRoot;
        public float growthFrameSeconds = .14f;
        readonly List<InteractableHighlight> highlights = new List<InteractableHighlight>();
        static Sprite placeholderSprite;
        static Texture2D placeholderTexture;

        void Start()
        {
            if (isActive) return;
            var targets = new List<SpriteRenderer>();
            if (dormantVisual)
            {
                var renderer = dormantVisual.GetComponent<SpriteRenderer>();
                if (renderer) targets.Add(renderer);
            }
            if (stepArtRenderers != null)
                foreach (var renderer in stepArtRenderers)
                    if (renderer && !targets.Contains(renderer)) targets.Add(renderer);
            foreach (var renderer in targets)
            {
                var highlight = renderer.gameObject.AddComponent<InteractableHighlight>();
                highlight.Initialize(renderer, new Color(.74f, 1f, .58f));
                highlights.Add(highlight);
            }
        }

        public void Activate()
        {
            if (isActive) return;
            isActive = true;
            foreach (var highlight in highlights)
                if (highlight) highlight.Hide();
            if (oneWayPlatforms && grownRoot)
                foreach (var collider in grownRoot.GetComponentsInChildren<Collider2D>(true))
                    MakeOneWay(collider);
            if (generatePlaceholderSteps && placeholderSteps != null && placeholderSteps.Length > 0)
            {
                if (stepFramesA != null && stepFramesA.Length > 0)
                    StartCoroutine(GrowLeafSteps());
                else
                {
                    if (dormantVisual) dormantVisual.SetActive(false);
                    GeneratePlaceholderSteps();
                }
            }
            else if (bridgeRenderer && bridgeFrames != null && bridgeFrames.Length > 0)
                StartCoroutine(GrowBridge());
            else if (grownRoot)
            {
                if (dormantVisual) dormantVisual.SetActive(false);
                grownRoot.SetActive(true);
            }
            Debug.Log("Resonance activated: " + name);
        }

        IEnumerator GrowLeafSteps()
        {
            bool hasPlacedArt = stepArtRenderers != null && stepArtRenderers.Length >= placeholderSteps.Length;
            var bud = !hasPlacedArt && dormantVisual ? dormantVisual.GetComponent<SpriteRenderer>() : null;
            if (bud && plantFrames != null)
                foreach (var frame in plantFrames)
                {
                    if (frame) bud.sprite = frame;
                    yield return new WaitForSeconds(growthFrameSeconds);
                }

            for (int i = 0; i < placeholderSteps.Length; i++)
            {
                Vector4 step = placeholderSteps[i];
                Sprite[] frames = i == 0 ? stepFramesA : i == 1 ? stepFramesB : stepFramesC;
                if (frames == null || frames.Length == 0) frames = stepFramesA;
                Sprite final = frames[frames.Length - 1];
                if (!final) continue;

                var platform = new GameObject("Resonance leaf step " + (i + 1));
                platform.transform.SetParent(transform);
                platform.transform.position = new Vector3(step.x, step.y, 0);
                platform.layer = 6;
                SpriteRenderer renderer = hasPlacedArt ? stepArtRenderers[i] : null;
                if (!renderer)
                {
                    var newArt = new GameObject("Leaf artwork");
                    renderer = newArt.AddComponent<SpriteRenderer>();
                    newArt.transform.position = platform.transform.position;
                }
                float startingY = renderer.transform.position.y - step.y;
                renderer.transform.SetParent(platform.transform, true);
                renderer.sortingOrder = 6;
                float scale = step.z * 1.12f / final.bounds.size.x;
                renderer.transform.localScale = new Vector3(scale, scale, 1f);
                for (int frameIndex = 0; frameIndex < frames.Length; frameIndex++)
                {
                    var frame = frames[frameIndex];
                    if (!frame) continue;
                    renderer.sprite = frame;
                    float progress = (frameIndex + 1f) / frames.Length;
                    float grownY = stepArtGroundY - step.y + frame.bounds.size.y * scale * .5f;
                    renderer.transform.localPosition = new Vector3(0, Mathf.Lerp(startingY, grownY, progress), 0);
                    yield return new WaitForSeconds(growthFrameSeconds);
                }
                var collider = platform.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(ColliderWidth(i, step.z), step.w);
                if (oneWayPlatforms) MakeOneWay(collider);
            }
        }

        IEnumerator GrowBridge()
        {
            if (grownRoot) grownRoot.SetActive(true);
            var collider = bridgeCollider ? bridgeCollider : bridgeRenderer.GetComponent<Collider2D>();
            if (collider) collider.enabled = false;
            foreach (var frame in bridgeFrames)
            {
                if (!frame) continue;
                bridgeRenderer.sprite = frame;
                float width = frame.bounds.size.x * Mathf.Abs(bridgeRenderer.transform.lossyScale.x);
                float height = frame.bounds.size.y * Mathf.Abs(bridgeRenderer.transform.lossyScale.y);
                bridgeRenderer.transform.position = new Vector3(
                    transform.position.x + (bridgeGrowsFromRight ? -width : width) * .5f,
                    height * .5f + bridgeVisualBaseY, 0f);
                yield return new WaitForSeconds(growthFrameSeconds);
            }
            if (collider) collider.enabled = true;
        }

        void GeneratePlaceholderSteps()
        {
            if (!placeholderSprite)
            {
                placeholderTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                placeholderTexture.SetPixel(0, 0, Color.white);
                placeholderTexture.Apply();
                placeholderSprite = Sprite.Create(placeholderTexture,
                    new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1);
            }
            for (int i = 0; i < placeholderSteps.Length; i++)
            {
                var step = placeholderSteps[i];
                var go = new GameObject("Resonance leaf step");
                go.transform.SetParent(transform);
                go.transform.position = new Vector3(step.x, step.y, 0);
                go.transform.localScale = new Vector3(step.z, step.w, 1);
                go.layer = 6;
                var sprite = go.AddComponent<SpriteRenderer>();
                sprite.sprite = placeholderSprite;
                sprite.color = new Color(.69f, .9f, .48f);
                sprite.sortingOrder = 6;
                var collider = go.AddComponent<BoxCollider2D>();
                collider.size = new Vector2(ColliderWidth(i, step.z) / step.z, 1);
                if (oneWayPlatforms) MakeOneWay(collider);
            }
        }
        float ColliderWidth(int index, float artworkWidth) =>
            stepColliderWidths != null && index < stepColliderWidths.Length && stepColliderWidths[index] > 0
                ? stepColliderWidths[index] : artworkWidth;
        static void MakeOneWay(Collider2D collider)
        {
            var effector = collider.GetComponent<PlatformEffector2D>();
            if (!effector) effector = collider.gameObject.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.useOneWayGrouping = true;
            effector.surfaceArc = 175f;
            effector.useSideFriction = false;
            effector.useSideBounce = false;
            collider.usedByEffector = true;
        }
    }
}
