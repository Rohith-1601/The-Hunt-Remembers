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

    // Current camera rotation
    private float yaw;
    private float pitch;

    // Target rotation
    private float targetYaw;
    private float targetPitch;

    // SmoothDamp velocities
    private float yawVelocity;
    private float pitchVelocity;

    // Pivot follow velocity
    private Vector3 pivotVelocity;

    // Camera local position smoothing
    private Vector3 cameraLocalVelocity;

    // FOV smoothing
    private float fovVelocity;

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

        // Initial rotation
        yaw = player.eulerAngles.y;
        targetYaw = yaw;

        pitch = 10f;
        targetPitch = pitch;

        // Initial position
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

        targetYaw += mouseX;
        targetPitch -= mouseY;

        targetPitch =
            Mathf.Clamp(
                targetPitch,
                minPitch,
                maxPitch
            );
    }

    // =========================================================
    // ROTATION
    // =========================================================

    private void SmoothRotation()
    {
        yaw =
            Mathf.SmoothDampAngle(
                yaw,
                targetYaw,
                ref yawVelocity,
                yawSmoothTime
            );

        pitch =
            Mathf.SmoothDamp(
                pitch,
                targetPitch,
                ref pitchVelocity,
                pitchSmoothTime
            );

        // IMPORTANT:
        // Keep WORLD rotation.
        transform.rotation =
            Quaternion.Euler(
                pitch,
                yaw,
                0f
            );
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

        transform.position =
            Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref pivotVelocity,
                positionSmoothTime
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

        if (distance <= 0.001f)
            return;

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
                cameraPositionSmoothTime
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

        cameraComponent.fieldOfView =
            Mathf.SmoothDamp(
                cameraComponent.fieldOfView,
                targetFOV,
                ref fovVelocity,
                fovSmoothTime
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

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus &&
            lockCursorOnStart)
        {
            SetCursorState(true);
        }
    }
}