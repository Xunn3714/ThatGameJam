using UnityEngine;

namespace SeedGame
{
    public sealed class Level02Algae : Level02ResonanceTarget
    {
        public SpriteRenderer darkness;
        public SpriteRenderer[] glowMarks;
        bool lit;
        float progress;

        public override void OnResonance(SeedPlayer player, int pulseSerial)
        {
            lit = true;
        }

        void Update()
        {
            if (!lit) return;
            progress = Mathf.MoveTowards(progress, 1f, Time.deltaTime / 1.2f);
            if (darkness)
            {
                var color = darkness.color;
                color.a = Mathf.Lerp(.42f, .06f, progress);
                darkness.color = color;
            }
            if (glowMarks != null)
                foreach (var mark in glowMarks)
                    if (mark) mark.color = Color.Lerp(new Color(.23f,.44f,.42f),
                        new Color(.50f,1f,.87f), progress);
        }
    }
}
