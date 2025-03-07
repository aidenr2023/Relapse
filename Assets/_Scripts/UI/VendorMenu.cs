﻿using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VendorMenu : GameMenu
{
    private const string VENDOR_SCENE_NAME = "VendorUIScene";

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

    [SerializeField] private TMP_Text upgradeInfoText;
    [SerializeField] private TMP_Text upgradeMoneyText;

    [Header("Gossip Dialogue")] [SerializeField]
    private DialogueUI dialogueUI;

    [Header("Menus")] [SerializeField] private GameObject initialMenu;
    [SerializeField] private GameObject powerMenu;
    [SerializeField] private GameObject upgradeMenu;
    [SerializeField] private GameObject gossipMenu;

    [Header("Navigation Control")] [SerializeField]
    private GameObject firstSelectedButton;

    [SerializeField] private GameObject shopSelectedButton;
    [SerializeField] private GameObject gossipSelectedButton;
    [SerializeField] private GameObject upgradeSelectedButton;

    [Header("Notifications")] [SerializeField]
    private Image shopNotification;

    [SerializeField] private Image gossipNotification;
    [SerializeField] private Image upgradeNotification;

    #endregion

    #region Private Fields

    private VendorScriptableObject _currentVendor;
    private PowerScriptableObject[] _medPowers;
    private PowerScriptableObject[] _drugPowers;

    private GameObject _isolatedMenu;
    private bool _playTutorialAfterClose;
    private PowerScriptableObject _purchasedPower;
    

    #endregion

    #region Getters

    public IEnumerable<PowerScriptableObject> MedPowers => _medPowers;
    public IEnumerable<PowerScriptableObject> DrugPowers => _drugPowers;

    public GameObject InitialMenu => initialMenu;

    public GameObject PowerMenu => powerMenu;

    public GameObject GossipMenu => gossipMenu;

    public VendorScriptableObject CurrentVendor => _currentVendor;

    private DialogueNode GossipDialogue => _currentVendor.GossipDialogue;

    #endregion

    protected override void CustomAwake()
    {
        // Set the instance
        Instance = this;
    }

    protected override void CustomActivate()
    {
    }

    protected override void CustomDeactivate()
    {
        // If the tutorial should be played after the menu is closed, play the tutorial
        if (_playTutorialAfterClose && _purchasedPower != null && _purchasedPower.Tutorial != null)
            TutorialScreen.Play(this, _purchasedPower.Tutorial, false);
        
        // Reset the flags
        _playTutorialAfterClose = false;
        _purchasedPower = null;
    }

    protected override void CustomDestroy()
    {
    }

    protected override void CustomStart()
    {
        // Isolate the initial menu
        IsolateMenu(initialMenu);

        // Set the bigger icon image to null
        SetBiggerIconImage(null);
    }

    protected override void CustomUpdate()
    {
        if (MenuManager.Instance.ActiveMenu == this &&
            eventSystem.currentSelectedGameObject == null)
        {
            if (_isolatedMenu == gossipMenu)
            {
                // Debug.Log(
                // $"Setting selected game object to {dialogueUI.NextButton.gameObject} {eventSystem.isActiveAndEnabled}");

                if (dialogueUI.CurrentDialogue is not DialogueChoiceNode)
                    eventSystem.SetSelectedGameObject(dialogueUI.NextButton.gameObject);
                else
                    eventSystem.SetSelectedGameObject(dialogueUI.Buttons[0].gameObject);
            }

            else if (_isolatedMenu == initialMenu)
                eventSystem.SetSelectedGameObject(firstSelectedButton);
        }

        // Update the notifications
        UpdateNotifications();
    }

    private void UpdateNotifications()
    {
        // Return if the current vendor is null
        if (_currentVendor == null)
            return;

        // If the player can buy from the vendor, show the shop notification
        shopNotification.enabled = _currentVendor.CanBuyFromVendor;

        // If the player can gossip with the vendor, show the gossip notification
        gossipNotification.enabled = !_currentVendor.HasGossipped;

        // If the player can upgrade with the vendor, show the upgrade notification
        upgradeNotification.enabled = Player.Instance.PlayerInventory.MoneyCount >= _currentVendor.UpgradeCost;

        // var scaleAdd = Mathf.Sin(Time.time * Mathf.PI * 2) * 0.1f;
        // var newScale = Vector3.one + new Vector3(scaleAdd, scaleAdd, 0);
        //
        // shopNotification.transform.localScale = newScale;
        // gossipNotification.transform.localScale = newScale;
        // upgradeNotification.transform.localScale = newScale;
    }

    public void StartDialogue()
    {
        _currentVendor.HasGossipped = true;
        dialogueUI.StartDialogue(GossipDialogue);
    }

    public void IsolateMenu(GameObject menu)
    {
        // If the menu is the power menu, but the player can no longer buy from the vendor, return
        if (menu == powerMenu && !_currentVendor.CanBuyFromVendor)
        {
            // TODO: Add a timer for spamming this message
            JournalTooltipManager.Instance.AddTooltip($"You can no longer buy from {_currentVendor.VendorName}.");
            return;
        }

        initialMenu.SetActive(false);
        powerMenu.SetActive(false);
        upgradeMenu.SetActive(false);
        gossipMenu.SetActive(false);

        menu.SetActive(true);
        _isolatedMenu = menu;

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
        {
            Debug.Log($"Setting selected game object to {gossipSelectedButton} {eventSystem.isActiveAndEnabled}");
            SetSelectedGameObject(gossipSelectedButton);
        }
        else if (menu == upgradeMenu)
        {
            SetUpUpgradeMenu();
            SetSelectedGameObject(upgradeSelectedButton);
        }
        else
            SetSelectedGameObject(firstSelectedButton);
    }

    private void SetUpUpgradeMenu()
    {
        var statToUpgrade = CurrentVendor.VendorType switch
        {
            VendorType.Doctor => "Health",
            VendorType.Dealer => "Toxicity",
            _ => "Unknown"
        };

        upgradeInfoText.text = $"Do you want to upgrade your maximum {statToUpgrade} for ${CurrentVendor.UpgradeCost}?";
        upgradeMoneyText.text = $"Money: ${Player.Instance.PlayerInventory.MoneyCount}";
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

        // // Set this menu to active
        // gameObject.SetActive(true);
        Activate();

        // Populate the shop
        PopulateShop();

        // // Isolate the initial menu
        // IsolateMenu(initialMenu);

        // If the vendor has intro dialogue, start the dialogue
        if (vendor.IntroDialogue != null && !vendor.HasIntroduced)
        {
            dialogueUI.StartDialogue(vendor.IntroDialogue);
            IsolateMenu(gossipMenu);
        }

        // If the vendor has already introduced themselves, start the already introduced dialogue
        else if (vendor.AlreadyIntroducedDialogue != null && vendor.HasIntroduced)
        {
            dialogueUI.StartDialogue(vendor.AlreadyIntroducedDialogue);
            IsolateMenu(gossipMenu);
        }

        // Otherwise, isolate the initial menu
        else
            IsolateMenu(initialMenu);

        // Set the introduced flag in the vendor information
        vendor.HasIntroduced = true;
        
        // Reset the play tutorial after close flag
        _playTutorialAfterClose = false;
        _purchasedPower = null;
    }

    public void EndVendor()
    {
        // // Set this menu to inactive
        // gameObject.SetActive(false);
        Deactivate();
    }

    public void BuyUpgrade()
    {
        var playerInventory = Player.Instance.PlayerInventory;

        // If the player does not have enough money, return
        if (playerInventory.MoneyCount < CurrentVendor.UpgradeCost)
        {
            JournalTooltipManager.Instance.AddTooltip("You do not have enough money to buy this upgrade.");
            return;
        }

        // Deduct the cost of the upgrade from the player's money
        playerInventory.RemoveItem(playerInventory.MoneyObject, CurrentVendor.UpgradeCost);

        var playerInfo = Player.Instance.PlayerInfo;

        switch (CurrentVendor.VendorType)
        {
            // If this vendor is a doctor, increase the player's max health
            case VendorType.Doctor:
                Player.Instance.PlayerInfo.ChangeMaxHealth(playerInfo.MaxHealth + CurrentVendor.UpgradeAmount);
                JournalTooltipManager.Instance.AddTooltip(
                    $"You have bought a health upgrade. Your max health is now {playerInfo.MaxHealth}.");
                break;

            // If this vendor is a dealer, increase the player's toxicity
            case VendorType.Dealer:
                Player.Instance.PlayerInfo.ChangeMaxToxicity(playerInfo.MaxTolerance + CurrentVendor.UpgradeAmount);
                JournalTooltipManager.Instance.AddTooltip(
                    $"You have bought a toxicity upgrade. Your max toxicity is now {playerInfo.MaxTolerance}.");
                break;
        }
    }
    
    public void SetPurchasedPower(PowerScriptableObject power)
    {
        _purchasedPower = power;
        _playTutorialAfterClose = true;
    }

    public void SetSelectedGameObject(GameObject element)
    {
        // Set the selected element
        // EventSystem.current.SetSelectedGameObject(element);
        eventSystem.SetSelectedGameObject(element);
    }

    public override void OnBackPressed()
    {
        // If the initial menu is active, end the vendor
        if (initialMenu.activeSelf)
            EndVendor();
        else if (powerMenu.activeSelf)
            IsolateMenu(initialMenu);
    }

    public static IEnumerator LoadVendorMenu()
    {
        // Load the vendor UI scene
        SceneManager.LoadScene(VENDOR_SCENE_NAME, LoadSceneMode.Additive);

        // Wait while the instance is null
        yield return new WaitUntil(() => Instance != null);
    }
}