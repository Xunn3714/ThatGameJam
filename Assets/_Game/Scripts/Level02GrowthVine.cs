using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SeedGame
{
    public sealed class Level02GrowthVine : Level02ResonanceTarget
    {
        public GameObject[] platforms;
        public SpriteRenderer bud;
        public Level02GrowthStep[] artSteps;
        public SpriteRenderer[] placedStages;
        InteractableHighlight highlight;
        EdgeCollider2D grownCollider;
        bool growing;

        void Awake()
        {
            Level02PlacedStages.Show(placedStages, 0);
            if (placedStages == null || placedStages.Length < 3 || !placedStages[2]) return;
            if (platforms != null)
                foreach (var oldStep in platforms)
                    if (oldStep) oldStep.SetActive(false);
            var mature = placedStages[2].gameObject;
            mature.layer = 6; // the player's ground mask
            // A sprite-shaped polygon has gaps between leaves and separate branches.
            // Use one continuous upper edge for the whole finished plant instead.
            foreach (var shape in mature.GetComponents<PolygonCollider2D>())
                shape.enabled = false;
            grownCollider = mature.GetComponent<EdgeCollider2D>();
            if (!grownCollider) grownCollider = mature.AddComponent<EdgeCollider2D>();
            grownCollider.points = BuildContinuousTop(placedStages[2]);
            MakeOneWay(grownCollider);
            grownCollider.enabled = false;
        }

        public static Vector2[] BuildContinuousTop(SpriteRenderer renderer)
        {
            const int samples = 40;
            var sprite = renderer.sprite;
            if (!sprite) return new[] { new Vector2(-.5f, 0f), new Vector2(.5f, 0f) };
            var shape = new List<Vector2>();
            var paths = new List<List<Vector2>>();
            float left = float.PositiveInfinity;
            float right = float.NegativeInfinity;
            for (int i = 0; i < sprite.GetPhysicsShapeCount(); i++)
            {
                shape.Clear();
                sprite.GetPhysicsShape(i, shape);
                if (shape.Count < 2) continue;
                var path = new List<Vector2>(shape);
                paths.Add(path);
                foreach (var point in path)
                {
                    left = Mathf.Min(left, point.x);
                    right = Mathf.Max(right, point.x);
                }
            }
            if (paths.Count == 0 || right - left < .1f)
            {
                left = sprite.bounds.min.x;
                right = sprite.bounds.max.x;
                float top = sprite.bounds.max.y - sprite.bounds.size.y * .18f;
                return new[] { new Vector2(left, top), new Vector2(right, top) };
            }

            var heights = new float[samples + 1];
            for (int i = 0; i <= samples; i++)
            {
                float x = Mathf.Lerp(left + .01f, right - .01f, i / (float)samples);
                heights[i] = float.NegativeInfinity;
                foreach (var path in paths)
                    for (int j = 0; j < path.Count; j++)
                    {
                        Vector2 a = path[j];
                        Vector2 b = path[(j + 1) % path.Count];
                        if (x < Mathf.Min(a.x, b.x) || x > Mathf.Max(a.x, b.x)) continue;
                        float t = Mathf.Abs(b.x - a.x) < .0001f ? 0f : (x - a.x) / (b.x - a.x);
                        heights[i] = Mathf.Max(heights[i], Mathf.Lerp(a.y, b.y, t));
                    }
            }
            // Bridge any transparent columns, then smooth small leaf notches.
            for (int i = 1; i <= samples; i++)
                if (float.IsNegativeInfinity(heights[i])) heights[i] = heights[i - 1];
            for (int i = samples - 1; i >= 0; i--)
                if (float.IsNegativeInfinity(heights[i])) heights[i] = heights[i + 1];
            float fallbackTop = sprite.bounds.max.y - sprite.bounds.size.y * .18f;
            float maxStep = (right - left) / samples * 2.5f;
            for (int i = 0; i <= samples; i++)
            {
                if (float.IsNegativeInfinity(heights[i])) heights[i] = fallbackTop;
                if (i > 0) heights[i] = Mathf.Min(heights[i], heights[i - 1] + maxStep);
            }
            for (int i = samples - 1; i >= 0; i--)
                heights[i] = Mathf.Min(heights[i], heights[i + 1] + maxStep);
            var points = new Vector2[samples + 1];
            for (int i = 0; i <= samples; i++)
            {
                float middle = heights[i];
                float before = heights[Mathf.Max(0, i - 1)];
                float after = heights[Mathf.Min(samples, i + 1)];
                float y = (before + middle * 2f + after) * .25f - .035f;
                float x = Mathf.Lerp(left + .01f, right - .01f, i / (float)samples);
                points[i] = new Vector2(renderer.flipX ? -x : x, y);
            }
            if (renderer.flipX) System.Array.Reverse(points);
            return points;
        }

        void Start() => highlight = Level02PlacedStages.HighlightFirst(placedStages);

        public override void OnResonance(SeedPlayer player, int pulseSerial)
        {
            if (growing) return;
            growing = true;
            GameAudio.PlayPlantGrowth();
            if (highlight) highlight.Hide();
            if (bud) bud.color = new Color(.54f, .9f, .61f);
            if (placedStages != null && placedStages.Length > 1)
                StartCoroutine(GrowPlaced());
            else StartCoroutine(Grow());
        }

        IEnumerator GrowPlaced()
        {
            for (int i = 1; i < placedStages.Length; i++)
            {
                yield return new WaitForSeconds(.28f);
                Level02PlacedStages.Show(placedStages, i);
            }
            if (grownCollider) grownCollider.enabled = true;
        }

        IEnumerator Grow()
        {
            for (int i = 0; platforms != null && i < platforms.Length; i++)
            {
                var platform = platforms[i];
                if (!platform) continue;
                var art = artSteps != null && i < artSteps.Length ? artSteps[i] : null;
                var collider = platform.GetComponent<Collider2D>();
                platform.SetActive(true);
                if (collider)
                {
                    MakeOneWay(collider);
                    collider.enabled = false;
                }
                float t = 0f;
                while (t < .4f)
                {
                    t += Time.deltaTime;
                    if (art) art.Show(Mathf.Clamp01(t / .4f));
                    yield return null;
                }
                if (art) art.Show(1f);
                if (collider) collider.enabled = true;
                yield return new WaitForSeconds(.1f);
            }
        }

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
