using UnityEngine;
using TMPro;

public class ObjectiveHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text objectiveText;

    private void OnEnable()
    {
        ObjectiveManager.OnObjectiveChanged += UpdateObjective;
    }

    private void OnDisable()
    {
        ObjectiveManager.OnObjectiveChanged -= UpdateObjective;
    }

    private void Start()
    {
        if (ObjectiveManager.Instance != null)
        {
            UpdateObjective(
                ObjectiveManager.Instance.CurrentObjective
            );
        }
    }

    private void UpdateObjective(
        ObjectiveManager.ObjectiveType objective)
    {
        objectiveText.text =
            GetObjectiveText(objective);
    }

    private string GetObjectiveText(
        ObjectiveManager.ObjectiveType objective)
    {
        switch (objective)
        {
            case ObjectiveManager.ObjectiveType.RestorePower:
                return "Restore Power";

            case ObjectiveManager.ObjectiveType.RecoverResearchSample:
                return "Recover Research Sample";

            case ObjectiveManager.ObjectiveType.SendEmergencySignal:
                return "Send Emergency Signal";

            case ObjectiveManager.ObjectiveType.Escape:
                return "Escape";

            default:
                return "";
        }
    }
}