using System.Collections;
using UnityEngine;

namespace SeedGame
{
    public sealed class Level02Mushroom : Level02ResonanceTarget
    {
        public SpriteRenderer visual;
        public Sprite[] growthFrames;
        public SpriteRenderer[] placedStages;
        public float matureWidth = 1.8f;
        public float bounceSpeed = 13f;
        public bool canRescueFromBelow;
        bool awake;
        float lastBounce;
        Vector3 restScale;
        bool growing;
        InteractableHighlight highlight;
        BoxCollider2D bounceCollider;

        void Awake()
        {
            Level02PlacedStages.Show(placedStages, 0);
            bounceCollider = GetComponent<BoxCollider2D>();
            if (bounceCollider) bounceCollider.enabled = false;
        }

        void Start()
        {
            if (visual) restScale = visual.transform.localScale;
            highlight = Level02PlacedStages.HighlightFirst(placedStages);
        }

        public override void OnResonance(SeedPlayer player, int pulseSerial)
        {
            if (!awake && ((placedStages != null && placedStages.Length > 1) ||
                (growthFrames != null && growthFrames.Length > 0)))
            {
                GameAudio.PlayPlantGrowth();
                StartCoroutine(Grow(canRescueFromBelow ? player : null));
            }
            awake = true;
            if (highlight) highlight.Hide();
            if (visual)
                visual.color = Color.white;
            TryBounce(player, canRescueFromBelow);
        }

        IEnumerator Grow(SeedPlayer rescuePlayer = null)
        {
            growing = true;
            if (placedStages != null && placedStages.Length > 1)
            {
                for (int i = 1; i < placedStages.Length; i++)
                {
                    yield return new WaitForSeconds(.2f);
                    Level02PlacedStages.Show(placedStages, i);
                }
                growing = false;
                if (bounceCollider) bounceCollider.enabled = true;
                if (rescuePlayer) TryBounce(rescuePlayer, true);
                yield break;
            }
            for (int i = 0; i < growthFrames.Length; i++)
            {
                if (visual && growthFrames[i])
                {
                    visual.sprite = growthFrames[i];
                    float width = Mathf.Lerp(.65f, matureWidth,
                        (i + 1f) / growthFrames.Length);
                    restScale = Vector3.one * (width / visual.sprite.bounds.size.x);
                    visual.transform.localScale = restScale;
                }
                yield return new WaitForSeconds(.16f);
            }
            growing = false;
            if (bounceCollider) bounceCollider.enabled = true;
            if (rescuePlayer) TryBounce(rescuePlayer, true);
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            if (!awake || growing) return;
            TryBounce(collision.gameObject.GetComponent<SeedPlayer>());
        }

        void TryBounce(SeedPlayer player, bool allowFromBelow = false)
        {
            if (!player || growing || Time.time - lastBounce < .5f) return;
            if (Mathf.Abs(player.transform.position.x - transform.position.x) > 1.15f) return;
            if (!allowFromBelow && player.transform.position.y < transform.position.y) return;
            lastBounce = Time.time;
            var body = player.GetComponent<Rigidbody2D>();
            body.linearVelocity = new Vector2(body.linearVelocity.x, bounceSpeed);
            if (visual) visual.transform.localScale = new Vector3(restScale.x, restScale.y * .65f, restScale.z);
        }

        void Update()
        {
            if (visual && !growing) visual.transform.localScale = Vector3.Lerp(visual.transform.localScale,
                restScale, Time.deltaTime * 8f);
        }
    }
}
