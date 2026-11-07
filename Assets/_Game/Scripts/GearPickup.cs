using System.Collections;
using UnityEngine;

namespace SeedGame
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class GearPickup : MonoBehaviour
    {
        bool collected;
        InteractableHighlight highlight;

        void Awake()
        {
            var gear = Resources.Load<Sprite>("UI/Gear");
            var sprite = GetComponent<SpriteRenderer>();
            if (gear && sprite) { sprite.sprite = gear; sprite.color = Color.white; }
            if (sprite)
            {
                highlight = gameObject.AddComponent<InteractableHighlight>();
                highlight.Initialize(sprite, new Color(1f, .78f, .24f));
            }
        }
        void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<SeedPlayer>();
            if (!player || collected) return;
            collected = true;
            if (highlight) highlight.Hide();
            GetComponent<Collider2D>().enabled = false;
            StartCoroutine(CollectSequence(player));
        }

        IEnumerator CollectSequence(SeedPlayer player)
        {
            var camera = Camera.main ? Camera.main.GetComponent<SeedCamera>() : null;
            var body = player.GetComponent<Rigidbody2D>();
            var oldConstraints = body.constraints;
            var oldControls = player.controlsEnabled;
            body.linearVelocity = Vector2.zero;
            body.constraints = RigidbodyConstraints2D.FreezeAll;
            player.controlsEnabled = false;
            Vector3 start = transform.position;
            Vector3 startScale = transform.localScale;
            float elapsed = 0f;
            bool granted = false;
            while (elapsed < 5f)
            {
                elapsed += Time.deltaTime;
                float focus = elapsed < .65f ? Mathf.SmoothStep(0f, 1f, elapsed / .65f)
                    : elapsed > 4.3f ? Mathf.SmoothStep(1f, 0f, (elapsed - 4.3f) / .7f) : 1f;
                if (camera) camera.SetGearFocus(focus);
                if (!granted && elapsed >= 1.2f)
                {
                    granted = true;
                    GearState.Collect();
                    player.ApplyGearCount(GearState.Count);
                    player.ShowGearEnergy();
                    GameAudio.PlayGearPickup();
                    Debug.Log("Gear collected: " + GearState.Count + "/4");
                }
                var renderer = GetComponent<SpriteRenderer>();
                if (renderer) renderer.enabled = elapsed < 1.2f;
                if (elapsed < 1.2f)
                {
                    float travel = Mathf.SmoothStep(0f, 1f, elapsed / 1.2f);
                    transform.position = Vector3.Lerp(start, player.transform.position + Vector3.up * .65f, travel);
                    transform.localScale = startScale * (1f - .65f * travel);
                    transform.Rotate(0f, 0f, 180f * Time.deltaTime);
                }
                yield return null;
            }
            if (camera) camera.SetGearFocus(0f);
            body.constraints = oldConstraints;
            body.WakeUp();
            if (!Physics2D.OverlapCircle(player.feet.position, .13f, player.groundMask))
                body.linearVelocity = new Vector2(0f, -.1f);
            player.controlsEnabled = oldControls;
            gameObject.SetActive(false);
        }
    }
}
