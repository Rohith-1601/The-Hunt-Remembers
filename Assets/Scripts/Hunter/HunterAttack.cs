using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class HunterAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HunterAnimationController animationController;
    [SerializeField] private HunterAI hunterAI;
    [SerializeField] private Transform player;

    [Header("Attack Range")]
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Attack Timing")]
    [SerializeField] private float attackDuration = 1.0f;
    [SerializeField] private float damageDelay = 0.35f;

    [Header("Damage")]
    [SerializeField] private int biteDamage = 1;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 12f;

    private NavMeshAgent agent;

    private bool isAttacking;
    private bool damageApplied;

    private float nextAttackTime;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animationController == null)
        {
            animationController =
                GetComponent<HunterAnimationController>();
        }

        if (hunterAI == null)
        {
            hunterAI =
                GetComponent<HunterAI>();
        }
    }

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        // Hunter must actually be chasing
        // before it can attack.
        if (hunterAI == null ||
            !hunterAI.IsChasing)
        {
            return;
        }

        if (isAttacking)
            return;

        if (Time.time < nextAttackTime)
            return;

        float distance =
            Vector3.Distance(
                transform.position,
                player.position
            );

        if (distance <= attackRange)
        {
            BeginAttack();
        }
    }

    private void FindPlayer()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player =
                playerObject.transform;
        }
    }

    private void BeginAttack()
    {
        if (isAttacking)
            return;

        isAttacking = true;
        damageApplied = false;

        nextAttackTime =
            Time.time +
            attackCooldown;

        agent.isStopped =
            true;

        agent.updateRotation =
            false;

        FacePlayer();

        animationController.PlayBite();

        StartCoroutine(
            AttackRoutine()
        );
    }

    private IEnumerator AttackRoutine()
    {
        float timer = 0f;

        while (timer < attackDuration)
        {
            timer += Time.deltaTime;

            FacePlayer();

            if (!damageApplied &&
                timer >= damageDelay)
            {
                ApplyBiteDamage();
            }

            yield return null;
        }

        isAttacking = false;

        agent.updateRotation =
            true;

        agent.isStopped =
            false;
    }

    private void FacePlayer()
    {
        if (player == null)
            return;

        Vector3 direction =
            player.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(
                direction
            );

        transform.rotation =
            Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed *
                Time.deltaTime
            );
    }

    private void ApplyBiteDamage()
    {
        damageApplied = true;

        if (player == null)
            return;

        PlayerHealth playerHealth =
            player.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogError(
                "HunterAttack: PlayerHealth was not found.",
                player
            );

            return;
        }

        playerHealth.TakeDamage(
            biteDamage
        );

        Debug.Log(
            "HUNTER BITE HIT PLAYER"
        );
    }
}