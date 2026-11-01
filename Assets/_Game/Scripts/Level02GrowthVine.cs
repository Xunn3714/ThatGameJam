using System.Collections;
using UnityEngine;

namespace SeedGame
{
    public sealed class Level02GrowthVine : Level02ResonanceTarget
    {
        public GameObject[] platforms;
        public SpriteRenderer bud;
        bool growing;

        public override void OnResonance(SeedPlayer player, int pulseSerial)
        {
            if (growing) return;
            growing = true;
            if (bud) bud.color = new Color(.54f, .9f, .61f);
            StartCoroutine(Grow());
        }

        IEnumerator Grow()
        {
            foreach (var platform in platforms)
            {
                if (!platform) continue;
                var finalScale = platform.transform.localScale;
                var collider = platform.GetComponent<Collider2D>();
                platform.transform.localScale = new Vector3(.03f, .03f, 1f);
                platform.SetActive(true);
                if (collider) collider.enabled = false;
                float t = 0f;
                while (t < .4f)
                {
                    t += Time.deltaTime;
                    platform.transform.localScale = Vector3.Lerp(new Vector3(.03f,.03f,1f),
                        finalScale, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / .4f)));
                    yield return null;
                }
                platform.transform.localScale = finalScale;
                if (collider) collider.enabled = true;
                yield return new WaitForSeconds(.1f);
            }
        }
    }
}
