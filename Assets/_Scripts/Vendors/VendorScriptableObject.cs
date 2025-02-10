using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Vendor", menuName = "Vendor Information")]
public class VendorScriptableObject : ScriptableObject
{
    #region Serialized Fields

    [SerializeField] private string vendorName;

    [SerializeField] private VendorType vendorType;
    [SerializeField, Min(0)] private int upgradeCost = 250;
    [SerializeField, Min(0)] private int upgradeAmount = 50;
    
    [SerializeField] private PowerScriptableObject[] medicinePowers;
    [SerializeField] private PowerScriptableObject[] drugPowers;

    [SerializeField] private DialogueNode introDialogue;
    [SerializeField] private DialogueNode alreadyIntroducedDialogue;
    [SerializeField] private DialogueNode gossipDialogue;

    #endregion

    [NonSerialized] private bool _canBuyFromVendor = true;
    [NonSerialized] private bool _hasIntroduced = false;

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

    #endregion

    public void ResetVendorInformation()
    {
        // Reset the vendor information
        _canBuyFromVendor = true;
        _hasIntroduced = false;
    }
}