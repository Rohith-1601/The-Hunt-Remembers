using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class HunterAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [Header("Movement")]
    [SerializeField] private float maxRunSpeed = 5f;
    [SerializeField] private float speedDampTime = 0.1f;

    private NavMeshAgent agent;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (animator == null)
        {
            Debug.LogError(
                "HunterAnimationController: Animator not found.",
                this
            );

            enabled = false;
        }
    }

    private void Update()
    {
        UpdateMovementAnimation();
    }

    private void UpdateMovementAnimation()
    {
        if (agent == null ||
            !agent.isOnNavMesh)
            return;

        float movementSpeed =
            agent.velocity.magnitude;

        float normalizedSpeed =
            Mathf.Clamp01(
                movementSpeed / maxRunSpeed
            );

        animator.SetFloat(
            SpeedHash,
            normalizedSpeed,
            speedDampTime,
            Time.deltaTime
        );
    }

    public void PlayAttack()
    {
        if (animator == null)
            return;

        animator.SetTrigger(
            AttackHash
        );
    }
}