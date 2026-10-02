using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 60f;

    [Header("Cursor")]
    [SerializeField] private bool lockCursor = true;

    private float yaw;
    private float pitch;

    public float Yaw => yaw;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("CameraOrbit: Player reference is missing.", this);
            enabled = false;
            return;
        }

        yaw = player.eulerAngles.y;
        pitch = 10f;

        SetCursorState(lockCursor);
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Keep the solved WORLD rotation relationship.
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // Follow the player's position while keeping the pivot's own height.
        transform.position = new Vector3(
            player.position.x,
            player.position.y + 1.6f,
            player.position.z
        );

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            lockCursor = !lockCursor;
            SetCursorState(lockCursor);
        }
    }

    private void SetCursorState(bool locked)
    {
        Cursor.lockState = locked
            ? CursorLockMode.Locked
            : CursorLockMode.None;

        Cursor.visible = !locked;
    }
}