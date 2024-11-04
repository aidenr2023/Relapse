using System;
using System.Collections.Generic;
using UnityEngine;

public class VendorMenu : MonoBehaviour
{
    #region Serialized Fields

    [Header("Powers Shop Screen")] [SerializeField]
    private PowerScriptableObject[] powers;

    [SerializeField] private VendorShopButton[] powerButtons;

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
}