using UnityEngine;

namespace SeedGame
{
    // Aligns the shallow-water collision and recovery mushrooms with the hand-placed art.
    // This also updates an already-open editor scene when Play starts.
    public static class Level02SceneSync
    {
        public static void Install()
        {
            var entranceArt = FindSprite("Ground at entrance");
            var entranceFloor = FindBox("Ground at entrance collision");
            var waterFloor = FindBox("Shallow water basin - safe lower route");
            if (entranceArt && entranceFloor && waterFloor)
            {
                var oldFloor = entranceFloor.bounds;
                SetWorldBox(entranceFloor, oldFloor.min.x, entranceArt.bounds.max.x,
                    oldFloor.min.y, oldFloor.max.y);

                var firstStoneArt = FindSprite("River stone 1");
                if (firstStoneArt)
                {
                    // The small tab at the top of this PNG is above its broad walking lip.
                    float lipInset = firstStoneArt.bounds.size.y * .046f;
                    float lipY = firstStoneArt.bounds.max.y - lipInset;
                    firstStoneArt.transform.position +=
                        Vector3.up * (entranceFloor.bounds.max.y - lipY);
                }

                for (int i = 1; i <= 4; i++)
                {
                    var art = FindSprite("River stone " + i);
                    var stone = FindBox("Stepping stone " + i);
                    if (!art || !stone) continue;

                    float top = i == 1 ? entranceFloor.bounds.max.y - .008f : stone.bounds.max.y;
                    SetWorldBox(stone, art.bounds.min.x + .02f, art.bounds.max.x - .02f,
                        waterFloor.bounds.min.y, top);
                    stone.isTrigger = false;
                    stone.enabled = true;
                }
            }

            var small = FindSprite("First climbing column");
            var middle = FindSprite("Tall climbing column");
            var large = FindSprite("Stone bridge before gate");
            if (small && middle && large)
            {
                MoveRecoveryMushroom("Recovery mushroom between columns 1 and 2",
                    (small.bounds.max.x + middle.bounds.min.x) * .5f);
                MoveRecoveryMushroom("Recovery mushroom between columns 2 and 3",
                    (middle.bounds.max.x + large.bounds.min.x) * .5f);
            }

            var player = Object.FindAnyObjectByType<SeedPlayer>();
            if (player && player.TryGetComponent(out Rigidbody2D body))
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            Physics2D.SyncTransforms();
        }

        static SpriteRenderer FindSprite(string objectName)
        {
            var target = GameObject.Find(objectName);
            return target ? target.GetComponent<SpriteRenderer>() : null;
        }

        static BoxCollider2D FindBox(string objectName)
        {
            var target = GameObject.Find(objectName);
            return target ? target.GetComponent<BoxCollider2D>() : null;
        }

        static void SetWorldBox(BoxCollider2D box, float left, float right, float bottom, float top)
        {
            if (right <= left || top <= bottom) return;
            var worldCenter = new Vector3((left + right) * .5f, (bottom + top) * .5f, 0f);
            var localCenter = box.transform.InverseTransformPoint(worldCenter);
            var scale = box.transform.lossyScale;
            box.offset = new Vector2(localCenter.x, localCenter.y);
            box.size = new Vector2((right - left) / Mathf.Abs(scale.x),
                (top - bottom) / Mathf.Abs(scale.y));
        }

        static void MoveRecoveryMushroom(string objectName, float desiredX)
        {
            var target = GameObject.Find(objectName);
            if (!target || !target.TryGetComponent(out Level02Mushroom mushroom)) return;
            float shift = desiredX - target.transform.position.x;
            if (Mathf.Abs(shift) < .001f) return;

            var delta = Vector3.right * shift;
            target.transform.position += delta;
            if (mushroom.placedStages == null) return;
            foreach (var stage in mushroom.placedStages)
                if (stage && !stage.transform.IsChildOf(target.transform))
                    stage.transform.position += delta;
        }
    }
}
