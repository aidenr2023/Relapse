using System;
using UnityEngine;

public class ReticleUIManager : MonoBehaviour
{
    [SerializeField] private CanvasGroup reticleCanvasGroup;
    [SerializeField] private CanvasGroup actionCanvasGroup;
    [SerializeField] private CanvasGroup pickupCanvasGroup;

    [SerializeField, Min(0)] private float opacityLerpAmount = .1f;

    private InteractionIcon _currentInteractionIcon;

    private CanvasGroup[] AllCanvasGroups => new[] { reticleCanvasGroup, actionCanvasGroup, pickupCanvasGroup };

    private void Start()
    {
        // Update the interaction Icon
        UpdateInteractionIcon();

        // Get all the canvas groups
        var canvasGroups = AllCanvasGroups;

        // Loop through all the canvas groups
        // Set the alpha of the canvas group to 0
        foreach (var canvasGroup in canvasGroups)
            canvasGroup.alpha = 0;

        // Set the alpha of the current interaction icon to 1
        GetCanvasGroup(_currentInteractionIcon).alpha = 1;
    }

    private void Update()
    {
        // Update the interaction icon
        UpdateInteractionIcon();

        // Update the reticle's opacity
        UpdateReticleOpacity();
    }


    private void UpdateInteractionIcon()
    {
        // Check if the player is looking at an interactable
        var player = Player.Instance;

        if (player == null)
        {
            _currentInteractionIcon = InteractionIcon.None;
            return;
        }

        // Set the current interaction icon to the player's current interaction icon
        _currentInteractionIcon = player.PlayerInteraction.CurrentInteractionIcon;
    }

    private void UpdateReticleOpacity()
    {
        // Determine the current canvas group
        var currentCanvasGroup = GetCanvasGroup(_currentInteractionIcon);

        // Create an array of all the canvas groups
        var canvasGroups = AllCanvasGroups;

        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.deltaTime / defaultFrameTime;

        // Loop through all the canvas groups
        foreach (var canvasGroup in canvasGroups)
        {
            // If the canvas group is the current canvas group, lerp the alpha to 1
            // Otherwise, lerp the alpha to 0
            var targetOpacity = canvasGroup == currentCanvasGroup ? 1 : 0;

            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetOpacity, opacityLerpAmount * frameAmount);
        }
    }

    private CanvasGroup GetCanvasGroup(InteractionIcon interactionIcon)
    {
        return interactionIcon switch
        {
            InteractionIcon.None => reticleCanvasGroup,
            InteractionIcon.Action => actionCanvasGroup,
            InteractionIcon.Pickup => pickupCanvasGroup,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}