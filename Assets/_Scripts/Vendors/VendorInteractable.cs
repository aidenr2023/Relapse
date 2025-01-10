using System.Collections.Generic;
using UnityEngine;

public class VendorInteractable : MonoBehaviour, IInteractable
{
    #region Serialized Fields

    [SerializeField] private VendorScriptableObject vendorInformation;

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    public bool IsInteractable { get; set; } = true;

    public bool HasOutline { get; set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    public InteractionIcon InteractionIcon => InteractionIcon.Action;

    #endregion

    public void Interact(PlayerInteraction playerInteraction)
    {
        // Get the vendor menu instance & activate the shop
        VendorMenu.Instance.StartVendor(vendorInformation);
        Debug.Log($"Starting vendor with information: {vendorInformation.VendorName}");
    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        return $"Talk to {vendorInformation.VendorName}";
    }
}