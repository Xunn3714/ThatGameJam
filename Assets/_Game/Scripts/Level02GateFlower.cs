using UnityEngine;

namespace SeedGame
{
    public sealed class Level02GateFlower : Level02ResonanceTarget
    {
        public Level02Gate gate;
        public SpriteRenderer visual;
        public int ActivePulse { get; private set; } = -1;
        float growth;
        Vector3 idleScale;

        void Awake()
        {
            if (visual) idleScale = visual.transform.localScale;
        }

        public override void OnResonance(SeedPlayer player, int pulseSerial)
        {
            if (ActivePulse == pulseSerial) return;
            ActivePulse = pulseSerial;
            growth = 0f;
            if (gate) gate.Check(pulseSerial);
        }

        public void ResetForNextPulse()
        {
            if (ActivePulse < 0) return;
            ActivePulse = -1;
            growth = 0f;
        }

        void Update()
        {
            if (!visual) return;
            growth = Mathf.MoveTowards(growth, ActivePulse >= 0 ? 1f : 0f, Time.deltaTime * 3f);
            visual.color = Color.Lerp(new Color(.43f,.65f,.49f), new Color(.72f,1f,.70f), growth);
            visual.transform.localScale = idleScale * (1f + growth * .45f);
        }
    }
}
