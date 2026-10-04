using UnityEngine;

public class PlayerNoiseEmitter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ThirdPersonController controller;

    [Header("Noise Levels")]
    [SerializeField] private float crouchNoise = 1.5f;
    [SerializeField] private float walkNoise = 4f;
    [SerializeField] private float sprintNoise = 9f;

    [Header("Noise Timing")]
    [SerializeField] private float noiseInterval = 0.25f;

    private float noiseTimer;

    private void Awake()
    {
        if (controller == null)
        {
            controller =
                GetComponent<ThirdPersonController>();
        }

        if (controller == null)
        {
            Debug.LogError(
                "PlayerNoiseEmitter: " +
                "ThirdPersonController reference is missing.",
                this
            );

            enabled = false;
        }
    }

    private void Update()
    {
        if (NoiseManager.Instance == null)
            return;

        if (controller.CurrentSpeed <= 0.01f)
        {
            noiseTimer = 0f;
            return;
        }

        noiseTimer += Time.deltaTime;

        if (noiseTimer < noiseInterval)
            return;

        noiseTimer = 0f;

        GenerateMovementNoise();
    }

    private void GenerateMovementNoise()
    {
        float loudness;

        if (controller.IsCrouching)
        {
            loudness = crouchNoise;
        }
        else if (controller.IsSprinting)
        {
            loudness = sprintNoise;
        }
        else
        {
            loudness = walkNoise;
        }

        NoiseManager.Instance.GenerateNoise(
            transform.position,
            loudness,
            gameObject
        );
    }
}