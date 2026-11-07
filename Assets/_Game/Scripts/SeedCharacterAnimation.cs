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
        Rigidbody2D body;
        float walkTime;
        int lastStepCycle = -1;

        void Awake()
        {
            art = GetComponent<SpriteRenderer>();
            if (player) body = player.GetComponent<Rigidbody2D>();
        }

        void Update()
        {
            if (!player || frames == null || frames.Length == 0 || !frames[0]) return;
            if (!body) body = player.GetComponent<Rigidbody2D>();
            float speed = body ? body.linearVelocity.x : 0f;
            if (player.controlsEnabled && Mathf.Abs(speed) > .08f)
            {
                int frame = Mathf.FloorToInt(walkTime * framesPerSecond);
                int cycle = frame / frames.Length;
                art.sprite = frames[frame % frames.Length];
                art.flipX = speed < 0f;
                if (cycle != lastStepCycle && player.IsOnGround)
                    GameAudio.PlayFootstep();
                lastStepCycle = cycle;
                walkTime += Time.deltaTime;
            }
            else
            {
                art.sprite = frames[0];
                walkTime = 0f;
                lastStepCycle = -1;
            }
        }
    }
}
