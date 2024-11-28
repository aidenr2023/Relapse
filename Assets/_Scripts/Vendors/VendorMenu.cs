using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VendorMenu : MonoBehaviour
{
    public static VendorMenu Instance { get; private set; }

    #region Serialized Fields

    [Header("Powers Shop Screen")] [SerializeField]
    private VendorShopButton[] medButtons;

    [SerializeField] private VendorShopButton[] drugButtons;

    [SerializeField] private TMP_Text powerTypeText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text toleranceImpactText;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Gossip Dialogue")] [SerializeField]
    private DialogueUI dialogueUI;

    [Header("Menus")] [SerializeField] private GameObject initialMenu;
    [SerializeField] private GameObject powerMenu;
    [SerializeField] private GameObject gossipMenu;

    #endregion

    private VendorScriptableObject _currentVendor;
    private PowerScriptableObject[] _medPowers;
    private PowerScriptableObject[] _drugPowers;

    #region Getters

    public IEnumerable<PowerScriptableObject> MedPowers => _medPowers;
    public IEnumerable<PowerScriptableObject> DrugPowers => _drugPowers;

    public GameObject InitialMenu => initialMenu;

    public GameObject PowerMenu => powerMenu;

    public GameObject GossipMenu => gossipMenu;

    public bool IsVendorActive => gameObject.activeSelf;

    public VendorScriptableObject CurrentVendor => _currentVendor;

    private DialogueNode GossipDialogue => _currentVendor.GossipDialogue;

    #endregion

    private void Awake()
    {
        // Set the instance
        Instance = this;
    }

    private void Start()
    {
        // Isolate the initial menu
        IsolateMenu(initialMenu);
    }

    public void StartDialogue()
    {
        dialogueUI.StartDialogue(GossipDialogue);
    }

    public void IsolateMenu(GameObject menu)
    {
        // If the menu is the power menu, but the player can no longer buy from the vendor, return
        if (menu == powerMenu && !_currentVendor.CanBuyFromVendor)
        {
            JournalTooltipManager.Instance.AddTooltip($"You can no longer buy from {_currentVendor.VendorName}.");
            return;
        }

        initialMenu.SetActive(false);
        powerMenu.SetActive(false);
        gossipMenu.SetActive(false);

        menu.SetActive(true);
    }

    private void PopulateShop()
    {
        // Reset all the power buttons
        foreach (var button in medButtons)
            button.Reset();

        // Reset all the power buttons
        foreach (var button in drugButtons)
            button.Reset();

        for (var i = 0; i < _medPowers.Length; i++)
        {
            // Get the current power
            var power = _medPowers[i];

            // Get the current power button
            var button = medButtons[i];

            // Set the power
            button.SetPower(power);

            // Enable the button
            button.Enable();
        }

        for (var i = 0; i < _drugPowers.Length; i++)
        {
            // Get the current power
            var power = _drugPowers[i];

            // Get the current power button
            var button = drugButtons[i];

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

    public void StartVendor(VendorScriptableObject vendor)
    {
        // Set the current vendor
        _currentVendor = vendor;

        // Set the powers
        _medPowers = vendor.MedicinePowers;
        _drugPowers = vendor.DrugPowers;

        // Set this menu to active
        gameObject.SetActive(true);

        // Set the cursor to visible
        // InputManager.Instance.SetCursorState(true);

        // Pause the game
        Time.timeScale = 0;

        // Populate the shop
        PopulateShop();

        // Isolate the initial menu
        IsolateMenu(initialMenu);
    }

    public void EndVendor()
    {
        // Set this menu to inactive
        gameObject.SetActive(false);

        // Set the cursor to invisible
        // InputManager.Instance.SetCursorState(false);

        // Unpause the game
        // TODO: Connect with timescale manager
        Time.timeScale = 1;
    }
}