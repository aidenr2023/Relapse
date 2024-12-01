using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    [SerializeField] private Image biggerPowerImage;

    [Header("Gossip Dialogue")] [SerializeField]
    private DialogueUI dialogueUI;

    [Header("Menus")] [SerializeField] private GameObject initialMenu;
    [SerializeField] private GameObject powerMenu;
    [SerializeField] private GameObject gossipMenu;

    [Header("Navigation Control")] [SerializeField]
    private GameObject firstSelectedButton;

    [SerializeField] private GameObject shopSelectedButton;
    [SerializeField] private GameObject gossipSelectedButton;

    #endregion

    #region Private Fields

    private VendorScriptableObject _currentVendor;
    private PowerScriptableObject[] _medPowers;
    private PowerScriptableObject[] _drugPowers;

    #endregion

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

        // Set the bigger icon image to null
        SetBiggerIconImage(null);
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

        // Set the selected button
        if (menu == powerMenu)
            SetSelectedGameObject(shopSelectedButton);
        else if (menu == gossipMenu)
            SetSelectedGameObject(gossipSelectedButton);
        else
            SetSelectedGameObject(firstSelectedButton);
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

        // Set the bigger icon image
        SetBiggerIconImage(power);
    }

    private void SetBiggerIconImage(PowerScriptableObject power)
    {
        // If the power has an image, set the image
        if (power != null && power.Icon != null)
        {
            // Enable the image
            biggerPowerImage.enabled = true;

            biggerPowerImage.sprite = power.Icon;
            biggerPowerImage.preserveAspect = true;
        }
        else
        {
            // Otherwise, set the image to null
            biggerPowerImage.sprite = null;

            // Disable the image
            biggerPowerImage.enabled = false;
        }
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

    public void SetSelectedGameObject(GameObject element)
    {
        // Set the selected element
        EventSystem.current.SetSelectedGameObject(element);
    }
}