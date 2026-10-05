using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private ThirdPersonController controller;
    [SerializeField] private Transform cameraTransform;

    [Header("Look")]
    [SerializeField] private float mouseSensitivity = 3f;

    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 60f;

    [Header("Look Smoothing")]
    [SerializeField] private float yawSmoothTime = 0.05f;
    [SerializeField] private float pitchSmoothTime = 0.05f;

    [Header("Pivot Follow")]
    [SerializeField] private float standingHeight = 1.6f;
    [SerializeField] private float crouchingHeight = 1.0f;
    [SerializeField] private float positionSmoothTime = 0.06f;

    [Header("Camera Position")]
    [SerializeField] private float normalDistance = 4.8f;
    [SerializeField] private float crouchDistance = 4.3f;
    [SerializeField] private float shoulderOffset = 0.55f;

    [Header("Camera Collision")]
    [SerializeField] private float collisionRadius = 0.2f;
    [SerializeField] private float minimumCameraDistance = 1f;
    [SerializeField] private LayerMask collisionMask = ~0;

    [Header("Camera Position Smoothing")]
    [SerializeField] private float cameraPositionSmoothTime = 0.06f;

    [Header("FOV")]
    [SerializeField] private float normalFOV = 65f;
    [SerializeField] private float sprintFOV = 70f;
    [SerializeField] private float crouchFOV = 60f;
    [SerializeField] private float fovSmoothTime = 0.08f;

    [Header("Cursor")]
    [SerializeField] private bool lockCursorOnStart = true;

    private float yaw;
    private float pitch;

    private float targetYaw;
    private float targetPitch;

    private float yawVelocity;
    private float pitchVelocity;

    private Vector3 pivotVelocity;
    private Vector3 cameraLocalVelocity;

    private float fovVelocity;

    private bool reportedInvalidRotation;

    // IMPORTANT:
    // ThirdPersonController uses this for camera-relative movement.
    public float Yaw => yaw;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError(
                "CameraOrbit: Player reference is missing.",
                this
            );

            enabled = false;
            return;
        }

        if (cameraTransform == null)
        {
            Debug.LogError(
                "CameraOrbit: Camera Transform reference is missing.",
                this
            );

            enabled = false;
            return;
        }

        float startingYaw = player.eulerAngles.y;

        if (!IsFinite(startingYaw))
        {
            startingYaw = 0f;
        }

        startingYaw = NormalizeYaw(startingYaw);

        yaw = startingYaw;
        targetYaw = startingYaw;

        pitch = 10f;
        targetPitch = 10f;

        transform.position =
            player.position +
            Vector3.up * standingHeight;

        transform.rotation =
            Quaternion.Euler(
                pitch,
                yaw,
                0f
            );

        SetCursorState(lockCursorOnStart);
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        HandleMouseLook();
        SmoothRotation();
        SmoothPivotFollow();
        HandleCameraPosition();
        HandleFOV();
    }

    // =========================================================
    // MOUSE LOOK
    // =========================================================

    private void HandleMouseLook()
    {
        float mouseX =
            Input.GetAxis("Mouse X") *
            mouseSensitivity;

        float mouseY =
            Input.GetAxis("Mouse Y") *
            mouseSensitivity;

        // Protect against invalid input values.
        if (!IsFinite(mouseX))
            mouseX = 0f;

        if (!IsFinite(mouseY))
            mouseY = 0f;

        targetYaw += mouseX;
        targetPitch -= mouseY;

        // Keep yaw inside a manageable range.
        targetYaw =
            NormalizeYaw(targetYaw);

        // Clamp pitch.
        targetPitch =
            Mathf.Clamp(
                targetPitch,
                minPitch,
                maxPitch
            );

        // Final safety check.
        if (!IsFinite(targetYaw))
        {
            targetYaw = yaw;
        }

        if (!IsFinite(targetPitch))
        {
            targetPitch = pitch;
        }
    }

    // =========================================================
    // ROTATION
    // =========================================================

    private void SmoothRotation()
    {
        // Safety before SmoothDamp.
        if (!IsFinite(yaw))
        {
            yaw = 0f;
            yawVelocity = 0f;
        }

        if (!IsFinite(targetYaw))
        {
            targetYaw = yaw;
        }

        if (!IsFinite(pitch))
        {
            pitch = 10f;
            pitchVelocity = 0f;
        }

        if (!IsFinite(targetPitch))
        {
            targetPitch = pitch;
        }

        targetYaw =
            NormalizeYaw(targetYaw);

        targetPitch =
            Mathf.Clamp(
                targetPitch,
                minPitch,
                maxPitch
            );

        yaw =
            Mathf.SmoothDampAngle(
                yaw,
                targetYaw,
                ref yawVelocity,
                Mathf.Max(
                    yawSmoothTime,
                    0.0001f
                )
            );

        pitch =
            Mathf.SmoothDamp(
                pitch,
                targetPitch,
                ref pitchVelocity,
                Mathf.Max(
                    pitchSmoothTime,
                    0.0001f
                )
            );

        // Final safety.
        if (!IsFinite(yaw))
        {
            yaw = targetYaw;
            yawVelocity = 0f;
        }

        if (!IsFinite(pitch))
        {
            pitch = targetPitch;
            pitchVelocity = 0f;
        }

        yaw =
            NormalizeYaw(yaw);

        pitch =
            Mathf.Clamp(
                pitch,
                minPitch,
                maxPitch
            );

        // IMPORTANT:
        // Keep WORLD rotation.
        Quaternion targetRotation =
            Quaternion.Euler(
                pitch,
                yaw,
                0f
            );

        transform.rotation =
            targetRotation;

        reportedInvalidRotation = false;
    }

    // =========================================================
    // PIVOT FOLLOW
    // =========================================================

    private void SmoothPivotFollow()
    {
        float targetHeight =
            standingHeight;

        if (controller != null &&
            controller.IsCrouching)
        {
            targetHeight =
                crouchingHeight;
        }

        Vector3 targetPosition =
            player.position +
            Vector3.up * targetHeight;

        if (!IsFiniteVector3(targetPosition))
        {
            return;
        }

        transform.position =
            Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref pivotVelocity,
                Mathf.Max(
                    positionSmoothTime,
                    0.0001f
                )
            );
    }

    // =========================================================
    // CAMERA POSITION
    // =========================================================

    private void HandleCameraPosition()
    {
        float targetDistance =
            normalDistance;

        if (controller != null &&
            controller.IsCrouching)
        {
            targetDistance =
                crouchDistance;
        }

        Vector3 desiredLocalPosition =
            new Vector3(
                shoulderOffset,
                0f,
                -targetDistance
            );

        Vector3 desiredWorldPosition =
            transform.TransformPoint(
                desiredLocalPosition
            );

        Vector3 pivotPosition =
            transform.position;

        Vector3 direction =
            desiredWorldPosition -
            pivotPosition;

        float distance =
            direction.magnitude;

        if (!IsFinite(distance) ||
            distance <= 0.001f)
        {
            return;
        }

        direction.Normalize();

        float finalDistance =
            distance;

        if (Physics.SphereCast(
                pivotPosition,
                collisionRadius,
                direction,
                out RaycastHit hit,
                distance,
                collisionMask,
                QueryTriggerInteraction.Ignore))
        {
            if (hit.collider.transform != player &&
                !hit.collider.transform.IsChildOf(player))
            {
                finalDistance =
                    Mathf.Max(
                        minimumCameraDistance,
                        hit.distance -
                        collisionRadius
                    );
            }
        }

        if (!IsFinite(finalDistance))
        {
            finalDistance =
                targetDistance;
        }

        Vector3 targetCameraLocalPosition =
            new Vector3(
                shoulderOffset,
                0f,
                -finalDistance
            );

        cameraTransform.localPosition =
            Vector3.SmoothDamp(
                cameraTransform.localPosition,
                targetCameraLocalPosition,
                ref cameraLocalVelocity,
                Mathf.Max(
                    cameraPositionSmoothTime,
                    0.0001f
                )
            );
    }

    // =========================================================
    // FOV
    // =========================================================

    private void HandleFOV()
    {
        Camera cameraComponent =
            cameraTransform.GetComponent<Camera>();

        if (cameraComponent == null)
            return;

        float targetFOV =
            normalFOV;

        if (controller != null &&
            controller.IsCrouching)
        {
            targetFOV =
                crouchFOV;
        }
        else if (controller != null &&
                 controller.IsSprinting)
        {
            targetFOV =
                sprintFOV;
        }

        if (!IsFinite(
                cameraComponent.fieldOfView))
        {
            cameraComponent.fieldOfView =
                normalFOV;

            fovVelocity = 0f;
        }

        cameraComponent.fieldOfView =
            Mathf.SmoothDamp(
                cameraComponent.fieldOfView,
                targetFOV,
                ref fovVelocity,
                Mathf.Max(
                    fovSmoothTime,
                    0.0001f
                )
            );
    }

    // =========================================================
    // CURSOR
    // =========================================================

    public void SetCursorState(bool locked)
    {
        Cursor.lockState =
            locked
                ? CursorLockMode.Locked
                : CursorLockMode.None;

        Cursor.visible =
            !locked;
    }

    private void OnApplicationFocus(
        bool hasFocus)
    {
        if (hasFocus &&
            lockCursorOnStart)
        {
            SetCursorState(true);
        }
    }

    // =========================================================
    // SAFETY HELPERS
    // =========================================================

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) &&
               !float.IsInfinity(value);
    }

    private static bool IsFiniteVector3(
        Vector3 value)
    {
        return IsFinite(value.x) &&
               IsFinite(value.y) &&
               IsFinite(value.z);
    }

    private static float NormalizeYaw(
        float value)
    {
        if (!IsFinite(value))
            return 0f;

        value %= 360f;

        if (value > 180f)
            value -= 360f;

        if (value < -180f)
            value += 360f;

        return value;
    }
}