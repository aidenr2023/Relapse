using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VendorMenu : GameMenu
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

    private TokenManager<float>.ManagedToken _pauseToken;

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

    protected override void CustomOnEnable()
    {
    }

    protected override void CustomOnDisable()
    {
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
        {
            // If, the first med button is enabled, select it
            if (medButtons[0].isActiveAndEnabled)
                SetSelectedGameObject(medButtons[0].gameObject);
            // Else if the first drug button is enabled, select it
            else if (drugButtons[0].isActiveAndEnabled)
                SetSelectedGameObject(drugButtons[0].gameObject);
            // Otherwise, select the shop selected button
            else
                SetSelectedGameObject(shopSelectedButton);
        }
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

        // Enable buttons
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

        var backButton = shopSelectedButton.GetComponent<Button>();

        // Set up navigation
        for (var i = 0; i < _medPowers.Length; i++)
        {
            // Get the current power
            var power = _medPowers[i];

            // Get the current power button
            var button = medButtons[i];

            // If there is a button before this one, set up the up navigation
            if (i > 0)
                button.SetNavigationUp(medButtons[i - 1].Button);

            // If there is a button after this one, set up the down navigation
            if (i < _medPowers.Length - 1)
                button.SetNavigationDown(medButtons[i + 1].Button);
            // Otherwise, set the down navigation to the shop selected button
            else
                button.SetNavigationDown(backButton);

            // Set the left navigation to the back button
            button.SetNavigationLeft(backButton);

            // Set the right navigation to the first drug button (if it is enabled)
            if (drugButtons[0].isActiveAndEnabled)
                button.SetNavigationRight(drugButtons[0].Button);
        }

        for (var i = 0; i < _drugPowers.Length; i++)
        {
            // Get the current power
            var power = _drugPowers[i];

            // Get the current power button
            var button = drugButtons[i];

            // If there is a button before this one, set up the up navigation
            if (i > 0)
                button.SetNavigationUp(drugButtons[i - 1].Button);

            // If there is a button after this one, set up the down navigation
            if (i < _drugPowers.Length - 1)
                button.SetNavigationDown(drugButtons[i + 1].Button);
            // Otherwise, set the down navigation to the shop selected button
            else
                button.SetNavigationDown(backButton);

            // If there is a drug button, set the left navigation to the first drug button
            if (medButtons[0].isActiveAndEnabled)
                button.SetNavigationLeft(medButtons[0].Button);

            // Set the right navigation to the back button
            button.SetNavigationRight(backButton);
        }

        var nav = backButton.navigation;

        // Set the back button's up to be the first med or drug
        if (medButtons[0].isActiveAndEnabled)
            nav.selectOnUp = medButtons[0].Button;
        else if (drugButtons[0].isActiveAndEnabled)
            nav.selectOnUp = drugButtons[0].Button;

        backButton.navigation = nav;
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

        // Pause the game
        _pauseToken = TimeScaleManager.Instance.TimeScaleTokenManager.AddToken(0, -1, true);

        // Populate the shop
        PopulateShop();

        // Isolate the initial menu
        IsolateMenu(initialMenu);
    }

    public void EndVendor()
    {
        // Set this menu to inactive
        gameObject.SetActive(false);

        // Unpause the game
        TimeScaleManager.Instance.TimeScaleTokenManager.RemoveToken(_pauseToken);
    }

    public void SetSelectedGameObject(GameObject element)
    {
        // Set the selected element
        EventSystem.current.SetSelectedGameObject(element);
    }
}