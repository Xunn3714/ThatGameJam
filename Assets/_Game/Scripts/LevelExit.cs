using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeedGame
{
    public sealed class LevelExit : MonoBehaviour
    {
        public string destinationScene;
        public int requiredGears;
        public Level02Gate requiredGate;
        // The trigger starts at the doorway. Wait until the player's centre has
        // passed into it so the front half of the arch can hide the transition.
        public float entryDepth = .6f;
        bool entered;

        void OnTriggerStay2D(Collider2D other)
        {
            if (entered) return;
            var player = other.GetComponent<SeedPlayer>();
            if (!player || GearState.Count < requiredGears) return;
            if (requiredGate && !requiredGate.IsFullyOpen) return;
            if (player.transform.position.x < transform.position.x + entryDepth) return;
            entered = true;
            if (!string.IsNullOrEmpty(destinationScene))
                LevelTransitionFade.Load(destinationScene, player);
            else LevelFlow.Instance?.Finish();
        }
    }

    // Persists just long enough to cover the scene load, then reveals the new level.
    sealed class LevelTransitionFade : MonoBehaviour
    {
        float blackAlpha;

        public static void Load(string destination, SeedPlayer player)
        {
            var transition = new GameObject("Black doorway transition")
                .AddComponent<LevelTransitionFade>();
            DontDestroyOnLoad(transition.gameObject);
            transition.StartCoroutine(transition.ChangeScene(destination, player));
        }

        IEnumerator ChangeScene(string destination, SeedPlayer player)
        {
            if (player)
            {
                player.controlsEnabled = false;
                var body = player.GetComponent<Rigidbody2D>();
                if (body)
                {
                    body.linearVelocity = Vector2.zero;
                    body.simulated = false;
                }
            }
            yield return Fade(0f, 1f, .5f);
            yield return new WaitForSecondsRealtime(.18f);
            SceneManager.LoadScene(destination);
            yield return null;
            yield return Fade(1f, 0f, .7f);
            Destroy(gameObject);
        }

        IEnumerator Fade(float from, float to, float seconds)
        {
            float elapsed = 0f;
            while (elapsed < seconds)
            {
                elapsed += Time.unscaledDeltaTime;
                blackAlpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / seconds));
                yield return null;
            }
            blackAlpha = to;
        }

        void OnGUI()
        {
            if (blackAlpha <= 0f) return;
            GUI.depth = -950;
            GUI.color = new Color(0f, 0f, 0f, blackAlpha);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
