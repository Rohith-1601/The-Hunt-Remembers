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

    [Header("Current Objective")]
    [SerializeField]
    private ObjectiveType currentObjective =
        ObjectiveType.RestorePower;

    public ObjectiveType CurrentObjective =>
        currentObjective;

    public bool GameWon { get; private set; }

    public static event Action<ObjectiveType>
        OnObjectiveChanged;

    public static event Action<ObjectiveType>
        OnObjectiveCompleted;

    public static event Action
        OnGameWon;

    private void Start()
    {
        OnObjectiveChanged?.Invoke(
            currentObjective
        );
    }

    public bool CanInteract(
        ObjectiveType objective)
    {
        return !GameWon &&
               objective == currentObjective;
    }

    public void CompleteObjective(
        ObjectiveType objective)
    {
        if (GameWon)
            return;

        if (objective != currentObjective)
            return;

        OnObjectiveCompleted?.Invoke(
            objective
        );

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

        OnObjectiveChanged?.Invoke(
            currentObjective
        );
    }

    private void WinGame()
    {
        GameWon = true;

        OnGameWon?.Invoke();

        Debug.Log(
            "GAME WON!"
        );
    }
}