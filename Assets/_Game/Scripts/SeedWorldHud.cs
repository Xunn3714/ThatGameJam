using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace SeedGame
{
    // Small world-space prompts stay beside the character as the camera moves.
    public sealed class SeedWorldHud : MonoBehaviour
    {
        enum Hint { None, Move, Jump, Resonance, Glide, Gate }

        SeedPlayer player;
        Sprite[] energyArt;
        SpriteRenderer previousEnergy;
        SpriteRenderer currentEnergy;
        SpriteRenderer[] moveHint;
        SpriteRenderer[] jumpHint;
        SpriteRenderer[] resonanceHint;
        SpriteRenderer[] glideHint;
        SpriteRenderer[] gateHint;
        Transform gateHintRoot;
        float gateHintCenterX;
        float gateHintTopY;
        FirstLevelTutorial tutorial;
        Level02Oxalis oxalis;
        Level02Gate gate;
        Rigidbody2D playerBody;
        bool secondLevel;
        bool moved;
        bool jumped;
        bool glided;
        float nextIdleReminderAt;
        Vector3 lastPosition;
        int shownEnergy;
        float blend = 1f;
        float visibility;
        float visibleUntil;
        float gearFlashStartedAt = -10f;
        float energyWarningUntil;
        float energyWarningStartedAt;
        bool firstLevel;
        bool thirdLevel;
        Level03RebirthSequence rebirth;

        void Awake()
        {
            player = GetComponent<SeedPlayer>();
            playerBody = GetComponent<Rigidbody2D>();
            firstLevel = SceneManager.GetActiveScene().name == "Level01_Alpha";
            secondLevel = SceneManager.GetActiveScene().name == "Level02_Alpha";
            thirdLevel = SceneManager.GetActiveScene().name == "Level03_Alpha";
            energyArt = new[] {
                Resources.Load<Sprite>("UI/CelticEnergy1"),
                Resources.Load<Sprite>("UI/CelticEnergy2"),
                Resources.Load<Sprite>("UI/CelticEnergy3")
            };
            previousEnergy = SpritePart("Previous energy", new Vector3(-1.08f, .18f, 0), 1.25f, 24);
            currentEnergy = SpritePart("Current energy", new Vector3(-1.08f, .18f, 0), 1.25f, 25);
            moveHint = new[] {
                HintPart("Move A", "UI/MoveA", .50f, 1.12f, .40f),
                HintPart("Move or", "UI/MoveOr", .91f, 1.12f, .28f),
                HintPart("Move D", "UI/MoveD", 1.32f, 1.12f, .40f),
                HintPart("Move label", "UI/MoveLabel", 2.16f, 1.12f, 1.02f)
            };
            jumpHint = new[] {
                HintPart("Jump Space", "UI/JumpSpace", .72f, 1.12f, .80f),
                HintPart("Jump label", "UI/JumpLabel", 1.84f, 1.12f, 1.08f)
            };
            resonanceHint = new[] {
                HintPart("Resonance E", "UI/ResonanceKey", .52f, 1.12f, .40f),
                HintPart("Resonance label", "UI/ResonanceLabelRounded", 1.82f, 1.12f, 1.76f)
            };
            glideHint = new[] {
                HintPart("Glide Hold", "UI/HoldLabel", .66f, 1.12f, .78f),
                HintPart("Glide Space", "UI/JumpSpace", 1.49f, 1.12f, .78f),
                HintPart("Glide label", "UI/GlideLabel", 2.48f, 1.12f, .98f)
            };
            gateHintRoot = new GameObject("Gate hint group").transform;
            gateHintRoot.SetParent(transform, false);
            gateHint = new[] {
                HintPart("Gate Press", "UI/PressLabel", .52f, 1.40f, .70f, gateHintRoot),
                HintPart("Gate E", "UI/ResonanceKey", 1.12f, 1.40f, .40f, gateHintRoot),
                HintPart("Gate multiple times", "UI/MultipleTimesLabel", 2.26f, 1.40f, 1.70f, gateHintRoot),
                HintPart("Gate extend range", "UI/ExtendRangeLabel", 1.68f, 1.04f, 2.52f, gateHintRoot)
            };
            SetHint(Hint.None);
            shownEnergy = player ? player.Energy : 0;
            currentEnergy.sprite = EnergySprite(shownEnergy);
            lastPosition = transform.position;
            nextIdleReminderAt = Time.time + 7f;
        }

        public void ShowForFourSeconds() => visibleUntil = Mathf.Max(visibleUntil, Time.time + 4f);

        void Start()
        {
            if (firstLevel) tutorial = FindAnyObjectByType<FirstLevelTutorial>();
            if (secondLevel)
            {
                oxalis = FindAnyObjectByType<Level02Oxalis>();
                gate = FindAnyObjectByType<Level02Gate>();
                var cameraZone = FindAnyObjectByType<Level02GateCameraZone>();
                gateHintRoot.SetParent(cameraZone ? cameraZone.transform : null, true);
                var mechanismFloor = GameObject.Find("地板_0");
                var floorArt = mechanismFloor ? mechanismFloor.GetComponent<SpriteRenderer>() : null;
                gateHintCenterX = floorArt ? floorArt.bounds.center.x :
                    cameraZone ? (cameraZone.viewMin.x + cameraZone.viewMax.x) * .5f :
                    gate ? gate.transform.position.x + 8f : 73.8f;
                gateHintTopY = floorArt ? floorArt.bounds.min.y - .9f : 1.2f;
            }
            if (thirdLevel) rebirth = FindAnyObjectByType<Level03RebirthSequence>();
        }

        public void WarnNoEnergy()
        {
            if (!player || player.MaxEnergy == 0) return;
            energyWarningStartedAt = Time.time;
            energyWarningUntil = Time.time + 1.5f;
            visibleUntil = Mathf.Max(visibleUntil, Time.time + 2f);
        }

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
            bool warning = energy == 0 && player.MaxEnergy > 0 &&
                Time.time < energyWarningUntil;
            if (energy == 0) currentEnergy.sprite = warning ? energyArt[0] : null;
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
            bool red = warning &&
                Mathf.FloorToInt((Time.time - energyWarningStartedAt) * 7f) % 2 == 1;
            currentEnergy.color = new Color(1f, red ? .22f : 1f,
                red ? .22f : 1f, visibility * (warning ? 1f : blend) * flash);
            UpdateHints();
        }

        void LateUpdate()
        {
            if (!secondLevel || !gateHintRoot) return;
            var view = Camera.main;
            if (!view || !view.orthographic) return;
            var controller = view.GetComponent<SeedCamera>();
            float normalSize = controller ? controller.playSize : 4f;
            float zoom = view.orthographicSize / Mathf.Max(.1f, normalSize);
            float scale = Mathf.Clamp(zoom * 1.15f, 1.15f, 3f);
            gateHintRoot.localScale = Vector3.one * scale;
            // Keep both rows centered below the mechanism floor as the camera widens.
            gateHintRoot.position = new Vector3(gateHintCenterX - 1.65f * scale,
                gateHintTopY - 1.4f * scale, 0f);
        }

        void UpdateHints()
        {
            if (!player.controlsEnabled)
            {
                SetHint(Hint.None);
                return;
            }
            var keyboard = Keyboard.current;
            bool movePressed = keyboard != null &&
                (keyboard.aKey.isPressed || keyboard.dKey.isPressed ||
                 keyboard.leftArrowKey.isPressed || keyboard.rightArrowKey.isPressed);
            bool jumpPressed = keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
            bool resonatePressed = keyboard != null && keyboard.eKey.wasPressedThisFrame;
            if (movePressed) moved = true;
            if (jumpPressed) jumped = true;
            if (movePressed || jumpPressed || resonatePressed ||
                (transform.position - lastPosition).sqrMagnitude > .001f)
                nextIdleReminderAt = Time.time + 7f;
            lastPosition = transform.position;

            Hint hint = Hint.None;
            if (firstLevel)
            {
                float stumpX = tutorial ? tutorial.stumpX : 7.5f;
                if (!moved) hint = Hint.Move;
                else if (!jumped && transform.position.x >= stumpX - 2.2f)
                    hint = Hint.Jump;
                else if (player.MaxEnergy > 0 && player.Energy > 0 &&
                         transform.position.x > stumpX + 1.6f)
                {
                    var firstPlant = tutorial ? tutorial.firstPlant : null;
                    if (firstPlant && !firstPlant.isActive && Near(firstPlant.transform, 5.2f))
                        hint = Hint.Resonance;
                    else if (Time.time >= nextIdleReminderAt && NearInactivePlant())
                        hint = Hint.Resonance;
                }
            }
            else if (secondLevel)
            {
                if (gate && !gate.IsOpen &&
                    transform.position.x >= gate.transform.position.x - 10f &&
                    transform.position.x <= gate.transform.position.x + 15f &&
                    transform.position.y > 1f)
                    hint = Hint.Gate;
                else if (player.hasGlideLeaf && !glided) hint = Hint.Glide;
                else if (oxalis && !player.hasGlideLeaf && player.Energy > 0 &&
                         Near(oxalis.transform, 5f)) hint = Hint.Resonance;
                if (player.hasGlideLeaf && keyboard != null && keyboard.spaceKey.isPressed &&
                    playerBody && playerBody.linearVelocity.y < 0f && !player.IsOnGround)
                    glided = true;
            }
            else if (thirdLevel && rebirth && !rebirth.HasStarted && player.MaxEnergy > 0)
                hint = Hint.Resonance;
            SetHint(hint);
        }

        bool Near(Transform target, float distance) =>
            (transform.position - target.position).sqrMagnitude < distance * distance;

        bool NearInactivePlant()
        {
            if (player.plants == null) return false;
            foreach (var plant in player.plants)
                if (plant && !plant.isActive && Near(plant.transform, 5.2f)) return true;
            return false;
        }

        void SetHint(Hint hint)
        {
            SetHintGroup(moveHint, hint == Hint.Move);
            SetHintGroup(jumpHint, hint == Hint.Jump);
            SetHintGroup(resonanceHint, hint == Hint.Resonance);
            SetHintGroup(glideHint, hint == Hint.Glide);
            SetHintGroup(gateHint, hint == Hint.Gate);
        }

        static void SetHintGroup(SpriteRenderer[] group, bool enabled)
        {
            if (group == null) return;
            foreach (var sprite in group)
                if (sprite) sprite.enabled = enabled && sprite.sprite != null;
        }

        SpriteRenderer HintPart(string name, string resource, float x, float y, float width,
            Transform parent = null)
        {
            var renderer = SpritePart(name, new Vector3(x, y), 1f, 25, parent);
            renderer.sprite = Resources.Load<Sprite>(resource);
            if (renderer.sprite)
                renderer.transform.localScale = Vector3.one *
                    (width / renderer.sprite.bounds.size.x);
            renderer.color = Color.white;
            return renderer;
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

        SpriteRenderer SpritePart(string name, Vector3 position, float scale, int order,
            Transform parent = null)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent ? parent : transform, false);
            child.transform.localPosition = position;
            child.transform.localScale = Vector3.one * scale;
            var renderer = child.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = order;
            renderer.color = Color.clear;
            return renderer;
        }

        static void SetAlpha(SpriteRenderer renderer, float alpha)
        {
            renderer.color = new Color(1f, 1f, 1f, alpha);
        }
    }
}
