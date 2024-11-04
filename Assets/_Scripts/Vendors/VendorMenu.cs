using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VendorMenu : MonoBehaviour
{
    #region Serialized Fields

    [Header("Powers Shop Screen")] [SerializeField]
    private PowerScriptableObject[] powers;

    [SerializeField] private VendorShopButton[] powerButtons;

    [SerializeField] private TMP_Text powerTypeText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text toleranceImpactText;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Gossip Dialogue")] [SerializeField]
    private DialogueNode gossipDialogue;

    [SerializeField] private DialogueUI dialogueUI;

    [Header("Menus")] [SerializeField] private GameObject initialMenu;
    [SerializeField] private GameObject powerMenu;
    [SerializeField] private GameObject gossipMenu;

    #endregion

    #region Serialized Fields

    public IEnumerable<PowerScriptableObject> Powers => powers;

    #endregion

    private void Start()
    {
        // Isolate the initial menu
        IsolateMenu(initialMenu);

        // Populate the shop
        PopulateShop();
    }

    public void StartDialogue()
    {
        dialogueUI.StartDialogue(gossipDialogue);
    }

    public void IsolateMenu(GameObject menu)
    {
        initialMenu.SetActive(false);
        powerMenu.SetActive(false);
        gossipMenu.SetActive(false);

        menu.SetActive(true);
    }

    public void PopulateShop()
    {
        // Reset all the power buttons
        foreach (var button in powerButtons)
            button.Reset();

        for (var i = 0; i < powers.Length; i++)
        {
            // Get the current power
            var power = powers[i];

            // Get the current power button
            var button = powerButtons[i];

            // Set the power
            button.SetPower(power);

            // Enable the button
            button.Enable();
        }
    }

    public void SetShopDescriptions(VendorShopButton button)
    {
        // Get the power
        var power = button.Power;

        // Set the power type
        powerTypeText.text = $"Power Type: {power.PowerType}";

        // Set the price
        priceText.text = $"Price: $PLACEHOLDER PRICE";

        // Set the tolerance impact
        toleranceImpactText.text = $"Tolerance Impact: {power.BaseToleranceMeterImpact}";

        // Set the cooldown
        cooldownText.text = $"Cooldown: {power.Cooldown:0.00}s";

        // Set the description
        descriptionText.text = power.Description;
    }
}