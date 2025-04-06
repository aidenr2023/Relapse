using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Vendor", menuName = "Vendor Information")]
public class VendorScriptableObject : ScriptableObject
{
    #region Serialized Fields

    [SerializeField] private string vendorName;

    [SerializeField] private VendorType vendorType;
    [SerializeField, Min(0)] private int upgradeCost = 250;
    [SerializeField, Min(0)] private int upgradeAmount = 50;
    [SerializeField, Min(0)] private int maxUpgrades = 3;

    [SerializeField] private PowerScriptableObject[] medicinePowers;
    [SerializeField] private PowerScriptableObject[] drugPowers;

    [SerializeField] private DialogueNode introDialogue;
    [SerializeField] private DialogueNode alreadyIntroducedDialogue;
    [SerializeField] private DialogueNode gossipDialogue;

    #endregion

    [NonSerialized] private bool _canBuyFromVendor = true;
    [NonSerialized] private bool _hasIntroduced = false;
    [NonSerialized] private bool _hasGossipped = false;
    [NonSerialized] private int _upgradesRemaining;

    [NonSerialized] private bool _hasInitializedUpgradesRemaining = false;

    #region Getters

    public string VendorName => vendorName;

    public VendorType VendorType => vendorType;

    public int UpgradeCost => upgradeCost;

    public int UpgradeAmount => upgradeAmount;

    public PowerScriptableObject[] MedicinePowers => medicinePowers;

    public PowerScriptableObject[] DrugPowers => drugPowers;

    public DialogueNode IntroDialogue => introDialogue;

    public DialogueNode AlreadyIntroducedDialogue => alreadyIntroducedDialogue;

    public DialogueNode GossipDialogue => gossipDialogue;

    public bool CanBuyFromVendor
    {
        get => _canBuyFromVendor;
        set => _canBuyFromVendor = value;
    }

    public bool HasIntroduced
    {
        get => _hasIntroduced;
        set => _hasIntroduced = value;
    }

    public bool HasGossipped
    {
        get => _hasGossipped;
        set => _hasGossipped = value;
    }

    public int UpgradesRemaining
    {
        get
        {
            if (_hasInitializedUpgradesRemaining)
                return _upgradesRemaining;

            _upgradesRemaining = maxUpgrades;
            _hasInitializedUpgradesRemaining = true;
            return _upgradesRemaining;
        }
        set => _upgradesRemaining = value;
    }

    #endregion

    public void ResetVendorInformation()
    {
        // Reset the vendor information
        _canBuyFromVendor = true;
        _hasIntroduced = false;
        _hasGossipped = false;
        _upgradesRemaining = maxUpgrades;
    }

    public void ForceInitialize(VendorType type, IEnumerable<PowerScriptableObject> powers)
    {
        vendorType = type;

        // Force initialize the vendor information
        ResetVendorInformation();

        // Create a hash set to store the medicine powers
        var medicinePowersHashSet = new HashSet<PowerScriptableObject>();

        // Create a hash set to store the drug powers
        var drugPowersHashSet = new HashSet<PowerScriptableObject>();

        // Add the powers to the hash sets
        foreach (var power in powers)
        {
            if (power == null)
                continue;

            switch (power.PowerType)
            {
                case PowerType.Drug:
                    drugPowersHashSet.Add(power);
                    break;
                case PowerType.Medicine:
                    medicinePowersHashSet.Add(power);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        // Convert the hash sets to arrays
        medicinePowers = medicinePowersHashSet.ToArray();
        drugPowers = drugPowersHashSet.ToArray();
    }

    public static VendorScriptableObject CreateInstance(VendorScriptableObject original)
    {
        // Create a new instance of the vendor information
        var vendorInformation = ScriptableObject.CreateInstance<VendorScriptableObject>();

        // Copy the values from the original instance
        vendorInformation.vendorName = original.vendorName;
        vendorInformation.vendorType = original.vendorType;
        vendorInformation.upgradeCost = original.upgradeCost;
        vendorInformation.upgradeAmount = original.upgradeAmount;
        vendorInformation.medicinePowers = original.medicinePowers;
        vendorInformation.drugPowers = original.drugPowers;
        vendorInformation.introDialogue = original.introDialogue;
        vendorInformation.alreadyIntroducedDialogue = original.alreadyIntroducedDialogue;
        vendorInformation.gossipDialogue = original.gossipDialogue;

        return vendorInformation;
    }
}