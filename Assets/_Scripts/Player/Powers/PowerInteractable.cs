using System;
using System.Collections.Generic;
using UnityEngine;

public class PowerInteractable : MonoBehaviour, IInteractable
{
    #region Serialized Fields

    [SerializeField] private string interactText;

    [SerializeField] private PowerScriptableObject power;

    [SerializeField] private bool destroyOnInteract = true;

    #endregion

    private bool _isMarkedForDestruction;

    #region Getters

    public GameObject GameObject => gameObject;

    public bool IsInteractable => true;

    public bool HasOutline { get; set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    public InteractionIcon InteractionIcon => InteractionIcon.Pickup;

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
    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        return interactText;
    }
}