using UnityEngine;

public class PlayerFootstepAudio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ThirdPersonController controller;
    [SerializeField] private AudioSource audioSource;

    [Header("Normal Footsteps")]
    [SerializeField] private AudioClip[] walkFootsteps;

    [Header("Crouch")]
    [SerializeField] private AudioClip crouchFootstep;

    [Header("Sprint")]
    [SerializeField] private AudioClip sprintFootstep;

    [Header("Timing")]
    [SerializeField] private float walkInterval = 0.45f;
    [SerializeField] private float sprintInterval = 0.30f;
    [SerializeField] private float crouchInterval = 0.55f;

    [Header("Volume")]
    [SerializeField] private float walkVolume = 0.55f;
    [SerializeField] private float sprintVolume = 0.75f;
    [SerializeField] private float crouchVolume = 0.25f;

    private float footstepTimer;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<ThirdPersonController>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (controller == null)
        {
            Debug.LogError(
                "PlayerFootstepAudio: ThirdPersonController is missing.",
                this
            );

            enabled = false;
            return;
        }

        if (audioSource == null)
        {
            Debug.LogError(
                "PlayerFootstepAudio: AudioSource is missing.",
                this
            );

            enabled = false;
        }
    }

    private void Update()
    {
        if (controller.CurrentSpeed <= 0.01f)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer += Time.deltaTime;

        float interval;

        if (controller.IsCrouching)
            interval = crouchInterval;
        else if (controller.IsSprinting)
            interval = sprintInterval;
        else
            interval = walkInterval;

        if (footstepTimer >= interval)
        {
            footstepTimer = 0f;
            PlayFootstep();
        }
    }

    private void PlayFootstep()
    {
        AudioClip clip = null;
        float volume = 0.5f;

        if (controller.IsCrouching)
        {
            clip = crouchFootstep;
            volume = crouchVolume;
        }
        else if (controller.IsSprinting)
        {
            clip = sprintFootstep;
            volume = sprintVolume;
        }
        else
        {
            if (walkFootsteps != null &&
                walkFootsteps.Length > 0)
            {
                clip =
                    walkFootsteps[
                        Random.Range(
                            0,
                            walkFootsteps.Length
                        )
                    ];
            }

            volume = walkVolume;
        }

        if (clip == null)
            return;

        audioSource.PlayOneShot(
            clip,
            volume
        );
    }

    public void PlayLandingSound(AudioClip landingClip)
    {
        if (landingClip == null)
            return;

        audioSource.PlayOneShot(
            landingClip,
            0.8f
        );
    }
}