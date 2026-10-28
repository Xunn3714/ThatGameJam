using UnityEngine;

namespace SeedGame
{
    public sealed class LevelExit : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<SeedPlayer>()) LevelFlow.Instance?.Finish();
        }
    }
}
