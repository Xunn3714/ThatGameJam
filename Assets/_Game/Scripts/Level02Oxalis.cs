using UnityEngine;

namespace SeedGame
{
    public sealed class Level02Oxalis : Level02ResonanceTarget
    {
        public SpriteRenderer visual;
        bool opened;

        public override void OnResonance(SeedPlayer player, int pulseSerial)
        {
            if (opened) return;
            opened = true;
            if (visual)
            {
                visual.color = new Color(.74f, 1f, .7f);
                visual.transform.localScale *= 1.35f;
            }
            player.hasGlideLeaf = true;
            Debug.Log("Oxalis glide leaf acquired: hold Space while falling.");
        }
    }
}
