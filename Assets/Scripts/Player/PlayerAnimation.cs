using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ThirdPersonController controller;
    [SerializeField] private Animator animator;

    [Header("Animation Smoothing")]
    [SerializeField] private float speedDampTime = 0.1f;

    // Animator parameter hashes.
    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int CrouchingHash =
        Animator.StringToHash("IsCrouching");

    private void Awake()
    {
        if (controller == null)
        {
            Debug.LogError(
                "PlayerAnimation: ThirdPersonController reference is missing.",
                this
            );

            enabled = false;
            return;
        }

        if (animator == null)
        {
            Debug.LogError(
                "PlayerAnimation: Animator reference is missing.",
                this
            );

            enabled = false;
            return;
        }
    }

    private void Update()
    {
        UpdateCrouch();
        UpdateMovement();
    }

    // =========================================================
    // CROUCH
    // =========================================================

    private void UpdateCrouch()
    {
        animator.SetBool(
            CrouchingHash,
            controller.IsCrouching
        );
    }

    // =========================================================
    // MOVEMENT ANIMATION
    // =========================================================

    private void UpdateMovement()
    {
        float animationSpeed = 0f;

        // No movement input.
        if (controller.CurrentSpeed <= 0.01f)
        {
            animationSpeed = 0f;
        }
        // Crouching movement.
        else if (controller.IsCrouching)
        {
            animationSpeed = 1f;
        }
        // Running.
        else if (controller.IsSprinting)
        {
            animationSpeed = 1f;
        }
        // Normal walking.
        else
        {
            animationSpeed = 0.5f;
        }

        // Smoothly move toward the target animation speed.
        animator.SetFloat(
            SpeedHash,
            animationSpeed,
            speedDampTime,
            Time.deltaTime
        );
    }
}