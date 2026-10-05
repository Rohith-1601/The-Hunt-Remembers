using UnityEngine;

public class HunterHearingAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource audioSource;

    [Header("Heard Sound")]
    [SerializeField] private AudioClip heardRoar;

    [Header("Audio Settings")]
    [SerializeField] private float volume = 0.8f;

    [Header("Roar Cooldown")]
    [SerializeField] private float roarCooldown = 2.5f;

    private float nextRoarTime;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            Debug.LogError(
                "HunterHearingAudio: AudioSource is missing.",
                this
            );

            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        HunterAI.OnHunterHeardSomething += HandleHunterHeard;
    }

    private void OnDisable()
    {
        HunterAI.OnHunterHeardSomething -= HandleHunterHeard;
    }

    private void HandleHunterHeard(
        Vector3 soundPosition,
        float loudness)
    {
        // No roar assigned.
        if (heardRoar == null)
            return;

        // Prevent the roar from repeating too frequently.
        if (Time.time < nextRoarTime)
            return;

        // Start the cooldown.
        nextRoarTime =
            Time.time + roarCooldown;

        // Play exactly one roar for this hearing response.
        audioSource.PlayOneShot(
            heardRoar,
            volume
        );

        Debug.Log(
            $"Hunter heard something. Roar played. Loudness = {loudness:F1}"
        );
    }
}