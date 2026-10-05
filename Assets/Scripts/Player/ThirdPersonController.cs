using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraOrbit cameraOrbit;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float crouchSpeed = 1.8f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Crouch")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchingHeight = 1.2f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;

    private CharacterController characterController;
    private float verticalVelocity;

    public float CurrentSpeed { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsCrouching { get; private set; }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (cameraOrbit == null)
        {
            Debug.LogError(
                "ThirdPersonController: CameraOrbit reference is missing.",
                this
            );

            enabled = false;
            return;
        }

        SetStandingState();
    }

    private void Update()
    {
        HandleCrouch();
        HandleMovement();
        HandleGravity();
    }

    private void HandleMovement()
    {
        float horizontal =
            Input.GetAxisRaw("Horizontal");

        float vertical =
            Input.GetAxisRaw("Vertical");

        Vector3 inputDirection =
            new Vector3(
                horizontal,
                0f,
                vertical
            );

        inputDirection =
            Vector3.ClampMagnitude(
                inputDirection,
                1f
            );

        bool hasMovementInput =
            inputDirection.sqrMagnitude > 0.01f;

        // -----------------------------------------------------
        // CAMERA YAW
        // -----------------------------------------------------

        float yaw = cameraOrbit.Yaw;

        // Protect against invalid rotation values.
        if (float.IsNaN(yaw) ||
            float.IsInfinity(yaw))
        {
            Debug.LogWarning(
                "ThirdPersonController: Invalid Camera Yaw detected. " +
                "Movement frame skipped."
            );

            CurrentSpeed = 0f;
            IsSprinting = false;

            return;
        }

        // -----------------------------------------------------
        // CAMERA-RELATIVE MOVEMENT
        // -----------------------------------------------------

        Quaternion cameraRotation =
            Quaternion.Euler(
                0f,
                yaw,
                0f
            );

        Vector3 moveDirection =
            cameraRotation * inputDirection;

        // Protect against invalid or zero vectors.
        if (hasMovementInput &&
            moveDirection.sqrMagnitude > 0.0001f)
        {
            moveDirection.Normalize();
        }

        IsSprinting =
            Input.GetKey(KeyCode.LeftShift) &&
            hasMovementInput &&
            !IsCrouching;

        float targetSpeed;

        if (IsCrouching)
        {
            targetSpeed = crouchSpeed;
        }
        else if (IsSprinting)
        {
            targetSpeed = sprintSpeed;
        }
        else
        {
            targetSpeed = walkSpeed;
        }

        CurrentSpeed =
            hasMovementInput
                ? targetSpeed
                : 0f;

        // -----------------------------------------------------
        // PLAYER ROTATION
        // -----------------------------------------------------

        if (hasMovementInput &&
            moveDirection.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    moveDirection,
                    Vector3.up
                );

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed *
                    Time.deltaTime
                );
        }

        // -----------------------------------------------------
        // HORIZONTAL MOVEMENT
        // -----------------------------------------------------

        Vector3 horizontalVelocity =
            moveDirection *
            targetSpeed;

        characterController.Move(
            horizontalVelocity *
            Time.deltaTime
        );
    }

    private void HandleCrouch()
    {
        bool crouchPressed =
            Input.GetKey(KeyCode.LeftControl) ||
            Input.GetKey(KeyCode.RightControl);

        if (crouchPressed &&
            !IsCrouching)
        {
            SetCrouchingState();
        }
        else if (!crouchPressed &&
                 IsCrouching)
        {
            SetStandingState();
        }
    }

    private void SetCrouchingState()
    {
        IsCrouching = true;

        characterController.height =
            crouchingHeight;

        characterController.center =
            new Vector3(
                0f,
                crouchingHeight * 0.5f,
                0f
            );
    }

    private void SetStandingState()
    {
        IsCrouching = false;

        characterController.height =
            standingHeight;

        characterController.center =
            new Vector3(
                0f,
                standingHeight * 0.5f,
                0f
            );
    }

    private void HandleGravity()
    {
        if (characterController.isGrounded &&
            verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity +=
            gravity *
            Time.deltaTime;

        Vector3 gravityMovement =
            Vector3.up *
            verticalVelocity;

        characterController.Move(
            gravityMovement *
            Time.deltaTime
        );
    }
}