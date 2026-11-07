using System.Collections;
using UnityEngine;

namespace SeedGame
{
    public sealed class Level02GateFlower : Level02ResonanceTarget
    {
        public Level02Gate gate;
        public SpriteRenderer visual;
        public Sprite[] growthFrames;
        public SpriteRenderer[] placedStages;
        public int ActivePulse { get; private set; } = -1;
        public bool FullyGrown { get; private set; }
        InteractableHighlight highlight;
        Coroutine growingRoutine;
        float growth;

        void Awake() => Level02PlacedStages.Show(placedStages, 0);

        void Start() => highlight = Level02PlacedStages.HighlightFirst(placedStages);

        public override void OnResonance(SeedPlayer player, int pulseSerial)
        {
            if (ActivePulse == pulseSerial) return;
            ActivePulse = pulseSerial;
            GameAudio.PlayPlantGrowth();
            growth = 0f;
            FullyGrown = false;
            if (highlight) highlight.Hide();
            if (growingRoutine != null) StopCoroutine(growingRoutine);
            growingRoutine = StartCoroutine(Grow(pulseSerial));
        }

        IEnumerator Grow(int serial)
        {
            yield return new WaitForSeconds(.32f);
            if (ActivePulse != serial) yield break;
            Level02PlacedStages.Show(placedStages, 1);
            if (gate) gate.OnPlantPushing(this);
            yield return new WaitForSeconds(.48f);
            if (ActivePulse != serial) yield break;
            Level02PlacedStages.Show(placedStages, 2);
            FullyGrown = true;
            if (gate) gate.Check(serial);
            growingRoutine = null;
        }

        public void ResetForNextPulse()
        {
            if (ActivePulse < 0) return;
            ActivePulse = -1;
            growth = 0f;
            FullyGrown = false;
            if (growingRoutine != null) StopCoroutine(growingRoutine);
            growingRoutine = null;
            Level02PlacedStages.Show(placedStages, 0);
            if (highlight) highlight.Show();
        }

        void Update()
        {
            if (!visual || (placedStages != null && placedStages.Length > 0)) return;
            growth = Mathf.MoveTowards(growth, ActivePulse >= 0 ? 1f : 0f, Time.deltaTime * 3f);
            visual.color = Color.Lerp(new Color(.43f,.65f,.49f), new Color(.72f,1f,.70f), growth);
            if (growthFrames != null && growthFrames.Length > 0)
            {
                visual.sprite = growthFrames[Mathf.Min(growthFrames.Length - 1,
                    Mathf.FloorToInt(growth * growthFrames.Length))];
                float width = Mathf.Lerp(.65f, 1.1f, growth);
                visual.transform.localScale = Vector3.one *
                    (width / visual.sprite.bounds.size.x);
            }
        }
    }
}
