using System;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public enum ObjectiveType
    {
        RestorePower,
        RecoverResearchSample,
        SendEmergencySignal,
        Escape
    }

    public static ObjectiveManager Instance { get; private set; }

    [Header("Current Objective")]
    [SerializeField]
    private ObjectiveType currentObjective =
        ObjectiveType.RestorePower;

    public ObjectiveType CurrentObjective =>
        currentObjective;

    public bool GameWon { get; private set; }

    public static event Action<ObjectiveType> OnObjectiveChanged;

    public static event Action<ObjectiveType> OnObjectiveCompleted;

    public static event Action OnGameWon;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        // Tell the HUD which objective is active at the start.
        OnObjectiveChanged?.Invoke(currentObjective);
    }

    // =========================================================
    // CHECK IF OBJECTIVE CAN BE COMPLETED
    // =========================================================

    public bool CanComplete(ObjectiveType objective)
    {
        // Objective can only be completed when:
        // 1. Game is not already won
        // 2. The requested objective is the current objective
        return !GameWon &&
               objective == currentObjective;
    }

    // =========================================================
    // COMPLETE OBJECTIVE
    // =========================================================

    public void CompleteObjective(ObjectiveType objective)
    {
        // Do nothing if game has already been won.
        if (GameWon)
            return;

        // Do nothing if this is not the current objective.
        if (objective != currentObjective)
            return;

        // Tell other systems that this objective was completed.
        OnObjectiveCompleted?.Invoke(objective);

        // Move to the next objective.
        switch (objective)
        {
            case ObjectiveType.RestorePower:

                currentObjective =
                    ObjectiveType.RecoverResearchSample;

                break;

            case ObjectiveType.RecoverResearchSample:

                currentObjective =
                    ObjectiveType.SendEmergencySignal;

                break;

            case ObjectiveType.SendEmergencySignal:

                currentObjective =
                    ObjectiveType.Escape;

                break;

            case ObjectiveType.Escape:

                WinGame();

                return;
        }

        // Update the HUD.
        OnObjectiveChanged?.Invoke(currentObjective);
    }

    // =========================================================
    // GAME WIN
    // =========================================================

    private void WinGame()
    {
        GameWon = true;

        Debug.Log("GAME WON!");

        OnGameWon?.Invoke();
    }
}