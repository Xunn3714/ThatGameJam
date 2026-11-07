using UnityEngine;

namespace SeedGame
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class Level02GateCameraZone : MonoBehaviour
    {
        public SeedCamera cameraController;
        public Level02Gate gate;
        public Vector2 viewMin = new Vector2(55f, -.9f);
        public Vector2 viewMax = new Vector2(86.2f, 16.4f);
        public float padding = .05f;
        public float finishFramingAtX = 58f;
        bool active;
        SeedPlayer player;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (active || !other.TryGetComponent(out SeedPlayer enteringPlayer)) return;
            if (!cameraController && Camera.main)
                cameraController = Camera.main.GetComponent<SeedCamera>();
            if (!cameraController) return;
            active = true;
            player = enteringPlayer;
            // Keep the player visible while the view begins moving toward the gate.
            cameraController.ShowFixedView(new Vector2(42.5f, 3.2f),
                new Vector2(66.5f, 16.6f), padding);
        }

        void Update()
        {
            if (!active || !cameraController) return;
            if (gate && gate.IsFullyOpen)
            {
                cameraController.ClearFixedView();
                enabled = false;
                return;
            }
            if (!player) return;
            float progress = Mathf.SmoothStep(0f, 1f,
                Mathf.InverseLerp(46.5f, finishFramingAtX, player.transform.position.x));
            cameraController.MoveFixedView(
                Vector2.Lerp(new Vector2(42.5f, 3.2f), viewMin, progress),
                Vector2.Lerp(new Vector2(66.5f, 16.6f), viewMax, progress), padding);
        }
    }
}
