using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraOrbit cameraOrbit;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float sprintSpeed = 6f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;

    private CharacterController characterController;
    private float verticalVelocity;

    public float CurrentSpeed { get; private set; }
    public bool IsSprinting { get; private set; }

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        if (cameraOrbit == null)
        {
            Debug.LogError("ThirdPersonController: CameraOrbit reference is missing.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleGravity();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(horizontal, 0f, vertical);

        // Prevent diagonal movement from being faster.
        input = Vector3.ClampMagnitude(input, 1f);

        // Camera-relative movement.
        float yaw = cameraOrbit.Yaw;
        Quaternion cameraRotation = Quaternion.Euler(0f, yaw, 0f);

        Vector3 moveDirection = cameraRotation * input;

        bool hasMovementInput = input.sqrMagnitude > 0.01f;

        IsSprinting =
            Input.GetKey(KeyCode.LeftShift) &&
            hasMovementInput;

        float targetSpeed = IsSprinting ? sprintSpeed : walkSpeed;

        CurrentSpeed = hasMovementInput ? targetSpeed : 0f;

        if (hasMovementInput)
        {
            // Rotate the player toward movement direction.
            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        Vector3 horizontalVelocity = moveDirection * targetSpeed;

        characterController.Move(horizontalVelocity * Time.deltaTime);
    }

    private void HandleGravity()
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 gravityMovement =
            Vector3.up * verticalVelocity;

        characterController.Move(
            gravityMovement * Time.deltaTime
        );
    }
}