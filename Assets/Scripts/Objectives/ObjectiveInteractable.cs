using UnityEngine;

public class ObjectiveInteractable : MonoBehaviour
{
    [Header("Objective")]
    [SerializeField]
    private ObjectiveManager.ObjectiveType objectiveType;

    [Header("Interaction")]
    [SerializeField]
    private float interactionDistance = 2f;

    [Header("Player")]
    [SerializeField]
    private Transform player;

    [Header("Completion Feedback")]
    [SerializeField]
    private GameObject objectToActivate;

    [SerializeField]
    private GameObject objectToDeactivate;

    [SerializeField]
    private EmergencySignalController emergencySignalController;

    private bool completed;
    private bool playerInRange;

    public bool PlayerInRange => playerInRange;

    // =========================================================
    // PROMPT
    // =========================================================

    public string GetPrompt()
    {
        switch (objectiveType)
        {
            case ObjectiveManager.ObjectiveType.RestorePower:
                return "[E] Restore Power";

            case ObjectiveManager.ObjectiveType.RecoverResearchSample:
                return "[E] Recover Research Sample";

            case ObjectiveManager.ObjectiveType.SendEmergencySignal:
                return "[E] Send Emergency Signal";

            case ObjectiveManager.ObjectiveType.Escape:
                return "[E] Escape";

            default:
                return "";
        }
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        if (player != null)
            return;

        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning(
                "ObjectiveInteractable: Player with tag 'Player' was not found.",
                this
            );
        }
    }

    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        playerInRange = false;

        if (completed)
            return;

        if (player == null)
        {
            FindPlayer();
            return;
        }

        if (ObjectiveManager.Instance == null)
            return;

        // Do not allow interaction after the game is won.
        if (ObjectiveManager.Instance.GameWon)
            return;

        // Only the CURRENT objective can be interacted with.
        if (ObjectiveManager.Instance.CurrentObjective != objectiveType)
            return;

        float distance =
            Vector3.Distance(
                player.position,
                transform.position
            );

        if (distance <= interactionDistance)
        {
            playerInRange = true;

            if (Input.GetKeyDown(KeyCode.E))
            {
                CompleteInteraction();
            }
        }
    }

    // =========================================================
    // COMPLETE
    // =========================================================

    private void CompleteInteraction()
    {
        if (completed)
            return;

        if (ObjectiveManager.Instance == null)
            return;

        if (ObjectiveManager.Instance.GameWon)
            return;

        // Safety check:
        // make sure this is still the active objective.
        if (ObjectiveManager.Instance.CurrentObjective != objectiveType)
            return;

        completed = true;

        // Activate completion object.
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
        }

        // Deactivate old object.
        if (objectToDeactivate != null)
        {
            objectToDeactivate.SetActive(false);
        }

        // Tell ObjectiveManager to advance.
        ObjectiveManager.Instance.CompleteObjective(
            objectiveType
        );

        if (emergencySignalController != null)
        {
            emergencySignalController.SendSignal();
        }
    }
}