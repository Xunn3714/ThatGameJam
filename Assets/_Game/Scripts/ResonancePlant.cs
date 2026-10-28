using UnityEngine;

namespace SeedGame
{
    public sealed class ResonancePlant : MonoBehaviour
    {
        public GameObject dormantVisual;
        public GameObject grownRoot;
        public bool isActive;
        public bool generatePlaceholderSteps;
        public bool oneWayPlatforms;
        public Vector4[] placeholderSteps;
        static Sprite placeholderSprite;
        static Texture2D placeholderTexture;
        public void Activate()
        {
            if (isActive) return;
            isActive = true;
            if (dormantVisual) dormantVisual.SetActive(false);
            if (oneWayPlatforms && grownRoot)
                foreach (var collider in grownRoot.GetComponentsInChildren<Collider2D>(true))
                    MakeOneWay(collider);
            if (generatePlaceholderSteps && placeholderSteps != null && placeholderSteps.Length > 0)
                GenerateSteps();
            else if (grownRoot)
                grownRoot.SetActive(true);
            Debug.Log("Resonance activated: " + name);
        }
        void GenerateSteps()
        {
            if (!placeholderSprite)
            {
                placeholderTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                placeholderTexture.SetPixel(0, 0, Color.white);
                placeholderTexture.Apply();
                placeholderSprite = Sprite.Create(placeholderTexture,
                    new Rect(0, 0, 1, 1), new Vector2(.5f, .5f), 1);
            }
            foreach (var step in placeholderSteps)
            {
                var go = new GameObject("Resonance leaf step");
                go.transform.SetParent(transform);
                go.transform.position = new Vector3(step.x, step.y, 0);
                go.transform.localScale = new Vector3(step.z, step.w, 1);
                go.layer = 6;
                var sprite = go.AddComponent<SpriteRenderer>();
                sprite.sprite = placeholderSprite;
                sprite.color = new Color(.69f, .9f, .48f);
                sprite.sortingOrder = 6;
                var collider = go.AddComponent<BoxCollider2D>();
                collider.size = Vector2.one;
                if (oneWayPlatforms) MakeOneWay(collider);
            }
        }
        static void MakeOneWay(Collider2D collider)
        {
            var effector = collider.GetComponent<PlatformEffector2D>();
            if (!effector) effector = collider.gameObject.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.useOneWayGrouping = true;
            effector.surfaceArc = 175f;
            effector.useSideFriction = false;
            effector.useSideBounce = false;
            collider.usedByEffector = true;
        }
    }
}
