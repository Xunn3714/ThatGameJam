using UnityEngine;

namespace SeedGame
{
    public sealed class Checkpoint : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<SeedPlayer>();
            if (player) player.SetCheckpoint(transform);
        }
    }
}
