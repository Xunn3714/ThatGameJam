using UnityEngine;

namespace SeedGame
{
    public sealed class Level02Mushroom : Level02ResonanceTarget
    {
        public SpriteRenderer visual;
        public float bounceSpeed = 13f;
        bool awake;
        float lastBounce;

        public override void OnResonance(SeedPlayer player, int pulseSerial)
        {
            awake = true;
            if (visual) visual.color = new Color(.59f, .93f, .75f);
            TryBounce(player);
        }

        void OnCollisionStay2D(Collision2D collision)
        {
            if (!awake) return;
            TryBounce(collision.gameObject.GetComponent<SeedPlayer>());
        }

        void TryBounce(SeedPlayer player)
        {
            if (!player || Time.time - lastBounce < .5f) return;
            if (Mathf.Abs(player.transform.position.x - transform.position.x) > 1.15f) return;
            if (player.transform.position.y < transform.position.y) return;
            lastBounce = Time.time;
            var body = player.GetComponent<Rigidbody2D>();
            body.linearVelocity = new Vector2(body.linearVelocity.x, bounceSpeed);
            if (visual) visual.transform.localScale = new Vector3(1.8f, .42f, 1f);
        }

        void Update()
        {
            if (visual) visual.transform.localScale = Vector3.Lerp(visual.transform.localScale,
                new Vector3(1.8f, .65f, 1f), Time.deltaTime * 8f);
        }
    }
}
