using System.Collections;
using UnityEngine;

public class PlayerVault : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private ThirdPersonController movementController;
    [SerializeField] private Animator animator;

    [Header("Vault Detection")]
    [SerializeField] private float detectionDistance = 1.5f;
    [SerializeField] private float detectionHeight = 0.8f;
    [SerializeField] private float minObstacleHeight = 0.4f;
    [SerializeField] private float maxObstacleHeight = 1.2f;
    [SerializeField] private LayerMask obstacleMask = ~0;

    [Header("Vault Movement")]
    [SerializeField] private float vaultDuration = 0.45f;
    [SerializeField] private float vaultArcHeight = 0.8f;
    [SerializeField] private float landingDistance = 1.2f;

    private bool isVaulting;

    private static readonly int VaultHash =
        Animator.StringToHash("Vault");

    private void Awake()
    {
        if (characterController == null)
        {
            characterController =
                GetComponent<CharacterController>();
        }

        if (movementController == null)
        {
            movementController =
                GetComponent<ThirdPersonController>();
        }

        if (animator == null)
        {
            Debug.LogError(
                "PlayerVault: Animator reference is missing.",
                this
            );

            enabled = false;
            return;
        }
    }

    private void Update()
    {
        if (isVaulting)
            return;

        // Don't vault while crouching.
        if (movementController.IsCrouching)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryVault();
        }
    }

    private void TryVault()
    {
        Vector3 origin =
            transform.position +
            Vector3.up * detectionHeight;

        Vector3 direction =
            transform.forward;

        if (!Physics.Raycast(
                origin,
                direction,
                out RaycastHit hit,
                detectionDistance,
                obstacleMask,
                QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Vault: No obstacle detected.");
            return;
        }

        Collider obstacle =
            hit.collider;

        Bounds bounds =
            obstacle.bounds;

        float obstacleHeight =
            bounds.max.y -
            transform.position.y;

        Debug.Log(
            $"Vault: Detected {obstacle.name}, " +
            $"Height = {obstacleHeight:F2}"
        );

        if (obstacleHeight < minObstacleHeight ||
            obstacleHeight > maxObstacleHeight)
        {
            Debug.Log(
                $"Vault: Obstacle height " +
                $"{obstacleHeight:F2} is outside the allowed range."
            );

            return;
        }

        Vector3 landingPosition =
            transform.position +
            transform.forward *
            landingDistance;

        landingPosition.y =
            transform.position.y;

        StartCoroutine(
            PerformVault(landingPosition)
        );
    }

    private IEnumerator PerformVault(
        Vector3 landingPosition)
    {
        isVaulting = true;

        // Tell Animator to play the vault animation.
        animator.SetTrigger(VaultHash);

        // Disable normal movement during vault.
        movementController.enabled = false;

        Vector3 startPosition =
            transform.position;

        Vector3 middlePosition =
            Vector3.Lerp(
                startPosition,
                landingPosition,
                0.5f
            );

        middlePosition.y += vaultArcHeight;

        float elapsed = 0f;

        while (elapsed < vaultDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / vaultDuration
                );

            // Smooth movement.
            float smoothT =
                t * t * (3f - 2f * t);

            Vector3 vaultPosition =
                QuadraticBezier(
                    startPosition,
                    middlePosition,
                    landingPosition,
                    smoothT
                );

            characterController.enabled = false;

            transform.position =
                vaultPosition;

            characterController.enabled = true;

            yield return null;
        }

        characterController.enabled = false;

        transform.position =
            landingPosition;

        characterController.enabled = true;

        movementController.enabled = true;

        isVaulting = false;
    }

    private Vector3 QuadraticBezier(
        Vector3 start,
        Vector3 middle,
        Vector3 end,
        float t)
    {
        float oneMinusT =
            1f - t;

        return
            oneMinusT * oneMinusT * start +
            2f * oneMinusT * t * middle +
            t * t * end;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 origin =
            transform.position +
            Vector3.up * detectionHeight;

        Gizmos.DrawLine(
            origin,
            origin +
            transform.forward *
            detectionDistance
        );
    }
}