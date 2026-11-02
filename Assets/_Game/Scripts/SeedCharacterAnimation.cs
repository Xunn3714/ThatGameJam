using UnityEngine;

namespace SeedGame
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class SeedCharacterAnimation : MonoBehaviour
    {
        public SeedPlayer player;
        public Sprite[] frames;
        public float framesPerSecond = 7f;
        SpriteRenderer art;

        void Awake() => art = GetComponent<SpriteRenderer>();

        void Update()
        {
            if (!player || frames == null || frames.Length == 0 || !frames[0]) return;
            float speed = player.GetComponent<Rigidbody2D>().linearVelocity.x;
            if (Mathf.Abs(speed) > .08f)
            {
                art.sprite = frames[Mathf.FloorToInt(Time.time * framesPerSecond) % frames.Length];
                art.flipX = speed < 0f;
            }
            else art.sprite = frames[0];
        }
    }
}
