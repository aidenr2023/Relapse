using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class HordeModeVendorInteractable : MonoBehaviour, IVendorInteractable
{
    #region Serialized Fields

    [SerializeField] private VendorScriptableObject originalVendorInformation;

    [SerializeField] private PowerListVariable allPowers;
    [SerializeField] private PowerListVariable playerEquippedPowers;
    [SerializeField, Min(1)] private int powerCount = 4;

    [SerializeField] private UnityEvent onInteraction;

    [SerializeField] private UnityEvent afterTalkingOnce;

    #endregion

    private VendorScriptableObject vendorInformation;

    #region Getters

    public bool HasTalkedOnce { get; private set; }

    public InteractableMaterialManager InteractableMaterialManager { get; set; }

    public GameObject GameObject => gameObject;

    public bool IsInteractable { get; set; } = true;

    public bool HasOutline { get; set; }

    public HashSet<Material> OutlineMaterials { get; } = new();

    public InteractionIcon InteractionIcon => InteractionIcon.Action;

    public UnityEvent OnInteraction => onInteraction;

    #endregion

    private void Awake()
    {
        // Create a new instance of the vendor information
        vendorInformation = VendorScriptableObject.CreateInstance(originalVendorInformation);

        // Randomize the powers
        RandomizePowers(powerCount);
    }

    private void RandomizePowers(int numPowers)
    {
        var powersHashSet = new HashSet<PowerScriptableObject>(allPowers.value);

        var selection = new List<PowerScriptableObject>();

        // If the player's current power count is >= 4, set the numPowers to 0
        if (playerEquippedPowers.value.Count >= 4)
            numPowers = 0;
        
        for (var i = 0; i < numPowers; i++)
        {
            // Get a random power from the hash set
            var powers = powersHashSet.ToArray();
            var randomIndex = UnityEngine.Random.Range(0, powers.Length);
            var power = powers[randomIndex];

            // Remove the power from the hash set to avoid duplicates
            powersHashSet.Remove(power);

            // Add the power to the selection list
            selection.Add(power);
        }

        // Force initialization of the vendor information
        vendorInformation.ForceInitialize(VendorType.Doctor, selection);
    }

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

    [ContextMenu("Reinitialize")]
    public void Reinitialize()
    {
        // Reinitialize the vendor information
        vendorInformation.ResetVendorInformation();

        // Randomize the powers again
        RandomizePowers(powerCount);
    }
}