using UnityEngine;

namespace SeedGame
{
    // Stores the installer's baseline so Scene-view edits survive later art installs.
    public sealed class ManualArtPlacement : MonoBehaviour
    {
        public Vector3 generatedPosition;
        public Vector3 generatedScale;
        public Quaternion generatedRotation;

        public bool WasMoved =>
            Vector3.Distance(transform.position, generatedPosition) > .001f ||
            Vector3.Distance(transform.localScale, generatedScale) > .001f ||
            Quaternion.Angle(transform.rotation, generatedRotation) > .01f;

        public void RecordGenerated()
        {
            generatedPosition = transform.position;
            generatedScale = transform.localScale;
            generatedRotation = transform.rotation;
        }
    }
}
