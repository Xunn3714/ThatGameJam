using System.Collections;
using UnityEngine;

namespace SeedGame
{
    public sealed class Level02Oxalis : Level02ResonanceTarget
    {
        public SpriteRenderer visual;
        public Sprite[] growthFrames;
        public SpriteRenderer[] placedStages;
        InteractableHighlight highlight;
        bool opened;

        void Awake() => Level02PlacedStages.Show(placedStages, 0);

        void Start() => highlight = Level02PlacedStages.HighlightFirst(placedStages);

        public override void OnResonance(SeedPlayer player, int pulseSerial)
        {
            if (opened) return;
            opened = true;
            GameAudio.PlayPlantGrowth();
            if (highlight) highlight.Hide();
            if (placedStages != null && placedStages.Length > 1) StartCoroutine(GrowPlaced());
            if (visual)
            {
                visual.color = new Color(.74f, 1f, .7f);
                if ((placedStages == null || placedStages.Length == 0) &&
                    growthFrames != null && growthFrames.Length > 0)
                    StartCoroutine(Grow());
            }
            player.hasGlideLeaf = true;
            Debug.Log("Oxalis glide leaf acquired: hold Space while falling.");
        }

        IEnumerator Grow()
        {
            for (int i = 0; i < growthFrames.Length; i++)
            {
                if (growthFrames[i])
                {
                    visual.sprite = growthFrames[i];
                    float width = Mathf.Lerp(.6f, 1.25f,
                        (i + 1f) / growthFrames.Length);
                    visual.transform.localScale = Vector3.one *
                        (width / visual.sprite.bounds.size.x);
                }
                yield return new WaitForSeconds(.13f);
            }
        }

        IEnumerator GrowPlaced()
        {
            for (int i = 1; i < placedStages.Length; i++)
            {
                yield return new WaitForSeconds(.2f);
                Level02PlacedStages.Show(placedStages, i);
            }
        }
    }
}
