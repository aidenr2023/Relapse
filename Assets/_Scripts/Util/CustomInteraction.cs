using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CustomInteraction : MonoBehaviour, IInteractable
{
    #region Serialized Fields

    [SerializeField] private string interactText = "Interact";

    [SerializeField] private UnityEvent OnInteract;

    [SerializeField] private InteractionIcon interactionIcon;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;
    public bool IsInteractable => true;

    public bool HasOutline { get; set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    public InteractionIcon InteractionIcon => interactionIcon;

    #endregion

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

    public void PlayTutorial(TutorialScreen.Tutorial tutorial)
    {
        // Play the tutorial
        TutorialScreen.Instance.ChangeTutorial(tutorial);
        TutorialScreen.Instance.Activate();
    }
}