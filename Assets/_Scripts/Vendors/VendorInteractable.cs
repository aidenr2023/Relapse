using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(UniqueId), typeof(InteractableMaterialManager))]
public class VendorInteractable : MonoBehaviour, IVendorInteractable, ILevelLoaderInfo
{
    #region Serialized Fields

    [SerializeField] private VendorScriptableObject vendorInformation;

    [SerializeField] private UnityEvent onInteraction;

    [SerializeField] private UnityEvent afterTalkingOnce;

    #endregion

    public bool HasTalkedOnce { get; private set; }

    #region Getters

    public InteractableMaterialManager InteractableMaterialManager { get; set; }

    public GameObject GameObject => gameObject;

    public bool IsInteractable { get; set; } = true;

    public bool HasOutline { get; set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    public InteractionIcon InteractionIcon => InteractionIcon.Action;

    public UnityEvent OnInteraction => onInteraction;

    #endregion

    public void Interact(PlayerInteraction playerInteraction)
    {
        // Get the vendor menu instance & activate the shop
        VendorMenu.Instance.StartVendor(vendorInformation, this);

        // Invoke the on interaction event
        onInteraction.Invoke();
    }

    public void LookAtUpdate(PlayerInteraction playerInteraction)
    {
    }

    public string InteractText(PlayerInteraction playerInteraction)
    {
        return $"Talk to {vendorInformation.VendorName}";
    }

    public void InvokeAfterTalkingOnce()
    {
        if (HasTalkedOnce)
            return;

        afterTalkingOnce.Invoke();
        HasTalkedOnce = true;

        Debug.Log($"INVOKING THE EVENT AFTER TALKING ONCE");
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
    private const string HAS_INTRODUCED = "_hasIntroduced";

    public void LoadData(LevelLoader levelLoader)
    {
        // Reset the vendor information
        vendorInformation.ResetVendorInformation();

        // Load whether the player can buy from the vendor
        if (levelLoader.TryGetDataFromMemory(UniqueId, CAN_BUY_FROM_VENDOR, out bool canBuyFromVendor))
            vendorInformation.CanBuyFromVendor = canBuyFromVendor;

        // Load whether the player has introduced to the vendor
        if (levelLoader.TryGetDataFromMemory(UniqueId, HAS_INTRODUCED, out bool hasIntroduced))
            vendorInformation.HasIntroduced = hasIntroduced;
    }

    public void SaveData(LevelLoader levelLoader)
    {
        // Create a boolean to save whether the player can buy from the vendor
        // Save the data
        var canBuyFromVendorData = vendorInformation.CanBuyFromVendor;
        levelLoader.AddDataToMemory(UniqueId, new DataInfo(CAN_BUY_FROM_VENDOR, canBuyFromVendorData));

        // Create a boolean to save whether the player has introduced to the vendor
        // Save the data
        var hasIntroducedData = vendorInformation.HasIntroduced;
        levelLoader.AddDataToMemory(UniqueId, new DataInfo(HAS_INTRODUCED, hasIntroducedData));
    }

    #endregion
}