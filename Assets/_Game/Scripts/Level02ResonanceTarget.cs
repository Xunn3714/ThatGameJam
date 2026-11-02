using UnityEngine;

namespace SeedGame
{
    public abstract class Level02ResonanceTarget : MonoBehaviour
    {
        public abstract void OnResonance(SeedPlayer player, int pulseSerial);
    }
}
