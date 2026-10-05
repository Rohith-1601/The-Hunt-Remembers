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
        Searching
    }

    [Header("References")]
    [SerializeField] private Transform[] patrolPoints;

    [Header("Patrol")]
    [SerializeField] private float patrolSpeed = 2.5f;
    [SerializeField] private float patrolArrivalDistance = 0.8f;

    [Header("Hearing")]
    [SerializeField] private float baseHearingRange = 3f;
    [SerializeField] private float minimumLoudness = 1f;
    [SerializeField] private float hearingRefreshCooldown = 0.75f;

    [Header("Investigation")]
    [SerializeField] private float investigationSpeed = 5f;
    [SerializeField] private float investigationArrivalDistance = 1.2f;

    [Header("Search")]
    [SerializeField] private float searchDuration = 4f;
    [SerializeField] private float searchRotationSpeed = 90f;

    public static event Action<Vector3, float> OnHunterHeardSomething;

    private NavMeshAgent agent;

    private HunterState state;

    private Vector3 investigationTarget;

    private int currentPatrolIndex = -1;

    private Coroutine searchCoroutine;

    private float nextAllowedHearingTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.speed = patrolSpeed;

        state = HunterState.Patrolling;
    }

    private void Start()
    {
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

        state = HunterState.Patrolling;

        agent.speed = patrolSpeed;
        agent.isStopped = false;

        GoToNextPatrolPoint();
    }

    private void UpdatePatrol()
    {
        if (patrolPoints == null ||
            patrolPoints.Length == 0)
            return;

        if (agent.pathPending)
            return;

        if (agent.remainingDistance <= patrolArrivalDistance)
        {
            GoToNextPatrolPoint();
        }
    }

    private void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0)
            return;

        currentPatrolIndex++;

        if (currentPatrolIndex >= patrolPoints.Length)
        {
            currentPatrolIndex = 0;
        }

        Transform target =
            patrolPoints[currentPatrolIndex];

        if (target == null)
            return;

        agent.SetDestination(target.position);
    }

    // =========================================================
    // HEARING
    // =========================================================

    private void OnNoiseGenerated(
        NoiseManager.NoiseEvent noiseEvent)
    {
        if (noiseEvent.Source == gameObject)
            return;

        if (Time.time < nextAllowedHearingTime)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                noiseEvent.Position
            );

        float hearingRange =
            baseHearingRange *
            noiseEvent.Loudness;

        if (noiseEvent.Loudness < minimumLoudness)
            return;

        if (distance > hearingRange)
            return;

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

    // =========================================================
    // INVESTIGATION
    // =========================================================

    private void BeginInvestigation()
    {
        if (searchCoroutine != null)
        {
            StopCoroutine(searchCoroutine);
            searchCoroutine = null;
        }

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

    private void UpdateInvestigation()
    {
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
    // SEARCH
    // =========================================================

    private void BeginSearch()
    {
        state =
            HunterState.Searching;

        agent.isStopped = true;

        searchCoroutine =
            StartCoroutine(
                SearchArea()
            );

        Debug.Log(
            "Hunter reached the sound and is searching."
        );
    }

    private IEnumerator SearchArea()
    {
        float timer = 0f;

        while (timer < searchDuration)
        {
            timer += Time.deltaTime;

            transform.Rotate(
                Vector3.up,
                searchRotationSpeed *
                Time.deltaTime
            );

            yield return null;
        }

        searchCoroutine = null;

        // Return to regular route
        state =
            HunterState.Patrolling;

        agent.speed =
            patrolSpeed;

        agent.isStopped =
            false;

        GoToNextPatrolPoint();
    }
}