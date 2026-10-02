using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerLeap : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private ThirdPersonController movementController;
    [SerializeField] private Animator animator;

    [Header("Obstacle Detection")]
    [SerializeField] private float detectionDistance = 1.5f;
    [SerializeField] private float detectionHeight = 0.8f;
    [SerializeField] private float minimumTakeoffDistance = 0.35f;

    [SerializeField] private float minObstacleHeight = 0.4f;
    [SerializeField] private float maxObstacleHeight = 1.2f;

    [SerializeField] private LayerMask obstacleMask;

    [Header("Landing Detection")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float landingForwardOffset = 0.35f;
    [SerializeField] private float groundRayStartHeight = 2f;
    [SerializeField] private float groundRayDistance = 5f;

    [Header("Leap Movement")]
    [SerializeField] private float leapDuration = 0.6f;
    [SerializeField] private float leapArcHeight = 0.9f;

    [Header("Animation")]
    [SerializeField] private string leapTriggerName = "Leap";

    private bool isLeaping;
    private int leapTriggerHash;

    public bool IsLeaping => isLeaping;

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
                "PlayerLeap: Animator reference is missing.",
                this
            );

            enabled = false;
            return;
        }

        leapTriggerHash =
            Animator.StringToHash(leapTriggerName);
    }

    private void Update()
    {
        if (isLeaping)
            return;

        // No leap while crouching.
        if (movementController.IsCrouching)
            return;

        // Require movement input.
        if (movementController.CurrentSpeed <= 0.01f)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryLeap();
        }
    }

    private void TryLeap()
    {
        Vector3 forward =
            Vector3.ProjectOnPlane(
                transform.forward,
                Vector3.up
            ).normalized;

        Vector3 detectionOrigin =
            transform.position +
            Vector3.up * detectionHeight;

        // =====================================================
        // 1. DETECT OBSTACLE
        // =====================================================

        if (!Physics.Raycast(
                detectionOrigin,
                forward,
                out RaycastHit obstacleHit,
                detectionDistance,
                obstacleMask,
                QueryTriggerInteraction.Ignore))
        {
            Debug.Log("Leap: No obstacle detected.");
            return;
        }

        // Ignore player's own colliders.
        if (obstacleHit.collider.transform == transform ||
            obstacleHit.collider.transform.IsChildOf(transform))
        {
            return;
        }

        // =====================================================
        // 2. CHECK TAKEOFF DISTANCE
        // =====================================================

        if (obstacleHit.distance < minimumTakeoffDistance)
        {
            Debug.Log("Leap: Too close to obstacle.");
            return;
        }

        // =====================================================
        // 3. CHECK OBSTACLE HEIGHT
        // =====================================================

        Bounds obstacleBounds =
            obstacleHit.collider.bounds;

        float obstacleHeight =
            obstacleBounds.max.y -
            transform.position.y;

        Debug.Log(
            $"Leap: Obstacle = {obstacleHit.collider.name}, " +
            $"Height = {obstacleHeight:F2}"
        );

        if (obstacleHeight < minObstacleHeight ||
            obstacleHeight > maxObstacleHeight)
        {
            Debug.Log(
                $"Leap: Height {obstacleHeight:F2} " +
                $"is outside the allowed range."
            );

            return;
        }

        // =====================================================
        // 4. FIND FAR SIDE OF OBSTACLE
        // =====================================================

        Vector3 obstacleCenter =
            obstacleBounds.center;

        float projectedHalfDepth =
            Mathf.Abs(forward.x) * obstacleBounds.extents.x +
            Mathf.Abs(forward.z) * obstacleBounds.extents.z;

        Vector3 farEdge =
            obstacleCenter +
            forward * projectedHalfDepth;

        // Add enough room for the player's CharacterController.
        float safeDistance =
            characterController.radius +
            landingForwardOffset;

        Vector3 landingProbe =
            farEdge +
            forward * safeDistance;

        // =====================================================
        // 5. FIND ACTUAL FLOOR
        // =====================================================

        Vector3 groundRayOrigin =
            landingProbe +
            Vector3.up * groundRayStartHeight;

        if (!Physics.Raycast(
                groundRayOrigin,
                Vector3.down,
                out RaycastHit groundHit,
                groundRayDistance,
                groundMask,
                QueryTriggerInteraction.Ignore))
        {
            Debug.Log(
                "Leap: No ground found at landing location."
            );

            return;
        }

        // CharacterController position represents the
        // bottom/feet level because center = 1 and height = 2.
        Vector3 landingPosition =
            new Vector3(
                landingProbe.x,
                groundHit.point.y,
                landingProbe.z
            );

        Debug.Log(
            $"Leap: Landing position = {landingPosition}"
        );

        // =====================================================
        // 6. CHECK LANDING SPACE
        // =====================================================

        if (!CanFitAtLandingPosition(landingPosition))
        {
            Debug.Log(
                "Leap: Landing position is blocked."
            );

            return;
        }

        // =====================================================
        // 7. START LEAP
        // =====================================================

        StartCoroutine(
            PerformLeap(landingPosition)
        );
    }

    private bool CanFitAtLandingPosition(
        Vector3 landingPosition)
    {
        Vector3 capsuleCenter =
            landingPosition +
            characterController.center;

        float halfHeight =
            Mathf.Max(
                0f,
                characterController.height * 0.5f -
                characterController.radius
            );

        Vector3 bottom =
            capsuleCenter -
            Vector3.up * halfHeight;

        Vector3 top =
            capsuleCenter +
            Vector3.up * halfHeight;

        Collider[] overlaps =
            Physics.OverlapCapsule(
                bottom,
                top,
                characterController.radius,
                obstacleMask,
                QueryTriggerInteraction.Ignore
            );

        foreach (Collider collider in overlaps)
        {
            if (collider == characterController)
                continue;

            if (collider.transform == transform ||
                collider.transform.IsChildOf(transform))
            {
                continue;
            }

            return false;
        }

        return true;
    }

    private IEnumerator PerformLeap(
        Vector3 landingPosition)
    {
        isLeaping = true;

        // Play animation.
        animator.SetTrigger(leapTriggerHash);

        // Disable normal movement.
        movementController.enabled = false;

        // Disable controller only once during the leap.
        characterController.enabled = false;

        Vector3 startPosition =
            transform.position;

        Vector3 middlePosition =
            Vector3.Lerp(
                startPosition,
                landingPosition,
                0.5f
            );

        middlePosition.y += leapArcHeight;

        float elapsed = 0f;

        while (elapsed < leapDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / leapDuration
                );

            float smoothT =
                t * t * (3f - 2f * t);

            Vector3 leapPosition =
                QuadraticBezier(
                    startPosition,
                    middlePosition,
                    landingPosition,
                    smoothT
                );

            transform.position =
                leapPosition;

            yield return null;
        }

        // Force exact landing.
        transform.position =
            landingPosition;

        Physics.SyncTransforms();

        // Restore physics.
        characterController.enabled = true;

        // Restore movement.
        movementController.enabled = true;

        isLeaping = false;
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
        Vector3 forward =
            Vector3.ProjectOnPlane(
                transform.forward,
                Vector3.up
            ).normalized;

        Vector3 detectionOrigin =
            transform.position +
            Vector3.up * detectionHeight;

        Gizmos.color = Color.yellow;

        // Obstacle detection ray.
        Gizmos.DrawLine(
            detectionOrigin,
            detectionOrigin +
            forward * detectionDistance
        );
    }
}