using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Vendor", menuName = "Vendor Information")]
public class VendorScriptableObject : ScriptableObject
{
    #region Serialized Fields

    [SerializeField] private string vendorName;

    [SerializeField] private PowerScriptableObject[] medicinePowers;
    [SerializeField] private PowerScriptableObject[] drugPowers;

    [SerializeField] private DialogueNode gossipDialogue;

    #endregion

    [NonSerialized] private bool _canBuyFromVendor = true;

    #region Getters

    public string VendorName => vendorName;

    public PowerScriptableObject[] MedicinePowers => medicinePowers;

    public PowerScriptableObject[] DrugPowers => drugPowers;

    public DialogueNode GossipDialogue => gossipDialogue;

    public bool CanBuyFromVendor
    {
        get => _canBuyFromVendor;
        set => _canBuyFromVendor = value;
    }

    #endregion
}