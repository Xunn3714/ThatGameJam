using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeedGame
{
    // Small world-space prompts stay beside the character as the camera moves.
    public sealed class SeedWorldHud : MonoBehaviour
    {
        SeedPlayer player;
        Sprite[] energyArt;
        SpriteRenderer previousEnergy;
        SpriteRenderer currentEnergy;
        SpriteRenderer keyArt;
        Renderer keyLabelRenderer;
        int shownEnergy;
        float blend = 1f;
        float visibility;
        float visibleUntil;
        float gearFlashStartedAt = -10f;
        bool firstLevel;

        void Awake()
        {
            player = GetComponent<SeedPlayer>();
            firstLevel = SceneManager.GetActiveScene().name == "Level01_Alpha";
            energyArt = new[] {
                Resources.Load<Sprite>("UI/CelticEnergy1"),
                Resources.Load<Sprite>("UI/CelticEnergy2"),
                Resources.Load<Sprite>("UI/CelticEnergy3")
            };
            previousEnergy = SpritePart("Previous energy", new Vector3(-1.08f, .18f, 0), 1.25f, 24);
            currentEnergy = SpritePart("Current energy", new Vector3(-1.08f, .18f, 0), 1.25f, 25);
            keyArt = SpritePart("E resonance key", new Vector3(.9f, 1.2f, 0), .30f, 25);
            keyArt.sprite = Resources.Load<Sprite>("UI/ResonanceKey");
            keyArt.color = Color.white;
            keyArt.enabled = false;
            var keyLabel = TextPart("Resonance label", "Resonance", new Vector3(1.18f, 1.2f, 0), .11f, 25);
            keyLabelRenderer = keyLabel.GetComponent<Renderer>();
            keyLabelRenderer.enabled = false;
            shownEnergy = player ? player.Energy : 0;
            currentEnergy.sprite = EnergySprite(shownEnergy);
        }

        public void ShowForFourSeconds() => visibleUntil = Mathf.Max(visibleUntil, Time.time + 4f);

        public void CelebrateGearGain()
        {
            visibleUntil = Mathf.Max(visibleUntil, Time.time + 6f);
            gearFlashStartedAt = Time.time;
        }

        void Update()
        {
            if (!player) return;
            int energy = player.Energy;
            if (energy != shownEnergy)
            {
                if (energy > shownEnergy) ShowForFourSeconds();
                previousEnergy.sprite = EnergySprite(shownEnergy);
                currentEnergy.sprite = EnergySprite(energy);
                blend = 0f;
                shownEnergy = energy;
            }
            blend = Mathf.MoveTowards(blend, 1f, Time.deltaTime * 2.5f);
            visibility = Mathf.MoveTowards(visibility, Time.time < visibleUntil ? 1f : 0f,
                Time.deltaTime * 3f);
            SetAlpha(previousEnergy, visibility * (1f - blend));
            float flashTime = Time.time - gearFlashStartedAt;
            float flashPulse = flashTime < 2.1f
                ? .5f + .5f * Mathf.Sin(flashTime * Mathf.PI * 6f / 2.1f)
                : 1f;
            float flash = flashTime < 2.1f ? .55f + .45f * flashPulse : 1f;
            currentEnergy.transform.localScale = Vector3.one *
                (flashTime < 2.1f ? 1.25f + .4f * flashPulse : 1.25f);
            AlignEnergy(previousEnergy);
            AlignEnergy(currentEnergy);
            SetAlpha(currentEnergy, visibility * blend * flash);
            bool showKey = false;
            if (firstLevel && player.MaxEnergy > 0 && player.plants != null)
                foreach (var plant in player.plants)
                    if (plant && !plant.isActive &&
                        Vector2.Distance(player.transform.position, plant.transform.position) < 5.2f)
                    {
                        showKey = true;
                        break;
                    }
            keyArt.enabled = showKey;
            keyLabelRenderer.enabled = showKey;
        }

        Sprite EnergySprite(int count) => count > 0 ? energyArt[Mathf.Clamp(count - 1, 0, 2)] : null;

        // All three images share the pink first rune, but their canvases have
        // different sizes. Keep that rune registered to one world-space point.
        void AlignEnergy(SpriteRenderer renderer)
        {
            if (!renderer.sprite) return;
            var sprite = renderer.sprite;
            float scale = renderer.transform.localScale.x / sprite.pixelsPerUnit;
            float firstRuneX = sprite == energyArt[0] ? 0f :
                sprite == energyArt[1] ? 17f : 32f;
            renderer.transform.localPosition = new Vector3(
                -1.35f + (sprite.rect.width * .5f - firstRuneX) * scale,
                .63f - sprite.rect.height * .5f * scale, 0f);
        }

        SpriteRenderer SpritePart(string name, Vector3 position, float scale, int order)
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            child.transform.localPosition = position;
            child.transform.localScale = Vector3.one * scale;
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = order;
            renderer.color = Color.clear;
            return renderer;
        }

        TextMesh TextPart(string name, string value, Vector3 position, float size, int order)
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            child.transform.localPosition = position;
            var text = child.AddComponent<TextMesh>();
            text.text = value;
            text.fontSize = 48;
            text.characterSize = size;
            text.anchor = TextAnchor.MiddleLeft;
            text.color = Color.white;
            child.GetComponent<Renderer>().sortingOrder = order;
            return text;
        }

        static void SetAlpha(SpriteRenderer renderer, float alpha)
        {
            renderer.color = new Color(1f, 1f, 1f, alpha);
        }
    }
}
