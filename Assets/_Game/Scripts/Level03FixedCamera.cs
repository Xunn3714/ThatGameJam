using UnityEngine;

namespace SeedGame
{
    // Keeps every pixel of the wide painted scene in view without following the hero.
    [RequireComponent(typeof(Camera))]
    public sealed class Level03FixedCamera : MonoBehaviour
    {
        const float ArtAspect = 4096f / 1714f;
        Camera sceneCamera;
        int lastWidth;
        int lastHeight;

        void Awake()
        {
            sceneCamera = GetComponent<Camera>();
            sceneCamera.orthographic = true;
            sceneCamera.orthographicSize = 1714f / 400f;
            transform.position = new Vector3(0f, 0f, -10f);
            SetViewport();
        }

        void LateUpdate()
        {
            if (Screen.width != lastWidth || Screen.height != lastHeight) SetViewport();
        }

        void SetViewport()
        {
            lastWidth = Mathf.Max(1, Screen.width);
            lastHeight = Mathf.Max(1, Screen.height);
            float screenAspect = lastWidth / (float)lastHeight;
            if (screenAspect < ArtAspect)
            {
                float height = screenAspect / ArtAspect;
                sceneCamera.rect = new Rect(0f, (1f - height) * .5f, 1f, height);
            }
            else
            {
                float width = ArtAspect / screenAspect;
                sceneCamera.rect = new Rect((1f - width) * .5f, 0f, width, 1f);
            }
        }
    }
}
