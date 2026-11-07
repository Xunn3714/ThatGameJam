using UnityEngine;
using UnityEngine.InputSystem;

namespace SeedGame
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class SeedPlayer : MonoBehaviour
    {
        public float moveSpeed = 3.4f;
        public float jumpSpeed = 10f;
        public float resonanceRadius = 5f;
        public float energyRechargeSeconds = 3f;
        public float glideGravityScale = .05f;
        public float glideMaxFallSpeed = 1.4f;
        public bool hasGlideLeaf;
        public ResonancePlant[] plants;
        public LayerMask groundMask = 1 << 6;
        public Transform feet;
        public float fallResetY = -8f;
        public Transform checkpoint;
        Vector3 startPosition;
        public bool controlsEnabled = true;
        Rigidbody2D body;
        float horizontal;
        bool jumpQueued;
        float lastPulse;
        float lastEmptyAttempt = -10f;
        int maxEnergy;
        int energy;
        int pulseStage;
        int pulseSerial;
        float nextRecharge;
        float pulseUntil;
        float pulseBaseRadius;
        Vector3 pulseCenter;
        PhysicsMaterial2D contactMaterial;
        ResonanceBurstVfx resonanceVfx;
        SeedWorldHud worldHud;
        float normalGravity;
        public int PulseSerial => pulseSerial;
        public float PulseEndsAt => pulseUntil;
        public int Energy => energy;
        public int MaxEnergy => maxEnergy;
        public bool IsOnGround => IsGrounded();

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            normalGravity = body.gravityScale;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            // Repeated movement input presses the body into a wall. Zero friction lets it fall naturally.
            contactMaterial = new PhysicsMaterial2D("Player contact (no friction)")
            {
                friction = 0f,
                bounciness = 0f
            };
            GetComponent<BoxCollider2D>().sharedMaterial = contactMaterial;
            startPosition = transform.position;
            // A scene saved while the intro was locked must still accept input on the next Play.
            controlsEnabled = true;
            resonanceVfx = GetComponent<ResonanceBurstVfx>();
            if (!resonanceVfx) resonanceVfx = gameObject.AddComponent<ResonanceBurstVfx>();
        }
        void OnDestroy()
        {
            if (contactMaterial != null) Destroy(contactMaterial);
        }
        void Start()
        {
            ApplyGearCount(GearState.Count);
            if (plants == null || plants.Length == 0)
                plants = FindObjectsByType<ResonancePlant>();
            worldHud = GetComponent<SeedWorldHud>();
            if (!worldHud) worldHud = gameObject.AddComponent<SeedWorldHud>();
        }
        public void ShowGearEnergy() => worldHud?.CelebrateGearGain();
        public void ApplyGearCount(int count)
        {
            int newMax = Mathf.Clamp(count, 0, 4);
            if (newMax > maxEnergy)
                energy += newMax - maxEnergy;
            maxEnergy = newMax;
            energy = Mathf.Min(energy, maxEnergy);
            if (energy == maxEnergy) nextRecharge = 0f;
        }
        void Update()
        {
            if (energy < maxEnergy && nextRecharge > 0f && Time.time >= nextRecharge)
            {
                energy++;
                GameAudio.PlayEnergyRecharge();
                nextRecharge = energy < maxEnergy ? Time.time + energyRechargeSeconds : 0f;
            }
            if (!controlsEnabled) return;
            bool right = false, left = false, jump = false, resonate = false;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                right |= keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
                left |= keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
                jump |= keyboard.spaceKey.wasPressedThisFrame;
                resonate |= keyboard.eKey.wasPressedThisFrame;
            }
            horizontal = (right ? 1 : 0) - (left ? 1 : 0);
            if (jump) jumpQueued = true;
            if (resonate) Pulse();
            if (transform.position.y < fallResetY) Respawn();
        }
        void FixedUpdate()
        {
            if (!controlsEnabled) { body.linearVelocity = new Vector2(0, body.linearVelocity.y); return; }
            bool glide = hasGlideLeaf && Keyboard.current != null && Keyboard.current.spaceKey.isPressed
                && body.linearVelocity.y < 0f && !IsGrounded();
            body.gravityScale = glide ? glideGravityScale : normalGravity;
            if (glide && body.linearVelocity.y < -glideMaxFallSpeed)
                body.linearVelocity = new Vector2(body.linearVelocity.x, -glideMaxFallSpeed);
            body.linearVelocity = new Vector2(horizontal * moveSpeed, body.linearVelocity.y);
            if (jumpQueued && IsGrounded()) body.linearVelocity = new Vector2(body.linearVelocity.x, jumpSpeed);
            jumpQueued = false;
        }
        bool IsGrounded() => Physics2D.OverlapCircle(feet.position, .13f, groundMask) != null;
        void Pulse()
        {
            if (energy == 0)
            {
                if (Time.time - lastEmptyAttempt >= .3f)
                {
                    lastEmptyAttempt = Time.time;
                    GameAudio.PlayNoEnergy();
                    worldHud?.WarnNoEnergy();
                }
                return;
            }
            if (Time.time - lastPulse < .18f) return;
            lastPulse = Time.time;
            energy--;
            worldHud?.ShowForFourSeconds();
            if (nextRecharge <= 0f) nextRecharge = Time.time + energyRechargeSeconds;
            if (Time.time < pulseUntil && pulseStage < 4)
                pulseStage++;
            else
            {
                pulseStage = 1;
                pulseSerial++;
                pulseCenter = transform.position;
                pulseBaseRadius = ResonanceBaseRadius();
            }
            pulseUntil = Time.time + 2f;
            GameAudio.PlayResonance(pulseStage);
            float radius = pulseBaseRadius * RadiusMultiplier(pulseStage);
            if (plants == null || plants.Length == 0)
                plants = FindObjectsByType<ResonancePlant>();
            resonanceVfx.Play(pulseCenter, radius, pulseStage);
            int plantsInRange = 0;
            foreach (var plant in plants)
                if (plant && Vector2.Distance(pulseCenter, plant.transform.position) <= radius)
                {
                    plantsInRange++;
                    plant.Activate();
                }
            foreach (var target in FindObjectsByType<Level02ResonanceTarget>(FindObjectsSortMode.None))
                if (target && Vector2.Distance(pulseCenter, target.transform.position) <= radius)
                {
                    plantsInRange++;
                    target.OnResonance(this, pulseSerial);
                }
            if (plantsInRange == 0)
                Debug.Log("Resonance reached no plant at " + transform.position);
        }
        float ResonanceBaseRadius()
        {
            var gate = FindFirstObjectByType<Level02Gate>();
            if (gate && !gate.IsOpen && pulseCenter.x >= gate.transform.position.x - 8f &&
                pulseCenter.x < gate.transform.position.x)
                return Mathf.Max(resonanceRadius, 8.6f);
            return resonanceRadius;
        }
        static float RadiusMultiplier(int stage)
        {
            switch (stage)
            {
                case 2: return 1.55f;
                case 3: return 1.9f;
                case 4: return 2.15f;
                default: return 1f;
            }
        }
        public void SetCheckpoint(Transform point) { checkpoint = point; }
        public void Respawn()
        {
            transform.position = checkpoint != null ? checkpoint.position : startPosition;
            body.linearVelocity = Vector2.zero;
        }
    }
}
