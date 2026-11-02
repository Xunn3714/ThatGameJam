using UnityEngine;

namespace SeedGame
{
    // Place this on a decorative layer root, never on collision geometry.
    [DefaultExecutionOrder(100)]
    public sealed class ParallaxLayer : MonoBehaviour
    {
        public Transform cameraTarget;
        [Range(0f, 2f)] public float horizontalScreenSpeed = .5f;
        [Range(0f, 2f)] public float verticalScreenSpeed = .5f;

        Vector3 initialCameraPosition;
        Vector3 initialPosition;

        void OnEnable()
        {
            if (!cameraTarget && Camera.main) cameraTarget = Camera.main.transform;
            initialCameraPosition = cameraTarget ? cameraTarget.position : Vector3.zero;
            initialPosition = transform.position;
        }

        void LateUpdate()
        {
            if (!cameraTarget) return;
            Vector3 delta = cameraTarget.position - initialCameraPosition;
            transform.position = initialPosition + new Vector3(
                delta.x * (1f - horizontalScreenSpeed),
                delta.y * (1f - verticalScreenSpeed), 0f);
        }
    }
}
