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
        public Transform[] rubble;
        public Transform[] growingVines;
        public Vector3[] vineFullScales;
        public SpriteRenderer jammingSlab;
        public Sprite[] slabBreakFrames;
        public Transform[] gears;
        public float doorRise = 6f;
        // The five visible gears are ordered: right 1/2/3, left small, right small.
        // Angular speed is inversely proportional to each gear's visible diameter.
        static readonly float[] GearSpeeds = { -18f, 21.6f, -11.5f, 37f, -36f };
        bool open;
        bool fullyOpen;
        int pushingPlants;
        Vector3[] rubbleStarts;
        Quaternion[] rubbleRotations;
        Sprite intactSlab;
        Sprite foldedLeftSprite;
        Sprite foldedRightSprite;
        SpriteRenderer fallenSlab;
        Coroutine foldRoutine;
        public bool IsOpen => open;
        public bool IsFullyOpen => fullyOpen;

        void Awake()
        {
            if (jammingSlab) intactSlab = jammingSlab.sprite;
            rubbleStarts = new Vector3[rubble == null ? 0 : rubble.Length];
            rubbleRotations = new Quaternion[rubbleStarts.Length];
            for (int i = 0; i < rubbleStarts.Length; i++)
            {
                if (!rubble[i]) continue;
                rubbleStarts[i] = rubble[i].position;
                rubbleRotations[i] = rubble[i].rotation;
            }
        }

        void OnDestroy()
        {
            if (foldedLeftSprite) Destroy(foldedLeftSprite);
            if (foldedRightSprite) Destroy(foldedRightSprite);
        }

        void Update()
        {
            if (open || !player || flowers == null || Time.time < player.PulseEndsAt) return;
            foreach (var flower in flowers)
                if (flower) flower.ResetForNextPulse();
            if (pushingPlants > 0) ResetBreakage();
        }

        void ResetBreakage()
        {
            StopAllCoroutines();
            foldRoutine = null;
            if (fallenSlab) Destroy(fallenSlab.gameObject);
            fallenSlab = null;
            pushingPlants = 0;
            if (jammingSlab && slabBreakFrames != null && slabBreakFrames.Length > 0)
            {
                // The first keyframe is the intact sprite already assigned in the scene.
                jammingSlab.sprite = intactSlab;
            }
            for (int i = 0; i < rubbleStarts.Length; i++)
                if (rubble[i])
                {
                    rubble[i].position = rubbleStarts[i];
                    rubble[i].rotation = rubbleRotations[i];
                }
        }

        public void OnPlantPushing(Level02GateFlower flower)
        {
            if (open || !flower || flowers == null) return;
            int index = System.Array.IndexOf(flowers, flower);
            if (index < 0) return;
            pushingPlants++;
            if (jammingSlab && slabBreakFrames != null && slabBreakFrames.Length > 0)
            {
                if (pushingPlants <= 2) jammingSlab.sprite = slabBreakFrames[0];
                else if (!fallenSlab && foldRoutine == null)
                    foldRoutine = StartCoroutine(FoldSlab());
            }
            if (rubble != null && index < rubble.Length && rubble[index])
                StartCoroutine(PushRock(index));
        }

        IEnumerator PushRock(int index)
        {
            var rock = rubble[index];
            Vector3 start = rubbleStarts[index];
            Quaternion rotation = rubbleRotations[index];
            float direction = index % 2 == 0 ? -1f : 1f;
            for (float t = 0; t < .55f; t += Time.deltaTime)
            {
                float p = Mathf.SmoothStep(0f, 1f, t / .55f);
                if (rock)
                {
                    rock.position = start + new Vector3(direction * .7f, -.4f, 0f) * p;
                    rock.rotation = rotation * Quaternion.Euler(0f, 0f, direction * 28f * p);
                }
                yield return null;
            }
        }

        IEnumerator FoldSlab()
        {
            var source = slabBreakFrames[slabBreakFrames.Length - 1];
            if (!source || !jammingSlab) yield break;
            float cut = Mathf.Round(source.rect.width * .655f);
            float ppu = source.pixelsPerUnit;
            if (!foldedLeftSprite)
            {
                foldedLeftSprite = Sprite.Create(source.texture,
                    new Rect(source.rect.x, source.rect.y, cut, source.rect.height),
                    new Vector2(source.pivot.x / cut, source.pivot.y / source.rect.height), ppu);
                foldedRightSprite = Sprite.Create(source.texture,
                    new Rect(source.rect.x + cut, source.rect.y, source.rect.width - cut, source.rect.height),
                    new Vector2(0f, .64f), ppu);
            }
            Vector3 hinge = jammingSlab.transform.TransformPoint(new Vector3(
                (cut - source.pivot.x) / ppu,
                (.64f * source.rect.height - source.pivot.y) / ppu, 0f));
            jammingSlab.sprite = foldedLeftSprite;
            fallenSlab = new GameObject("Fallen right half of gear-blocking slab").AddComponent<SpriteRenderer>();
            fallenSlab.sprite = foldedRightSprite;
            fallenSlab.sortingLayerID = jammingSlab.sortingLayerID;
            fallenSlab.sortingOrder = jammingSlab.sortingOrder;
            fallenSlab.transform.position = hinge;
            fallenSlab.transform.rotation = jammingSlab.transform.rotation;
            fallenSlab.transform.localScale = jammingSlab.transform.lossyScale;
            Quaternion start = fallenSlab.transform.rotation;
            Quaternion end = start * Quaternion.Euler(0f, 0f, -50f);
            for (float t = 0f; t < .8f; t += Time.deltaTime)
            {
                if (!fallenSlab) yield break;
                fallenSlab.transform.rotation = Quaternion.Slerp(start, end,
                    Mathf.SmoothStep(0f, 1f, t / .8f));
                yield return null;
            }
            if (fallenSlab) fallenSlab.transform.rotation = end;
            foldRoutine = null;
        }

        public void Check(int serial)
        {
            if (open || flowers == null || flowers.Length != 4) return;
            foreach (var flower in flowers)
                if (!flower || flower.ActivePulse != serial || !flower.FullyGrown) return;
            open = true;
            StartCoroutine(Open());
        }

        IEnumerator Open()
        {
            if (jammingSlab && !fallenSlab && slabBreakFrames != null && slabBreakFrames.Length > 0)
                jammingSlab.sprite = slabBreakFrames[slabBreakFrames.Length - 1];
            if (foldRoutine != null) yield return foldRoutine;
            yield return new WaitForSeconds(.35f);
            Vector3 doorStart = gateVisual ? gateVisual.transform.position : transform.position;
            // The small gear turns at roughly 0.7 world units per second along
            // its rim. Lift the door at the same visible linear pace.
            const float doorSpeed = .7f;
            const float gearLeadIn = .45f;
            float liftDuration = doorRise / doorSpeed;
            float duration = gearLeadIn + liftDuration;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float lift = Mathf.Clamp01((t - gearLeadIn) / liftDuration);
                if (gears != null)
                    for (int i = 0; i < gears.Length; i++)
                        if (gears[i] && i < GearSpeeds.Length)
                            gears[i].Rotate(0f, 0f, GearSpeeds[i] * Time.deltaTime);
                if (gateVisual) gateVisual.transform.position = doorStart + Vector3.up * (doorRise * lift);
                if (gateCollider && lift > .9f) gateCollider.enabled = false;
                yield return null;
            }
            if (gateVisual) gateVisual.transform.position = doorStart + Vector3.up * doorRise;
            if (gateCollider) gateCollider.enabled = false;
            fullyOpen = true;
        }
    }
}
