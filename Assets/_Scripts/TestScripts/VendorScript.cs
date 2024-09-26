using UnityEngine;

public class VendorScript : MonoBehaviour, IInteractable
{
    private int selectionIndex = 0;
    
    [SerializeField] private PowerScriptableObject[] availablePowers;

    #region IInteractable

    public GameObject GameObject => gameObject;

    public string InteractText => "";
    public bool IsCurrentlyLookedAt { get; set; }
    public bool IsInteractable => true;

    #endregion

    public void Interact(PlayerInteraction playerInteraction)
    {
        // Add the power at the selected index to the player's inventory
        playerInteraction.Player.PlayerPowerManager.AddPower(availablePowers[selectionIndex]);
    }
}