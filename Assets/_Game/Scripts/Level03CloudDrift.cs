using UnityEngine;

namespace SeedGame
{
    public sealed class Level03CloudDrift : MonoBehaviour
    {
        public float speed = .045f;
        public float repeatWidth = 20.405f;
        Transform[] clouds;
        Vector3[] authoredPositions;
        float shift;

        void Awake()
        {
            clouds = new Transform[6];
            authoredPositions = new Vector3[clouds.Length];
            for (int i = 0; i < 3; i++)
            {
                var faded = GameObject.Find("L03 Faded Cloud " + (i + 1));
                var reborn = GameObject.Find("L03 Reborn Cloud " + (i + 1));
                if (faded) clouds[i * 2] = faded.transform;
                if (reborn) clouds[i * 2 + 1] = reborn.transform;
            }
            for (int i = 0; i < clouds.Length; i++)
                if (clouds[i]) authoredPositions[i] = clouds[i].position;
        }

        void Update()
        {
            shift = Mathf.Repeat(shift + speed * Time.deltaTime, repeatWidth);
            for (int i = 0; i < clouds.Length; i++)
                if (clouds[i])
                    clouds[i].position = authoredPositions[i] + Vector3.right * shift;
        }
    }
}
