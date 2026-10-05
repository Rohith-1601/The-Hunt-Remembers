using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class HunterAI : MonoBehaviour
{
    private enum HunterState
    {
        Patrolling,
        Investigating,
        Searching,
        Chasing
    }

    [Header("References")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform player;

    [Header("Patrol")]
    [SerializeField] private float patrolSpeed = 2.5f;
    [SerializeField] private float patrolArrivalDistance = 0.8f;

    [Header("Hearing")]
    [SerializeField] private float baseHearingRange = 3f;
    [SerializeField] private float minimumLoudness = 1f;
    [SerializeField] private float hearingRefreshCooldown = 0.75f;

    [Header("Vision")]
    [SerializeField] private float visionRange = 6f;
    [SerializeField] private float visionAngle = 70f;
    [SerializeField] private float eyeHeight = 1.5f;
    [SerializeField] private float visionCheckInterval = 0.1f;
    [SerializeField] private LayerMask visionMask = ~0;

    [Header("Investigation")]
    [SerializeField] private float investigationSpeed = 5f;
    [SerializeField] private float investigationArrivalDistance = 1.2f;

    [Header("Chase")]
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float losePlayerDelay = 2.5f;

    [Header("Search")]
    [SerializeField] private float searchDuration = 4f;

    [Header("Adaptive Memory")]
    [SerializeField] private bool useAdaptiveMemory = true;
    [SerializeField] private float adaptiveDetourChance = 0.65f;
    [SerializeField] private float adaptiveDetourCooldown = 20f;

    public static event Action<Vector3, float>
        OnHunterHeardSomething;

    public bool IsChasing =>
        state == HunterState.Chasing;

    private NavMeshAgent agent;

    private HunterState state;

    private Vector3 investigationTarget;
    private Vector3 lastKnownPlayerPosition;

    private int currentPatrolIndex = -1;

    private Coroutine searchCoroutine;

    private float nextAllowedHearingTime;
    private float nextVisionCheckTime;
    private float timeSinceLastPlayerSeen;

    private bool adaptiveDetourActive;
    private int adaptiveDetourIndex = -1;
    private float nextAdaptiveDetourTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.speed =
            patrolSpeed;

        state =
            HunterState.Patrolling;
    }

    private void Start()
    {
        FindPlayer();

        StartPatrol();
    }

    private void OnEnable()
    {
        NoiseManager.OnNoiseGenerated += OnNoiseGenerated;
    }

    private void OnDisable()
    {
        NoiseManager.OnNoiseGenerated -= OnNoiseGenerated;
    }

    private void Update()
    {
        TryDetectPlayer();

        switch (state)
        {
            case HunterState.Patrolling:
                UpdatePatrol();
                break;

            case HunterState.Investigating:
                UpdateInvestigation();
                break;

            case HunterState.Searching:
                break;

            case HunterState.Chasing:
                UpdateChase();
                break;
        }
    }

    // =========================================================
    // PLAYER
    // =========================================================

    private void FindPlayer()
    {
        if (player != null)
            return;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (playerObject != null)
        {
            player =
                playerObject.transform;
        }
    }

    // =========================================================
    // PATROL
    // =========================================================

    private void StartPatrol()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
        {
            agent.isStopped = true;
            return;
        }

        state =
            HunterState.Patrolling;

        agent.speed =
            patrolSpeed;

        agent.isStopped =
            false;

        adaptiveDetourActive = false;
        adaptiveDetourIndex = -1;

        bool usedAdaptiveRoute =
            TryStartAdaptiveDetour();

        if (!usedAdaptiveRoute)
        {
            GoToNextPatrolPoint();
        }
    }

    private void UpdatePatrol()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
        {
            return;
        }

        if (agent.pathPending)
            return;

        if (agent.remainingDistance >
            patrolArrivalDistance)
        {
            return;
        }

        // The Hunter reached a remembered area.
        if (adaptiveDetourActive)
        {
            currentPatrolIndex =
                adaptiveDetourIndex;

            adaptiveDetourActive =
                false;

            adaptiveDetourIndex =
                -1;

            Debug.Log(
                "HunterMemory: Adaptive detour complete. " +
                "Returning to regular route."
            );
        }

        GoToNextPatrolPoint();
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
        {
            return;
        }

        currentPatrolIndex++;

        if (currentPatrolIndex >=
            patrolPoints.Length)
        {
            currentPatrolIndex = 0;
        }

        Transform target =
            patrolPoints[currentPatrolIndex];

        if (target == null)
            return;

        agent.SetDestination(
            target.position
        );
    }

    private bool TryStartAdaptiveDetour()
    {
        if (!useAdaptiveMemory)
            return false;

        if (Time.time <
            nextAdaptiveDetourTime)
        {
            return false;
        }

        if (HunterMemory.Instance == null)
            return false;

        int rememberedIndex =
            HunterMemory.Instance
                .GetPreferredPatrolIndex();

        if (rememberedIndex < 0)
            return false;

        if (rememberedIndex ==
            currentPatrolIndex)
        {
            return false;
        }

        if (UnityEngine.Random.value >
            adaptiveDetourChance)
        {
            return false;
        }

        if (patrolPoints[rememberedIndex] == null)
            return false;

        adaptiveDetourIndex =
            rememberedIndex;

        adaptiveDetourActive =
            true;

        nextAdaptiveDetourTime =
            Time.time +
            adaptiveDetourCooldown;

        agent.speed =
            patrolSpeed;

        agent.isStopped =
            false;

        agent.SetDestination(
            patrolPoints[
                rememberedIndex
            ].position
        );

        Debug.Log(
            $"HunterMemory: Hunter is checking remembered area " +
            $"{patrolPoints[rememberedIndex].name}."
        );

        return true;
    }

    // =========================================================
    // HEARING
    // =========================================================

    private void OnNoiseGenerated(
        NoiseManager.NoiseEvent noiseEvent)
    {
        if (noiseEvent.Source == gameObject)
            return;

        // Once the Hunter is actively chasing,
        // visual pursuit takes priority over sound.
        if (state == HunterState.Chasing)
            return;

        if (Time.time <
            nextAllowedHearingTime)
        {
            return;
        }

        float distance =
            Vector3.Distance(
                transform.position,
                noiseEvent.Position
            );

        float hearingRange =
            baseHearingRange *
            noiseEvent.Loudness;

        if (noiseEvent.Loudness <
            minimumLoudness)
        {
            return;
        }

        if (distance >
            hearingRange)
        {
            return;
        }

        nextAllowedHearingTime =
            Time.time +
            hearingRefreshCooldown;

        investigationTarget =
            noiseEvent.Position;

        OnHunterHeardSomething?.Invoke(
            investigationTarget,
            noiseEvent.Loudness
        );

        BeginInvestigation();
    }

    private void BeginInvestigation()
    {
        if (searchCoroutine != null)
        {
            StopCoroutine(
                searchCoroutine
            );

            searchCoroutine =
                null;
        }

        adaptiveDetourActive =
            false;

        adaptiveDetourIndex =
            -1;

        state =
            HunterState.Investigating;

        agent.speed =
            investigationSpeed;

        agent.isStopped =
            false;

        agent.SetDestination(
            investigationTarget
        );

        Debug.Log(
            "Hunter heard a sound and is investigating."
        );
    }

    // =========================================================
    // INVESTIGATION
    // =========================================================

    private void UpdateInvestigation()
    {
        if (player != null &&
            CanSeePlayer())
        {
            lastKnownPlayerPosition =
                player.position;

            timeSinceLastPlayerSeen = 0f;

            BeginChase();

            return;
        }

        if (agent.pathPending)
            return;

        if (!agent.hasPath)
            return;

        if (agent.remainingDistance <=
            investigationArrivalDistance)
        {
            BeginSearch();
        }
    }

    // =========================================================
    // VISION
    // =========================================================

    private void TryDetectPlayer()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        if (Time.time <
            nextVisionCheckTime)
        {
            return;
        }

        nextVisionCheckTime =
            Time.time +
            visionCheckInterval;

        if (!CanSeePlayer())
            return;

        lastKnownPlayerPosition =
            player.position;

        timeSinceLastPlayerSeen =
            0f;

        if (state !=
            HunterState.Chasing)
        {
            BeginChase();
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 eyePosition =
            transform.position +
            Vector3.up * eyeHeight;

        Vector3 targetPosition =
            player.position +
            Vector3.up * 1f;

        Vector3 direction =
            targetPosition -
            eyePosition;

        float distance =
            direction.magnitude;

        if (distance > visionRange)
            return false;

        Vector3 flatDirection =
            new Vector3(
                direction.x,
                0f,
                direction.z
            );

        if (flatDirection.sqrMagnitude >
            0.001f)
        {
            flatDirection.Normalize();

            float angle =
                Vector3.Angle(
                    transform.forward,
                    flatDirection
                );

            if (angle >
                visionAngle * 0.5f)
            {
                return false;
            }
        }

        RaycastHit[] hits =
            Physics.RaycastAll(
                eyePosition,
                direction.normalized,
                distance,
                visionMask,
                QueryTriggerInteraction.Ignore
            );

        if (hits == null ||
            hits.Length == 0)
        {
            return false;
        }

        Array.Sort(
            hits,
            (a, b) =>
                a.distance.CompareTo(
                    b.distance
                )
        );

        foreach (RaycastHit hit in hits)
        {
            if (hit.transform == transform ||
                hit.transform.IsChildOf(transform))
            {
                continue;
            }

            if (hit.transform == player ||
                hit.transform.IsChildOf(player))
            {
                return true;
            }

            return false;
        }

        return false;
    }

    // =========================================================
    // CHASE
    // =========================================================

    private void BeginChase()
    {
        if (player == null)
            return;

        if (searchCoroutine != null)
        {
            StopCoroutine(
                searchCoroutine
            );

            searchCoroutine =
                null;
        }

        adaptiveDetourActive =
            false;

        adaptiveDetourIndex =
            -1;

        state =
            HunterState.Chasing;

        agent.speed =
            chaseSpeed;

        agent.isStopped =
            false;

        agent.SetDestination(
            player.position
        );

        Debug.Log(
            "Hunter detected the player and is chasing."
        );
    }

    private void UpdateChase()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        if (CanSeePlayer())
        {
            lastKnownPlayerPosition =
                player.position;

            timeSinceLastPlayerSeen =
                0f;
        }
        else
        {
            timeSinceLastPlayerSeen +=
                Time.deltaTime;
        }

        if (!agent.isStopped)
        {
            if (timeSinceLastPlayerSeen <=
                losePlayerDelay)
            {
                agent.SetDestination(
                    player.position
                );
            }
            else
            {
                agent.SetDestination(
                    lastKnownPlayerPosition
                );
            }
        }

        if (timeSinceLastPlayerSeen >
            losePlayerDelay)
        {
            if (!agent.pathPending &&
                agent.hasPath &&
                agent.remainingDistance <=
                investigationArrivalDistance)
            {
                BeginSearch();
            }
        }
    }

    // =========================================================
    // SEARCH
    // =========================================================

    private void BeginSearch()
    {
        state =
            HunterState.Searching;

        agent.isStopped =
            true;

        searchCoroutine =
            StartCoroutine(
                SearchArea()
            );

        Debug.Log(
            "Hunter reached the last known location and is searching."
        );
    }

    private IEnumerator SearchArea()
    {
        float timer = 0f;

        while (timer < searchDuration)
        {
            timer += Time.deltaTime;

            yield return null;
        }

        searchCoroutine = null;

        StartPatrol();
    }
}