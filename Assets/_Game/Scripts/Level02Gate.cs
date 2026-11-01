using System.Collections;
using UnityEngine;

namespace SeedGame
{
    public sealed class Level02Gate : MonoBehaviour
    {
        public SeedPlayer player;
        public Level02GateFlower[] flowers;
        public Collider2D gateCollider;
        public SpriteRenderer gateVisual;
        bool open;
        public bool IsOpen => open;

        void Update()
        {
            if (open || !player || flowers == null) return;
            if (Time.time < player.PulseEndsAt) return;
            foreach (var flower in flowers)
                if (flower) flower.ResetForNextPulse();
        }

        public void Check(int serial)
        {
            if (open || flowers == null || flowers.Length != 4) return;
            foreach (var flower in flowers)
                if (!flower || flower.ActivePulse != serial) return;
            open = true;
            StartCoroutine(Open());
        }

        IEnumerator Open()
        {
            if (gateCollider) gateCollider.enabled = false;
            var start = gateVisual ? gateVisual.transform.position : transform.position;
            for (float t = 0f; t < 1.2f; t += Time.deltaTime)
            {
                if (gateVisual)
                    gateVisual.transform.position = start + Vector3.up * (3.3f * Mathf.SmoothStep(0f,1f,t/1.2f));
                yield return null;
            }
            if (gateVisual) gateVisual.transform.position = start + Vector3.up * 3.3f;
            Debug.Log("Level 2 gate opened by four plants in one pulse.");
        }
    }
}
