using System;
using UnityEngine;

public class HunterMemory : MonoBehaviour
{
    public static HunterMemory Instance { get; private set; }

    [Header("Patrol Memory Zones")]
    [SerializeField] private Transform[] patrolPoints;

    [Header("Memory")]
    [SerializeField] private int memoryThreshold = 3;
    [SerializeField] private float minimumLoudnessToRemember = 4f;
    [SerializeField] private float maxRecordDistance = 15f;
    [SerializeField] private float recordCooldown = 1.5f;
    [SerializeField] private int maximumMemoryScore = 10;

    private int[] memoryScores;
    private float[] nextRecordTimes;

    private const string SaveKeyPrefix =
        "HunterMemory_";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeMemory();
    }

    private void OnEnable()
    {
        NoiseManager.OnNoiseGenerated += OnNoiseGenerated;
    }

    private void OnDisable()
    {
        NoiseManager.OnNoiseGenerated -= OnNoiseGenerated;
    }

    private void InitializeMemory()
    {
        if (patrolPoints == null)
        {
            patrolPoints = Array.Empty<Transform>();
        }

        memoryScores =
            new int[patrolPoints.Length];

        nextRecordTimes =
            new float[patrolPoints.Length];

        LoadMemory();
    }

    private void OnNoiseGenerated(
        NoiseManager.NoiseEvent noiseEvent)
    {
        if (noiseEvent.Source == null)
            return;

        // Only remember the actual player's movement noise.
        // Decoys and other objects are ignored.
        if (!noiseEvent.Source.CompareTag("Player"))
            return;

        // Ignore very quiet movement.
        if (noiseEvent.Loudness <
            minimumLoudnessToRemember)
        {
            return;
        }

        int nearestIndex =
            FindNearestPatrolPoint(
                noiseEvent.Position
            );

        if (nearestIndex < 0)
            return;

        float distance =
            Vector3.Distance(
                noiseEvent.Position,
                patrolPoints[nearestIndex].position
            );

        if (distance > maxRecordDistance)
            return;

        if (Time.time <
            nextRecordTimes[nearestIndex])
        {
            return;
        }

        nextRecordTimes[nearestIndex] =
            Time.time + recordCooldown;

        memoryScores[nearestIndex] =
            Mathf.Min(
                memoryScores[nearestIndex] + 1,
                maximumMemoryScore
            );

        SaveMemory(nearestIndex);

        Debug.Log(
            $"HunterMemory: Player behavior remembered at " +
            $"{patrolPoints[nearestIndex].name}. " +
            $"Score = {memoryScores[nearestIndex]}"
        );
    }

    private int FindNearestPatrolPoint(
        Vector3 position)
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
        {
            return -1;
        }

        int nearestIndex = -1;
        float nearestDistance =
            Mathf.Infinity;

        for (int i = 0;
             i < patrolPoints.Length;
             i++)
        {
            if (patrolPoints[i] == null)
                continue;

            float distance =
                Vector3.Distance(
                    position,
                    patrolPoints[i].position
                );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }

        return nearestIndex;
    }

    public int GetPreferredPatrolIndex()
    {
        if (memoryScores == null ||
            memoryScores.Length == 0)
        {
            return -1;
        }

        int highestScore =
            memoryThreshold;

        int[] candidates =
            new int[memoryScores.Length];

        int candidateCount = 0;

        for (int i = 0;
             i < memoryScores.Length;
             i++)
        {
            if (memoryScores[i] >= highestScore)
            {
                if (patrolPoints[i] != null)
                {
                    candidates[candidateCount] =
                        i;

                    candidateCount++;
                }
            }
        }

        if (candidateCount == 0)
            return -1;

        // Randomly choose between equally strong
        // remembered areas.
        int selected =
            UnityEngine.Random.Range(
                0,
                candidateCount
            );

        return candidates[selected];
    }

    public int GetMemoryScore(int index)
    {
        if (memoryScores == null ||
            index < 0 ||
            index >= memoryScores.Length)
        {
            return 0;
        }

        return memoryScores[index];
    }

    private void SaveMemory(int index)
    {
        if (index < 0 ||
            index >= memoryScores.Length)
        {
            return;
        }

        PlayerPrefs.SetInt(
            SaveKeyPrefix + index,
            memoryScores[index]
        );

        PlayerPrefs.Save();
    }

    private void LoadMemory()
    {
        for (int i = 0;
             i < memoryScores.Length;
             i++)
        {
            memoryScores[i] =
                PlayerPrefs.GetInt(
                    SaveKeyPrefix + i,
                    0
                );
        }
    }

    [ContextMenu("Reset Hunter Memory")]
    public void ResetMemory()
    {
        if (memoryScores == null)
            return;

        for (int i = 0;
             i < memoryScores.Length;
             i++)
        {
            memoryScores[i] = 0;

            PlayerPrefs.DeleteKey(
                SaveKeyPrefix + i
            );
        }

        PlayerPrefs.Save();

        Debug.Log(
            "HunterMemory: Memory reset."
        );
    }
}