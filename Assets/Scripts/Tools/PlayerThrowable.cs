using UnityEngine;

public class PlayerThrowable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject throwablePrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private Camera playerCamera;

    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 12f;
    [SerializeField] private float upwardForce = 2f;
    [SerializeField] private float throwCooldown = 1f;

    private float nextThrowTime;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (throwPoint == null)
        {
            Debug.LogError(
                "PlayerThrowable: Throw Point is missing.",
                this
            );

            enabled = false;
            return;
        }

        if (throwablePrefab == null)
        {
            Debug.LogError(
                "PlayerThrowable: Throwable Prefab is missing.",
                this
            );

            enabled = false;
        }
    }

    private void Update()
    {
        if (Time.time < nextThrowTime)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ThrowObject();
        }
    }

    private void ThrowObject()
    {
        nextThrowTime =
            Time.time + throwCooldown;

        GameObject throwable =
            Instantiate(
                throwablePrefab,
                throwPoint.position,
                Quaternion.identity
            );

        Rigidbody rigidbody =
            throwable.GetComponent<Rigidbody>();

        if (rigidbody == null)
        {
            Debug.LogError(
                "PlayerThrowable: Throwable prefab does not have a Rigidbody.",
                throwable
            );

            Destroy(throwable);
            return;
        }

        // Prevent the thrown object from immediately
        // colliding with the player.
        Collider[] playerColliders =
            GetComponentsInChildren<Collider>();

        Collider[] throwableColliders =
            throwable.GetComponentsInChildren<Collider>();

        foreach (Collider playerCollider in playerColliders)
        {
            foreach (Collider throwableCollider in throwableColliders)
            {
                Physics.IgnoreCollision(
                    playerCollider,
                    throwableCollider
                );
            }
        }

        // Throw toward the center of the player's camera.
        Vector3 throwDirection =
            playerCamera.transform.forward;

        throwDirection =
            (throwDirection +
             Vector3.up * upwardForce)
            .normalized;

        rigidbody.AddForce(
            throwDirection * throwForce,
            ForceMode.Impulse
        );
    }
}