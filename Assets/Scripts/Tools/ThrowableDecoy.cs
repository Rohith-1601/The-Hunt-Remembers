using UnityEngine;

public class ThrowableDecoy : MonoBehaviour
{
    [Header("Noise")]
    [SerializeField] private float impactLoudness = 12f;

    [Header("Lifetime")]
    [SerializeField] private float lifetime = 8f;

    [Header("Impact Audio")]
    [SerializeField] private AudioClip impactSound;
    [SerializeField] private float impactVolume = 0.8f;

    private bool hasImpacted;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasImpacted)
            return;

        hasImpacted = true;

        GenerateImpactNoise();

        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(
                impactSound,
                transform.position,
                impactVolume
            );
        }
    }

    private void GenerateImpactNoise()
    {
        if (NoiseManager.Instance == null)
            return;

        NoiseManager.Instance.GenerateNoise(
            transform.position,
            impactLoudness,
            gameObject
        );

        Debug.Log(
            $"Throwable impact noise generated at {transform.position}"
        );
    }
}