using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SeedGame
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class SeedPlayer : MonoBehaviour
    {
        public float moveSpeed = 3.8f;
        public float jumpSpeed = 10f;
        public float resonanceRadius = 5f;
        public float energyRechargeSeconds = 3f;
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
        int maxEnergy;
        int energy;
        int pulseStage;
        float nextRecharge;
        float pulseUntil;
        float visualRadius;
        Vector3 pulseCenter;
        float newEnergyFlashUntil;
        int newEnergySlot = -1;
        LineRenderer pulseRing;
        Material pulseMaterial;
        PhysicsMaterial2D contactMaterial;
        Coroutine pulseRoutine;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
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
            var ringObject = new GameObject("Reusable resonance ring");
            ringObject.transform.position = transform.position;
            pulseRing = ringObject.AddComponent<LineRenderer>();
            pulseRing.useWorldSpace = false;
            pulseRing.loop = true;
            pulseRing.positionCount = 48;
            pulseRing.startWidth = pulseRing.endWidth = .035f;
            pulseMaterial = new Material(Shader.Find("Sprites/Default"));
            pulseRing.sharedMaterial = pulseMaterial;
            pulseRing.startColor = pulseRing.endColor = new Color(1f, .82f, .45f, .8f);
            pulseRing.sortingOrder = 20;
            for (int i = 0; i < 48; i++)
            {
                float angle = i * Mathf.PI * 2f / 48f;
                pulseRing.SetPosition(i, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0));
            }
            pulseRing.enabled = false;
        }
        void OnDestroy()
        {
            if (pulseRing) Destroy(pulseRing.gameObject);
            if (pulseMaterial != null) Destroy(pulseMaterial);
            if (contactMaterial != null) Destroy(contactMaterial);
        }
        void Start()
        {
            ApplyGearCount(GearState.Count);
            if (plants == null || plants.Length == 0)
                plants = FindObjectsByType<ResonancePlant>();
        }
        public void ApplyGearCount(int count)
        {
            int newMax = Mathf.Clamp(count, 0, 4);
            if (newMax > maxEnergy)
            {
                energy += newMax - maxEnergy;
                newEnergySlot = newMax - 1;
                newEnergyFlashUntil = Time.time + 1.15f;
            }
            maxEnergy = newMax;
            energy = Mathf.Min(energy, maxEnergy);
            if (energy == maxEnergy) nextRecharge = 0f;
        }
        void Update()
        {
            if (energy < maxEnergy && nextRecharge > 0f && Time.time >= nextRecharge)
            {
                energy++;
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
            body.linearVelocity = new Vector2(horizontal * moveSpeed, body.linearVelocity.y);
            if (jumpQueued && IsGrounded()) body.linearVelocity = new Vector2(body.linearVelocity.x, jumpSpeed);
            jumpQueued = false;
        }
        bool IsGrounded() => Physics2D.OverlapCircle(feet.position, .13f, groundMask) != null;
        void Pulse()
        {
            if (Time.time - lastPulse < .18f || energy == 0) return;
            lastPulse = Time.time;
            energy--;
            if (nextRecharge <= 0f) nextRecharge = Time.time + energyRechargeSeconds;
            if (Time.time < pulseUntil && pulseStage < 4)
                pulseStage++;
            else
            {
                pulseStage = 1;
                pulseCenter = transform.position;
                visualRadius = 0f;
            }
            pulseUntil = Time.time + 2f;
            float radius = resonanceRadius * RadiusMultiplier(pulseStage);
            if (plants == null || plants.Length == 0)
                plants = FindObjectsByType<ResonancePlant>();
            if (pulseRoutine != null) StopCoroutine(pulseRoutine);
            pulseRing.enabled = true;
            pulseRing.transform.position = pulseCenter;
            pulseRoutine = StartCoroutine(PulseVisual());
            int plantsInRange = 0;
            foreach (var plant in plants)
                if (plant && Vector2.Distance(pulseCenter, plant.transform.position) <= radius)
                {
                    plantsInRange++;
                    plant.Activate();
                }
            if (plantsInRange == 0)
                Debug.Log("Resonance reached no plant at " + transform.position);
        }
        IEnumerator PulseVisual()
        {
            while (Time.time < pulseUntil)
            {
                float target = resonanceRadius * RadiusMultiplier(pulseStage);
                visualRadius = Mathf.MoveTowards(visualRadius, target, resonanceRadius * 8f * Time.deltaTime);
                pulseRing.transform.localScale = new Vector3(visualRadius, visualRadius, 1f);
                yield return null;
            }
            pulseRing.enabled = false;
            pulseRoutine = null;
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
        void OnGUI()
        {
            if (maxEnergy == 0) return;
            for (int i = 0; i < maxEnergy; i++)
            {
                float flash = i == newEnergySlot ? Mathf.Clamp01((newEnergyFlashUntil - Time.time) / 1.15f) : 0f;
                float grow = flash * 7f;
                GUI.color = i < energy ? new Color(1f, .82f, .4f) : Color.gray;
                GUI.Box(new Rect(16 + i * 36 - grow, 16 - grow, 30 + grow * 2f, 30 + grow * 2f),
                    i < energy ? "✦" : "·");
            }
            GUI.color = Color.white;
        }
        public void SetCheckpoint(Transform point) { checkpoint = point; }
        public void Respawn()
        {
            transform.position = checkpoint != null ? checkpoint.position : startPosition;
            body.linearVelocity = Vector2.zero;
        }
    }
}
