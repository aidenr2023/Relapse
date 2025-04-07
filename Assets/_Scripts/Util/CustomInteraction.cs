using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(InteractableMaterialManager))]
public class CustomInteraction : MonoBehaviour, IInteractable
{
    #region Serialized Fields

    [SerializeField] private string interactText = "Interact";

    [SerializeField] private bool interactOnce;

    [SerializeField] private UnityEvent OnInteract;

    [SerializeField] private InteractionIcon interactionIcon;

    #endregion

    private bool _hasInteracted;

    #region Getters

    public InteractableMaterialManager InteractableMaterialManager { get; set; }

    public GameObject GameObject => gameObject;
    public bool IsInteractable => (interactOnce && !_hasInteracted) || !interactOnce;

    public bool HasOutline { get; set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    public InteractionIcon InteractionIcon => interactionIcon;

    public UnityEvent OnInteraction => OnInteract;

    #endregion

    public void Interact(PlayerInteraction playerInteraction)
    {
        // Invoke the event
        OnInteract.Invoke();
        
        // Set the interacted flag
        _hasInteracted = true;
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

    public void PlayTutorial(Tutorial tutorial)
    {
        // Play the tutorial
        TutorialScreen.Play(this, tutorial);
    }
}