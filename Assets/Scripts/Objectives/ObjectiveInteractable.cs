using UnityEngine;

public class ObjectiveInteractable : MonoBehaviour
{
    [Header("Objective")]
    [SerializeField]
    private ObjectiveManager.ObjectiveType objectiveType;

    [Header("Interaction")]
    [SerializeField]
    private float interactionDistance = 2f;

    [Header("References")]
    [SerializeField]
    private Transform player;

    private bool playerNearby;

    public string ObjectiveName =>
        GetObjectiveName();

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObject =
                GameObject.FindGameObjectWithTag(
                    "Player"
                );

            if (playerObject != null)
                player = playerObject.transform;
        }
    }

    private void Update()
    {
        if (player == null)
            return;

        ObjectiveManager manager =
            FindObjectOfType<ObjectiveManager>();

        if (manager == null)
            return;

        if (!manager.CanInteract(objectiveType))
            return;

        float distance =
            Vector3.Distance(
                player.position,
                transform.position
            );

        playerNearby =
            distance <= interactionDistance;

        if (playerNearby &&
            Input.GetKeyDown(KeyCode.E))
        {
            manager.CompleteObjective(
                objectiveType
            );
        }
    }

    private string GetObjectiveName()
    {
        switch (objectiveType)
        {
            case ObjectiveManager.ObjectiveType.RestorePower:
                return "Restore Power";

            case ObjectiveManager.ObjectiveType.RecoverResearchSample:
                return "Recover Research Sample";

            case ObjectiveManager.ObjectiveType.SendEmergencySignal:
                return "Send Emergency Signal";

            case ObjectiveManager.ObjectiveType.Escape:
                return "Escape";
        }

        return "";
    }
}