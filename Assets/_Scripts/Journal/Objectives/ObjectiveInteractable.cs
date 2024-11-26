using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectiveInteractable : MonoBehaviour, IInteractable
{
    #region Serialized Fields

    [SerializeField] private string interactText;

    [SerializeField] private JournalObjectiveMode objectiveMode;
    [SerializeField] private JournalObjective objective;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    public bool IsInteractable => true;

    public bool HasOutline { get; set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    #endregion

    public void Interact(PlayerInteraction playerInteraction)
    {
        switch (objectiveMode)
        {
            case JournalObjectiveMode.Add:
                JournalObjectiveManager.Instance.AddObjective(objective);
                break;

            case JournalObjectiveMode.Complete:
                JournalObjectiveManager.Instance.CompleteObjective(objective);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        return interactText;
    }
}