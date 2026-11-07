using UnityEngine;

namespace SeedGame
{
    public sealed class SeedCamera : MonoBehaviour
    {
        public Transform target;
        public float minX = 0, maxX = 47, minY = -2, maxY = 11;
        public float deadZoneX = 1.1f, deadZoneY = .65f;
        public float followTime = .28f;
        public bool playIntro = true;
        public float playSize = 5f, introSize = 2.7f, introSeconds = 2.7f;
        Camera cam;
        Vector3 velocity;
        float elapsed;
        float gearFocus;
        bool snapOnFirstFrame;
        bool fixedView;
        bool returningFromFixedView;
        Vector2 fixedViewMin;
        Vector2 fixedViewMax;
        float fixedViewPadding;
        float sizeVelocity;
        public void SetGearFocus(float amount) { gearFocus = Mathf.Clamp01(amount); }
        public void ShowFixedView(Vector2 min, Vector2 max, float padding = .4f)
        {
            fixedViewMin = min;
            fixedViewMax = max;
            fixedViewPadding = Mathf.Max(0f, padding);
            fixedView = true;
            returningFromFixedView = false;
            sizeVelocity = 0f;
        }

        public void MoveFixedView(Vector2 min, Vector2 max, float padding)
        {
            if (!fixedView) return;
            fixedViewMin = min;
            fixedViewMax = max;
            fixedViewPadding = Mathf.Max(0f, padding);
        }

        public void ClearFixedView()
        {
            if (!fixedView) return;
            fixedView = false;
            returningFromFixedView = true;
            sizeVelocity = 0f;
        }

        void Awake()
        {
            cam = GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = playIntro ? introSize : playSize;
            snapOnFirstFrame = !playIntro;
        }
        void LateUpdate()
        {
            if (!target) return;
            if (playIntro) elapsed += Time.deltaTime;
            float t = playIntro
                ? Mathf.SmoothStep(0, 1, Mathf.Clamp01(elapsed / introSeconds))
                : 1f;
            float followSize = Mathf.Lerp(Mathf.Lerp(introSize, playSize, t), 2.65f, gearFocus);
            if (fixedView)
            {
                float frameHeight = (fixedViewMax.y - fixedViewMin.y) * .5f;
                float frameWidth = (fixedViewMax.x - fixedViewMin.x) * .5f / Mathf.Max(.01f, cam.aspect);
                float frameSize = Mathf.Max(frameHeight, frameWidth) + fixedViewPadding;
                cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize,
                    frameSize, ref sizeVelocity, .65f);
                Vector2 center = (fixedViewMin + fixedViewMax) * .5f;
                Vector3 position = new Vector3(center.x, center.y, transform.position.z);
                transform.position = Vector3.SmoothDamp(transform.position,
                    position, ref velocity, .7f);
                return;
            }
            if (returningFromFixedView)
            {
                cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize,
                    followSize, ref sizeVelocity, .45f);
                if (Mathf.Abs(cam.orthographicSize - followSize) < .02f)
                {
                    cam.orthographicSize = followSize;
                    returningFromFixedView = false;
                    sizeVelocity = 0f;
                }
            }
            else cam.orthographicSize = followSize;
            float desiredX = transform.position.x;
            float desiredY = transform.position.y;
            if (target.position.x > desiredX + deadZoneX) desiredX = target.position.x - deadZoneX;
            if (target.position.x < desiredX - deadZoneX) desiredX = target.position.x + deadZoneX;
            if (target.position.y > desiredY + deadZoneY) desiredY = target.position.y - deadZoneY;
            if (target.position.y < desiredY - deadZoneY) desiredY = target.position.y + deadZoneY;
            float halfWidth = cam.orthographicSize * cam.aspect;
            desiredX = Mathf.Lerp(target.position.x,
                Mathf.Clamp(desiredX, minX + halfWidth, maxX - halfWidth), t);
            float cameraMinY = minY + cam.orthographicSize;
            float cameraMaxY = maxY - cam.orthographicSize;
            desiredY = cameraMinY <= cameraMaxY
                ? Mathf.Clamp(desiredY, cameraMinY, cameraMaxY)
                : (minY + maxY) * .5f;
            desiredX = Mathf.Lerp(desiredX, target.position.x, gearFocus);
            desiredY = Mathf.Lerp(desiredY, target.position.y + .3f, gearFocus);
            // Gear close-ups must obey the same world limits as ordinary follow.
            desiredX = Mathf.Clamp(desiredX, minX + halfWidth, maxX - halfWidth);
            if (cameraMinY <= cameraMaxY)
                desiredY = Mathf.Clamp(desiredY, cameraMinY, cameraMaxY);
            var desiredPosition = new Vector3(desiredX, desiredY, transform.position.z);
            if (snapOnFirstFrame)
            {
                transform.position = desiredPosition;
                velocity = Vector3.zero;
                snapOnFirstFrame = false;
            }
            else
                transform.position = Vector3.SmoothDamp(transform.position,
                    desiredPosition, ref velocity, followTime);
        }
    }
}
