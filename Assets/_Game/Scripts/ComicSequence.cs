using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace SeedGame
{
    // Screen-space comic panels use the artist's 3000 x 1688 canvas. The
    // ending panels are irregular cutouts and must retain their pixel offsets.
    public sealed class ComicSequence : MonoBehaviour
    {
        const float CanvasWidth = 3000f;
        const float CanvasHeight = 1688f;
        const float PanelInterval = 2.5f;
        const float EndingPanelInterval = 4f;
        const float PanelFade = .35f;
        const string EndingThanks = "Thank you for playing!";
        const string EndingHint = "Press ESC to exit, press R to restart the game.";

        static ComicSequence instance;
        readonly Texture2D[] intro = new Texture2D[4];
        readonly Texture2D[] ending = new Texture2D[4];
        Texture2D introBackground;
        bool introPlayed;
        bool showingIntro;
        bool showingEnding;
        bool showIntroPanels;
        bool showEndingPanels;
        bool endingComplete;
        int visiblePanels;
        float newestPanelAt;
        float blackAlpha;
        float whiteAlpha;
        SeedPlayer player;
        Rigidbody2D playerBody;
        SeedCamera openingCamera;
        float normalGravity;
        Font endingHintFont;
        GUIStyle endingHintStyle;
        GUIStyle endingHintShadowStyle;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Bootstrap()
        {
            if (!instance)
                new GameObject("Opening and ending comics").AddComponent<ComicSequence>();
        }

        void Awake()
        {
            if (instance && instance != this) { Destroy(gameObject); return; }
            instance = this;
            DontDestroyOnLoad(gameObject);
            introBackground = Resources.Load<Texture2D>("Comics/IntroBackground");
            for (int i = 0; i < 3; i++)
                intro[i] = Resources.Load<Texture2D>("Comics/Intro" + (i + 1));
            for (int i = 0; i < 4; i++)
                ending[i] = Resources.Load<Texture2D>("Comics/Ending" + (i + 1));
            endingHintFont = Resources.Load<Font>("Fonts/Chewy-Regular");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            if (instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StopAllCoroutines();
            GameAudio.SetOpeningComicWind(false);
            showingIntro = showingEnding = showIntroPanels = showEndingPanels = false;
            endingComplete = false;
            blackAlpha = whiteAlpha = 0f;
            visiblePanels = 0;
            player = FindAnyObjectByType<SeedPlayer>();
            playerBody = player ? player.GetComponent<Rigidbody2D>() : null;
            openingCamera = null;

            if (scene.name == "Level01_Alpha" && !introPlayed)
            {
                if (!introBackground || !intro[0] || !intro[1] || !intro[2])
                {
                    Debug.LogError("Opening comic textures are missing from Resources/Comics.");
                    return;
                }
                introPlayed = true;
                showingIntro = showIntroPanels = true;
                visiblePanels = 1;
                newestPanelAt = Time.time;
                if (player) player.controlsEnabled = false;
                if (playerBody)
                {
                    normalGravity = playerBody.gravityScale;
                    playerBody.linearVelocity = Vector2.zero;
                    playerBody.simulated = false;
                }
                // Keep the ground framed while the seed waits above the top edge.
                // Moving the spawn alone would make the follow camera chase it.
                if (Camera.main)
                {
                    openingCamera = Camera.main.GetComponent<SeedCamera>();
                    if (openingCamera) openingCamera.enabled = false;
                    Camera.main.orthographicSize = 4f;
                    float halfWidth = 4f * Camera.main.aspect;
                    float minX = openingCamera ? openingCamera.minX : 0f;
                    float maxX = openingCamera ? openingCamera.maxX : 47f;
                    float minY = openingCamera ? openingCamera.minY : -1.6f;
                    float x = player ? player.transform.position.x : 2f;
                    float cameraX = Mathf.Clamp(x, minX + halfWidth, maxX - halfWidth);
                    Camera.main.transform.position = new Vector3(cameraX, minY + 4f, -10f);
                }
                if (player)
                    player.transform.position = new Vector3(
                        player.transform.position.x, 8.2f, player.transform.position.z);
                StartCoroutine(PlayIntro());
            }
            else if (scene.name == "Level03_Alpha")
            {
                StartCoroutine(WaitForEnding());
            }
        }

        void Update()
        {
            if (!endingComplete || Keyboard.current == null || !Keyboard.current.rKey.wasPressedThisFrame)
                return;

            endingComplete = false;
            introPlayed = false;
            SceneManager.LoadScene("Level01_Alpha");
        }

        IEnumerator PlayIntro()
        {
            GameAudio.SetOpeningComicWind(true);
            yield return new WaitForSeconds(PanelInterval);
            visiblePanels = 2;
            newestPanelAt = Time.time;
            yield return new WaitForSeconds(PanelInterval);
            visiblePanels = 3;
            newestPanelAt = Time.time;
            yield return new WaitForSeconds(PanelInterval);

            GameAudio.SetOpeningComicWind(false);
            yield return FadeBlack(0f, 1f, .55f);
            yield return new WaitForSeconds(1.5f);
            showIntroPanels = false;
            // The first level begins with the seed falling into the playable map.
            if (playerBody)
            {
                playerBody.gravityScale = Mathf.Min(normalGravity, 1.2f);
                playerBody.simulated = true;
            }
            yield return FadeBlack(1f, 0f, .9f);
            showingIntro = false;
            float deadline = Time.time + 3f;
            while (player && !player.IsOnGround && Time.time < deadline)
                yield return null;
            if (playerBody) playerBody.gravityScale = normalGravity;
            if (openingCamera)
            {
                openingCamera.playIntro = false;
                openingCamera.enabled = true;
            }
            if (player) player.controlsEnabled = true;
        }

        IEnumerator WaitForEnding()
        {
            Level03RebirthSequence rebirth = null;
            while (!rebirth)
            {
                rebirth = FindAnyObjectByType<Level03RebirthSequence>();
                yield return null;
            }
            while (!rebirth.IsComplete) yield return null;
            if (!ending[0] || !ending[1] || !ending[2] || !ending[3])
            {
                Debug.LogError("Ending comic textures are missing from Resources/Comics.");
                yield break;
            }
            if (player) player.controlsEnabled = false;
            if (playerBody)
            {
                playerBody.linearVelocity = Vector2.zero;
                playerBody.simulated = false;
            }
            showingEnding = true;
            yield return FadeWhite(0f, 1f, 1.2f);
            showEndingPanels = true;
            visiblePanels = 1;
            newestPanelAt = Time.time;
            yield return FadeWhite(1f, 0f, .8f);
            yield return new WaitForSeconds(EndingPanelInterval - .8f);
            for (int count = 2; count <= 4; count++)
            {
                visiblePanels = count;
                newestPanelAt = Time.time;
                yield return new WaitForSeconds(EndingPanelInterval);
            }
            // Keep the completed four-panel image on screen.
            endingComplete = true;
        }

        IEnumerator FadeBlack(float from, float to, float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                blackAlpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / seconds));
                yield return null;
            }
            blackAlpha = to;
        }

        IEnumerator FadeWhite(float from, float to, float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                whiteAlpha = Mathf.Lerp(from, to, Mathf.Clamp01(t / seconds));
                yield return null;
            }
            whiteAlpha = to;
        }

        void OnGUI()
        {
            if (!showingIntro && !showingEnding) return;
            GUI.depth = -1000;
            if (showIntroPanels || showEndingPanels)
            {
                DrawSolid(Color.white);
                float scale = Mathf.Min(Screen.width / CanvasWidth, Screen.height / CanvasHeight);
                float left = (Screen.width - CanvasWidth * scale) * .5f;
                float top = (Screen.height - CanvasHeight * scale) * .5f;
                if (showIntroPanels)
                {
                    DrawPanel(introBackground, 0, 0, 3000, 1688, left, top, scale, 1f);
                    DrawPanel(intro[0], 0, 0, 1386, 842, left, top, scale, PanelOpacity(1));
                    DrawPanel(intro[1], 0, 872, 1401, 816, left, top, scale, PanelOpacity(2));
                    DrawIntroRightPanel(left, top, scale, PanelOpacity(3));
                }
                else
                {
                    DrawPanel(ending[0], 113, 0, 2865, 734, left, top, scale, PanelOpacity(1));
                    DrawRightEdge(ending[0], 2978, 0, 22, 734, left, top, scale, PanelOpacity(1));
                    DrawPanel(ending[1], 1971, 243, 1010, 1445, left, top, scale, PanelOpacity(2));
                    DrawRightEdge(ending[1], 2981, 243, 19, 1445, left, top, scale, PanelOpacity(2));
                    DrawPanel(ending[2], 579, 680, 1751, 1008, left, top, scale, PanelOpacity(3));
                    DrawPanel(ending[3], 0, 0, 1046, 1688, left, top, scale, PanelOpacity(4));
                }
            }
            if (whiteAlpha > 0f) DrawSolid(new Color(1f, 1f, 1f, whiteAlpha));
            if (blackAlpha > 0f) DrawSolid(new Color(0f, 0f, 0f, blackAlpha));
            if (showingEnding && endingComplete) DrawEndingHints();
            GUI.color = Color.white;
        }

        void DrawEndingHints()
        {
            if (endingHintStyle == null)
            {
                endingHintStyle = new GUIStyle(GUI.skin.label)
                {
                    font = endingHintFont,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = false
                };
                endingHintShadowStyle = new GUIStyle(endingHintStyle);
                endingHintStyle.normal.textColor = new Color(.24f, .27f, .51f);
                endingHintShadowStyle.normal.textColor = new Color(.98f, .95f, 1f, .8f);
            }

            int fontSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height * .043f), 18, 46);
            var content = new GUIContent(EndingHint);
            endingHintStyle.fontSize = fontSize;
            while (fontSize > 14 && endingHintStyle.CalcSize(content).x > Screen.width - 32f)
                endingHintStyle.fontSize = --fontSize;
            endingHintShadowStyle.fontSize = fontSize;

            float height = fontSize * 1.8f;
            float bottomGap = Mathf.Max(14f, Screen.height * .03f);
            var rect = new Rect(16f, Screen.height - bottomGap - height, Screen.width - 32f, height);
            GUI.Label(new Rect(rect.x + 1.5f, rect.y + 1.5f, rect.width, rect.height),
                      content, endingHintShadowStyle);
            GUI.Label(rect, content, endingHintStyle);

            int thanksSize = Mathf.Clamp(Mathf.RoundToInt(Screen.height * .06f), 24, 58);
            var thanks = new GUIContent(EndingThanks);
            endingHintStyle.fontSize = thanksSize;
            while (thanksSize > 18 && endingHintStyle.CalcSize(thanks).x > Screen.width - 32f)
                endingHintStyle.fontSize = --thanksSize;
            endingHintShadowStyle.fontSize = thanksSize;

            float topGap = Mathf.Max(16f, Screen.height * .035f);
            var thanksRect = new Rect(16f, topGap, Screen.width - 32f, thanksSize * 1.8f);
            GUI.Label(new Rect(thanksRect.x + 1.5f, thanksRect.y + 1.5f,
                               thanksRect.width, thanksRect.height), thanks, endingHintShadowStyle);
            GUI.Label(thanksRect, thanks, endingHintStyle);
        }

        float PanelOpacity(int number)
        {
            if (number > visiblePanels) return 0f;
            if (number < visiblePanels) return 1f;
            return Mathf.Clamp01((Time.time - newestPanelAt) / PanelFade);
        }

        static void DrawSolid(Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        static void DrawPanel(Texture2D texture, float x, float y, float width, float height,
                              float left, float top, float scale, float opacity)
        {
            if (!texture || opacity <= 0f) return;
            GUI.color = new Color(1f, 1f, 1f, opacity);
            GUI.DrawTexture(new Rect(left + x * scale, top + y * scale,
                                     width * scale, height * scale), texture, ScaleMode.StretchToFill, true);
            GUI.color = Color.white;
        }

        static void DrawRightEdge(Texture2D texture, float x, float y, float width, float height,
                                  float left, float top, float scale, float opacity)
        {
            if (!texture || opacity <= 0f) return;
            GUI.color = new Color(1f, 1f, 1f, opacity);
            GUI.DrawTextureWithTexCoords(
                new Rect(left + x * scale, top + y * scale, width * scale, height * scale),
                texture, new Rect(1f - 1f / texture.width, 0f, 1f / texture.width, 1f), true);
            GUI.color = Color.white;
        }

        void DrawIntroRightPanel(float left, float top, float scale, float opacity)
        {
            var texture = intro[2];
            if (!texture || opacity <= 0f) return;
            // The source has a narrow pale margin at its left edge. Crop it so
            // the panel fills the top, bottom and right without widening the gutter.
            float inset = Mathf.Min(16f, texture.width * .02f) / texture.width;
            GUI.color = new Color(1f, 1f, 1f, opacity);
            GUI.DrawTextureWithTexCoords(
                new Rect(left + 1414f * scale, top, 1586f * scale, CanvasHeight * scale),
                texture, new Rect(inset, 0f, 1f - inset, 1f), true);
            GUI.color = Color.white;
        }
    }
}
