using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Interact";

    [SerializeField] private UnityEvent OnInteract;

    public GameObject GameObject => gameObject;
    public bool IsInteractable => true;

    public bool HasOutline { get; set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    public void Interact(PlayerInteraction playerInteraction)
    {
        // Invoke the event
        OnInteract.Invoke();
    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        return interactText;
    }

    public void AddTooltip(string message)
    {
        // Add the tooltip
        JournalTooltipManager.Instance.AddTooltip(message);
    }

    public void AddObjective(JournalObjective objective)
    {
        // Add the objective
        JournalObjectiveManager.Instance.AddObjective(objective);
    }

    public void CompleteObjective(JournalObjective objective)
    {
        // Complete the objective
        JournalObjectiveManager.Instance.CompleteObjective(objective);
    }
}