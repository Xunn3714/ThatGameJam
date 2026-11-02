using UnityEngine;

namespace SeedGame
{
    public sealed class SeedCamera : MonoBehaviour
    {
        public Transform target;
        public float minX = 0, maxX = 47, minY = -2, maxY = 11;
        public float deadZoneX = 1.1f, deadZoneY = .65f;
        public float followTime = .28f;
        public float playSize = 5f, introSize = 2.7f, introSeconds = 2.7f;
        Camera cam;
        Vector3 velocity;
        float elapsed;
        float gearFocus;
        public void SetGearFocus(float amount) { gearFocus = Mathf.Clamp01(amount); }
        void Awake() { cam = GetComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = introSize; }
        void LateUpdate()
        {
            if (!target) return;
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, Mathf.Clamp01(elapsed / introSeconds));
            cam.orthographicSize = Mathf.Lerp(Mathf.Lerp(introSize, playSize, t), 2.65f, gearFocus);
            float desiredX = transform.position.x;
            float desiredY = transform.position.y;
            if (target.position.x > desiredX + deadZoneX) desiredX = target.position.x - deadZoneX;
            if (target.position.x < desiredX - deadZoneX) desiredX = target.position.x + deadZoneX;
            if (target.position.y > desiredY + deadZoneY) desiredY = target.position.y - deadZoneY;
            if (target.position.y < desiredY - deadZoneY) desiredY = target.position.y + deadZoneY;
            float halfWidth = cam.orthographicSize * cam.aspect;
            desiredX = Mathf.Lerp(target.position.x,
                Mathf.Clamp(desiredX, minX + halfWidth, maxX - halfWidth), t);
            desiredY = Mathf.Clamp(desiredY, minY + cam.orthographicSize, maxY - cam.orthographicSize);
            desiredX = Mathf.Lerp(desiredX, target.position.x, gearFocus);
            desiredY = Mathf.Lerp(desiredY, target.position.y + .3f, gearFocus);
            transform.position = Vector3.SmoothDamp(transform.position,
                new Vector3(desiredX, desiredY, transform.position.z), ref velocity, followTime);
        }
    }
}
