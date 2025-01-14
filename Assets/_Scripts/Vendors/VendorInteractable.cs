using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UniqueId))]
public class VendorInteractable : MonoBehaviour, IInteractable, ILevelLoaderInfo
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

    #region ILevelLoaderInfo

    private UniqueId _uniqueId;

    public UniqueId UniqueId
    {
        get
        {
            if (_uniqueId == null)
                _uniqueId = GetComponent<UniqueId>();

            return _uniqueId;
        }
    }

    private const string CAN_BUY_FROM_VENDOR = "_canBuyFromVendor";

    public void LoadData(LevelLoader levelLoader)
    {
        // Load whether the player can buy from the vendor
        if (levelLoader.TryGetDataFromMemory(UniqueId, CAN_BUY_FROM_VENDOR, out bool canBuyFromVendor))
            vendorInformation.CanBuyFromVendor = canBuyFromVendor;
    }

    public void SaveData(LevelLoader levelLoader)
    {
        // Create a boolean to save whether the player can buy from the vendor
        // Save the data
        var canBuyFromVendorData = vendorInformation.CanBuyFromVendor;
        levelLoader.AddDataToMemory(UniqueId, new DataInfo(CAN_BUY_FROM_VENDOR, canBuyFromVendorData));
    }

    #endregion
}