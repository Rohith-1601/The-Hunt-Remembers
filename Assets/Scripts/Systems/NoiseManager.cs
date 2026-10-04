using System;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    public static NoiseManager Instance { get; private set; }

    public struct NoiseEvent
    {
        public Vector3 Position;
        public float Loudness;
        public GameObject Source;

        public NoiseEvent(
            Vector3 position,
            float loudness,
            GameObject source)
        {
            Position = position;
            Loudness = loudness;
            Source = source;
        }
    }

    public static event Action<NoiseEvent> OnNoiseGenerated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void GenerateNoise(
        Vector3 position,
        float loudness,
        GameObject source)
    {
        NoiseEvent noiseEvent =
            new NoiseEvent(
                position,
                loudness,
                source
            );

        OnNoiseGenerated?.Invoke(noiseEvent);

        Debug.Log(
            $"Noise generated: " +
            $"Loudness={loudness:F1}, " +
            $"Position={position}"
        );
    }
}