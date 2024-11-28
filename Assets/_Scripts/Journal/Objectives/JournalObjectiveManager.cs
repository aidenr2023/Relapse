using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class JournalObjectiveManager
{
    public static JournalObjectiveManager Instance => Journal.Instance?.JournalObjectiveManager;

    #region Serialized Fields

    [SerializeField] [Min(0)] private float objectiveTooltipDuration = 10;

    [SerializeField] private List<JournalObjective> activeObjectives;
    [SerializeField] private List<JournalObjective> completeObjectives;

    #endregion

    #region Private Fields

    private Journal _journal;

    #endregion

    #region Getters

    public IReadOnlyCollection<JournalObjective> ActiveObjectives => activeObjectives;

    public IReadOnlyCollection<JournalObjective> CompleteObjectives => completeObjectives;

    #endregion

    public event Action<JournalObjective> OnObjectiveAdded;
    public event Action<JournalObjective> OnObjectiveComplete;

    public void Initialize(Journal journal)
    {
        // Set the journal
        _journal = journal;

        InitializeEvents();
    }

    private void InitializeEvents()
    {
        // Initialize the events
        OnObjectiveAdded += AddTooltipOnObjectiveAdded;
        OnObjectiveComplete += AddTooltipOnObjectiveComplete;
    }

    public void AddObjective(JournalObjective objective)
    {
        // Return if the objective is already active
        if (activeObjectives.Contains(objective))
            return;

        // Return if the objective is already complete
        if (completeObjectives.Contains(objective))
            return;

        // Add the objective to the list of active objectives
        activeObjectives.Add(objective);

        // Invoke the event
        OnObjectiveAdded?.Invoke(objective);
    }

    public void CompleteObjective(JournalObjective objective)
    {
        // Remove the objective from the list of active objectives
        activeObjectives.Remove(objective);

        // Check if the objective is already complete
        var objectiveInList = completeObjectives.Contains(objective);

        // Return if the objective is already complete
        if (objectiveInList)
            return;

        // Add the objective to the list of complete objectives
        completeObjectives.Add(objective);

        // Invoke the event
        OnObjectiveComplete?.Invoke(objective);
    }

    private void AddTooltipOnObjectiveAdded(JournalObjective objective)
    {
        var objectiveText = $"New Objective:\n{objective.TooltipText}";

        // Add a tooltip for the objective
        _journal.JournalTooltipManager.AddTooltip(objectiveText, objectiveTooltipDuration);
    }

    private void AddTooltipOnObjectiveComplete(JournalObjective objective)
    {
        var objectiveText = $"Objective Complete:\n{objective.TooltipText}";

        // Add a tooltip for the objective
        _journal.JournalTooltipManager.AddTooltip(objectiveText, objectiveTooltipDuration);
    }

    public bool IsObjectiveComplete(JournalObjective objective)
    {
        return completeObjectives.Contains(objective);
    }
}