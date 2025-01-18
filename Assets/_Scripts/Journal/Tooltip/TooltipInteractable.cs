using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class TooltipInteractable : MonoBehaviour, IInteractable
{
    #region Serialized Fields

    [SerializeField] private string interactText;
    [SerializeField] [TextArea(1, 8)] private string tooltipText;
    [SerializeField] private UnityEvent onInteraction;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    public bool IsInteractable => true;

    public bool HasOutline { get; set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    public InteractionIcon InteractionIcon => InteractionIcon.Action;

    public UnityEvent OnInteraction => onInteraction;
    
    #endregion

    public void Interact(PlayerInteraction playerInteraction)
    {
        JournalTooltipManager.Instance.AddTooltip(tooltipText);
        
        // Invoke the on interaction event
        onInteraction.Invoke();
    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        return interactText;
    }
}