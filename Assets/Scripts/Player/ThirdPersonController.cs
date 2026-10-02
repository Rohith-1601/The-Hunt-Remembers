using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraOrbit cameraOrbit;
    [SerializeField] private Transform cameraPivot;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float crouchSpeed = 1.8f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Crouch")]
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchingHeight = 1.2f;
    [SerializeField] private float standingCameraHeight = 1.6f;
    [SerializeField] private float crouchingCameraHeight = 1f;

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

        if (cameraPivot == null)
        {
            Debug.LogError(
                "ThirdPersonController: Camera Pivot reference is missing.",
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

    // =========================================================
    // MOVEMENT
    // =========================================================

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 inputDirection =
            new Vector3(horizontal, 0f, vertical);

        // Prevent faster diagonal movement.
        inputDirection =
            Vector3.ClampMagnitude(inputDirection, 1f);

        bool hasMovementInput =
            inputDirection.sqrMagnitude > 0.01f;

        // Preserve our camera-relative movement system.
        float yaw = cameraOrbit.Yaw;

        Quaternion cameraRotation =
            Quaternion.Euler(0f, yaw, 0f);

        Vector3 moveDirection =
            cameraRotation * inputDirection;

        // Sprint only while standing.
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
            hasMovementInput ? targetSpeed : 0f;

        // Rotate toward movement direction.
        if (hasMovementInput)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
        }

        // Move horizontally.
        Vector3 horizontalVelocity =
            moveDirection * targetSpeed;

        characterController.Move(
            horizontalVelocity * Time.deltaTime
        );
    }

    // =========================================================
    // CROUCH
    // =========================================================

    private void HandleCrouch()
    {
        bool crouchPressed =
            Input.GetKey(KeyCode.LeftControl) ||
            Input.GetKey(KeyCode.RightControl);

        if (crouchPressed && !IsCrouching)
        {
            SetCrouchingState();

            Debug.Log("CROUCH ON");
        }
        else if (!crouchPressed && IsCrouching)
        {
            SetStandingState();

            Debug.Log("CROUCH OFF");
        }
    }

    private void SetCrouchingState()
    {
        IsCrouching = true;

        // Shrink the CharacterController only.
        characterController.height =
            crouchingHeight;

        characterController.center =
            new Vector3(
                0f,
                crouchingHeight * 0.5f,
                0f
            );

        // Lower camera.
        Vector3 cameraPosition =
            cameraPivot.localPosition;

        cameraPosition.y =
            crouchingCameraHeight;

        cameraPivot.localPosition =
            cameraPosition;
    }

    private void SetStandingState()
    {
        IsCrouching = false;

        // Restore CharacterController.
        characterController.height =
            standingHeight;

        characterController.center =
            new Vector3(
                0f,
                standingHeight * 0.5f,
                0f
            );

        // Restore camera.
        Vector3 cameraPosition =
            cameraPivot.localPosition;

        cameraPosition.y =
            standingCameraHeight;

        cameraPivot.localPosition =
            cameraPosition;
    }

    // =========================================================
    // GRAVITY
    // =========================================================

    private void HandleGravity()
    {
        if (characterController.isGrounded &&
            verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity +=
            gravity * Time.deltaTime;

        Vector3 gravityMovement =
            Vector3.up * verticalVelocity;

        characterController.Move(
            gravityMovement * Time.deltaTime
        );
    }
}