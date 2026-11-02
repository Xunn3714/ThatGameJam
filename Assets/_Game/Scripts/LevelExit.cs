using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeedGame
{
    public sealed class LevelExit : MonoBehaviour
    {
        public string destinationScene;
        public int requiredGears;
        public Level02Gate requiredGate;
        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.GetComponent<SeedPlayer>()) return;
            if (GearState.Count < requiredGears)
            {
                Debug.Log("Collect the gear before entering the ruins.");
                return;
            }
            if (requiredGate && !requiredGate.IsOpen) return;
            if (!string.IsNullOrEmpty(destinationScene)) SceneManager.LoadScene(destinationScene);
            else LevelFlow.Instance?.Finish();
        }
    }
}
