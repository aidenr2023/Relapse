using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(InteractableMaterialManager))]
public class PowerInteractable : MonoBehaviour, IInteractable
{
    #region Serialized Fields

    [SerializeField] private string interactText;

    [SerializeField] private PowerScriptableObject power;

    [SerializeField] private bool destroyOnInteract = true;

    [SerializeField] private UnityEvent onInteraction;
    
    #endregion

    private bool _isMarkedForDestruction;

    #region Getters

    public InteractableMaterialManager InteractableMaterialManager { get; set; }
    
    public GameObject GameObject => gameObject;

    public bool IsInteractable => true;

    public bool HasOutline { get; set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    public InteractionIcon InteractionIcon => InteractionIcon.Pickup;
    
    public UnityEvent OnInteraction => onInteraction;

    #endregion

    private void Update()
    {
        if (_isMarkedForDestruction)
        {
            Destroy(gameObject);
            return;
        }
    }

    public void Interact(PlayerInteraction playerInteraction)
    {
        // Check if the player already has the power
        if (playerInteraction.Player.PlayerPowerManager.HasPower(power))
        {
            // Display a tooltip telling the player they already have the power
            JournalTooltipManager.Instance.AddTooltip(
                $"You already have {power.PowerName}!"
            );

            return;
        }

        // Give the power to the player
        playerInteraction.Player.PlayerPowerManager.AddPower(power);

        // Destroy the object if it is marked for destruction
        if (destroyOnInteract)
            _isMarkedForDestruction = true;
        
        // Invoke the onInteraction event
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