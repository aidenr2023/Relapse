using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ApartmentAbilityRestorer : MonoBehaviour, IInteractable
{
    #region Serialized Fields

    [SerializeField] private string interactText = "<i>Restore Abilities</i>";

    [SerializeField] private string[] tooltipTexts =
    {
        "You can now sprint and jump!"
    };

    [SerializeField] private UnityEvent onInteraction;

    #endregion

    #region Private Fields

    private bool _isMarkedForDeletion;

    #endregion

    #region Getters

    public bool IsInteractable { get; private set; } = true;

    public GameObject GameObject => gameObject;

    public bool HasOutline { get; private set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    public InteractionIcon InteractionIcon => InteractionIcon.Pickup;

    public UnityEvent OnInteraction => onInteraction;

    #endregion

    private void Update()
    {
        if (_isMarkedForDeletion)
            Destroy(gameObject);
    }

    public void Interact(PlayerInteraction playerInteraction)
    {
        // Set the has outline flag to false
        HasOutline = false;

        // restore the player's abilities

        // Get the IPlayerController as a PlayerMovementV2
        var pm = playerInteraction?.Player?.PlayerController as PlayerMovementV2;

        // return if the player controller is not a PlayerMovementV2
        if (pm == null)
            return;

        // Restore the player's abilities
        pm.BasicPlayerMovement.SetCanJumpWithoutPower(true);
        pm.BasicPlayerMovement.SetCanSprintWithoutPower(true);

        // Add the tooltips
        foreach (var tip in tooltipTexts)
            JournalTooltipManager.Instance?.AddTooltip(tip);

        // Destroy the game object
        _isMarkedForDeletion = true;

        // Set has outline to false
        HasOutline = false;

        // Set is interactable to false
        IsInteractable = false;
        
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