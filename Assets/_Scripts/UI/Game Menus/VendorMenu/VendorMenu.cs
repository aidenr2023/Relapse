using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class VendorMenu : GameMenu
{
    public static VendorMenu Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private FloatReference playerMaxHealth;
    [SerializeField] private FloatReference playerCurrentHealth;
    [SerializeField] private FloatReference playerMaxToxicity;
    [SerializeField] private FloatReference playerCurrentToxicity;
    [SerializeField] private InventoryVariable playerInventory;
    [SerializeField] private PowerListReference allPowers;
    [SerializeField] private PowerListReference playerPowers;

    [Header("Powers Shop Screen")] [SerializeField]
    private NewVendorShopButton[] medButtons;

    [SerializeField] private NewVendorShopButton[] drugButtons;

    [SerializeField] private TMP_Text powerTypeText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text toleranceImpactText;
    [SerializeField] private TMP_Text cooldownText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image biggerPowerImage;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private TMP_Text powerNameText;
    [SerializeField] private Tutorial relapseTutorial;

    [Header("Upgrades"), SerializeField] private TMP_Text upgradeInfoText;
    [SerializeField] private TMP_Text upgradeMoneyText;
    [SerializeField] private GameObject mainUpgradePanel;
    [SerializeField] private GameObject upgradeConfirmationPanel;
    
    [SerializeField] private TMP_Text upgradesRemainingText;
    [SerializeField] private TMP_Text upgradeMoneyText2;
    
    
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
    [SerializeField] private GameObject upgradeConfirmationSelectedButton;

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

    private IVendorInteractable _vendorInteractable;
    private VendorType _currentUpgradeType;

    private Coroutine _changeClipCoroutine;

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
        {
            // TutorialScreen.Play(this, _purchasedPower.Tutorial, false);
            TutorialScreen.Play(this, relapseTutorial, false);
        }

        // Invoke the event to run after talking once
        if (_vendorInteractable != null && _purchasedPower != null)
            _vendorInteractable.InvokeAfterTalkingOnce();
        
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
        if (MenuManager.Instance.ActiveMenu == this && eventSystem.currentSelectedGameObject == null)
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
        upgradeNotification.enabled = playerInventory.MoneyCount >= _currentVendor.UpgradeCost &&
                                      _currentVendor.UpgradesRemaining > 0;

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

        // Disable the video player
        videoPlayer.enabled = false;

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

            // Enable the video player
            videoPlayer.enabled = true;
        }
        else if (menu == gossipMenu)
        {
            Debug.Log($"Setting selected game object to {gossipSelectedButton} {eventSystem.isActiveAndEnabled}");
            SetSelectedGameObject(gossipSelectedButton);
        }
        else if (menu == upgradeMenu)
        {
            // Update the upgrade text
            UpdateUpgradeText();
            
            // Disable the upgrade confirmation panel
            upgradeConfirmationPanel.SetActive(false);

            // Enable the main upgrade panel
            mainUpgradePanel.SetActive(true);

            SetSelectedGameObject(upgradeSelectedButton);
        }
        else
            SetSelectedGameObject(firstSelectedButton);
    }

    private void SetUpConfirmationPanel()
    {
        var statToUpgrade = _currentUpgradeType switch
        {
            VendorType.Doctor => "Health",
            VendorType.Dealer => "Toxicity",
            _ => "Unknown"
        };

        upgradeInfoText.text = $"Do you want to upgrade your maximum {statToUpgrade} for ${CurrentVendor.UpgradeCost}?";
        upgradeMoneyText.text = $"Money: ${playerInventory.MoneyCount}";
    }

    private void PopulateShop()
    {
        var allVendorPowers = new HashSet<PowerScriptableObject>(_currentVendor.MedicinePowers);
        allVendorPowers.UnionWith(_currentVendor.DrugPowers);

        var alreadyBoughtPowers = new HashSet<PowerScriptableObject>(playerPowers.Value);

        // Remove all the powers the player currently has
        foreach (var power in alreadyBoughtPowers)
            allVendorPowers.Remove(power);

        // Create a list for each type of power
        var neuroList = new List<PowerScriptableObject>();
        var vitalList = new List<PowerScriptableObject>();

        // Go through the vendors powers first
        foreach (var power in allVendorPowers)
            SetButtonPower(power, neuroList, vitalList);

        // Create a hash set of ALL the powers
        var remainingPowers = new HashSet<PowerScriptableObject>(allPowers.Value);

        // Remove the player powers from the all powers hash set
        foreach (var power in alreadyBoughtPowers)
        {
            remainingPowers.Remove(power);

            SetButtonPower(power, neuroList, vitalList);
        }

        // Remove the vendor powers from the all powers hash set
        foreach (var power in allVendorPowers)
            remainingPowers.Remove(power);

        // For each of the remaining powers, set the button power
        foreach (var power in remainingPowers)
            SetButtonPower(power, neuroList, vitalList);
    }

    private void SetButtonPower(
        PowerScriptableObject power,
        List<PowerScriptableObject> neuroList,
        List<PowerScriptableObject> vitalList
    )
    {
        switch (power.PowerType)
        {
            case PowerType.Medicine:
                medButtons[vitalList.Count].Initialize(power, _currentVendor.MedicinePowers.Contains(power));

                vitalList.Add(power);

                break;

            case PowerType.Drug:
                drugButtons[neuroList.Count].Initialize(power, _currentVendor.DrugPowers.Contains(power));

                neuroList.Add(power);

                break;
        }
    }

    public void SetShopDescriptions(NewVendorShopButton button)
    {
        // Get the power
        var power = button.Power;

        // Set the power name text
        powerNameText.text = power.PowerName;

        // Set the power type
        powerTypeText.text = power.PowerType switch
        {
            PowerType.Medicine => "Power Type: Vital",
            PowerType.Drug => "Power Type: Neuro",
            _ => "Unknown"
        };

        // Set the price
        priceText.text = $"Price: $PLACEHOLDER PRICE";

        // Set the tolerance impact
        toleranceImpactText.text = $"Toxicity Impact: {power.BaseToleranceMeterImpact}";

        // Set the cooldown
        cooldownText.text = $"Cooldown: {power.Cooldown:0.00} Seconds";

        // Set the description
        descriptionText.text = power.Description;

        VideoClip clip = null;
        
        // Set the video to the first video
        try
        {
            clip = power.Tutorial.TutorialPages[0].VideoClip;
        }
        catch (Exception e)
        {
            // Log the exception
            Debug.LogError(e);
        }

        // Change the video clip
        if (_changeClipCoroutine != null)
            StopCoroutine(_changeClipCoroutine);
        _changeClipCoroutine = StartCoroutine(ChangeVideoClip(clip));

        // Set the bigger icon image
        SetBiggerIconImage(power);
    }

    private IEnumerator ChangeVideoClip(VideoClip clip)
    {
        // Stop the video player
        videoPlayer.Stop();

        // Wait a frame
        yield return null;

        videoPlayer.time = 0;
        videoPlayer.clip = clip;

        // Wait another frame
        yield return null;

        // Start the video player if the clip is not null
        if (clip != null)
            videoPlayer.Play();
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

    public void StartVendor(VendorScriptableObject vendor, IVendorInteractable vendorInteractable)
    {
        // Set the current vendor
        _currentVendor = vendor;
        _vendorInteractable = vendorInteractable;

        // Set the powers
        _medPowers = vendor.MedicinePowers;
        _drugPowers = vendor.DrugPowers;

        // Set this menu to active
        Activate();

        // Populate the shop
        PopulateShop();

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
        // Set this menu to inactive
        Deactivate();
    }
    
    private void UpdateUpgradeText()
    {
        // Update the upgrades remaining text
        upgradesRemainingText.text = $"Upgrades Remaining: {CurrentVendor.UpgradesRemaining}";

        // Update the money text
        upgradeMoneyText2.text = $"You Have: ${playerInventory.MoneyCount}";
    }

    public void BuyUpgrade() => BuyUpgrade(_currentUpgradeType);

    public void SetUpgradeTypeHealth()
    {
        _currentUpgradeType = VendorType.Doctor;
        UpdateUpgradeText();
        SetUpConfirmationPanel();
    }

    public void SetUpgradeTypeToxicity()
    {
        _currentUpgradeType = VendorType.Dealer;
        UpdateUpgradeText();
        SetUpConfirmationPanel();
    }

    private void BuyUpgrade(VendorType vendorType)
    {
        // If the player does not have enough money, return
        if (playerInventory.MoneyCount < CurrentVendor.UpgradeCost)
        {
            JournalTooltipManager.Instance.AddTooltip("You do not have enough money to buy this upgrade!");
            return;
        }

        // If the vendor has no upgrades remaining, return
        if (CurrentVendor.UpgradesRemaining <= 0)
        {
            JournalTooltipManager.Instance.AddTooltip("You have no upgrades remaining!");
            return;
        }

        // Deduct the cost of the upgrade from the player's money
        playerInventory.RemoveItem(playerInventory.MoneyObject, CurrentVendor.UpgradeCost);

        // Decrement the upgrades remaining
        CurrentVendor.UpgradesRemaining--;

        // Apply the upgrade based on the vendor type
        switch (vendorType)
        {
            // If this vendor is a doctor, increase the player's max health
            case VendorType.Doctor:
                ChangeMaxValue(playerCurrentHealth, playerMaxHealth, CurrentVendor.UpgradeAmount);
                JournalTooltipManager.Instance.AddTooltip(
                    $"You have bought a health upgrade. Your max health is now {playerMaxHealth.Value}.");
                break;

            // If this vendor is a dealer, increase the player's toxicity
            case VendorType.Dealer:
                ChangeMaxValue(playerCurrentToxicity, playerMaxToxicity, CurrentVendor.UpgradeAmount);
                JournalTooltipManager.Instance.AddTooltip(
                    $"You have bought a toxicity upgrade. Your max toxicity is now {playerMaxToxicity.Value}.");
                break;
        }
        
        // Update the upgrade text
        UpdateUpgradeText();
    }

    public void ShowUpgradeConfirmationPanel()
    {
        // Disable the main upgrade panel
        mainUpgradePanel.SetActive(false);

        // Enable the upgrade confirmation panel
        upgradeConfirmationPanel.SetActive(true);
        
        // Update the upgrade text
        UpdateUpgradeText();
    }

    public void HideUpgradeConfirmationPanel()
    {
        // Disable the upgrade confirmation panel
        upgradeConfirmationPanel.SetActive(false);

        // Enable the main upgrade panel
        mainUpgradePanel.SetActive(true);
        
        // Update the upgrade text
        UpdateUpgradeText();
    }

    private void ChangeMaxValue(FloatReference current, FloatReference max, int amount)
    {
        var newValue = max.Value + amount;

        // If the player is gaining health, increase the max health & health
        if (newValue > max)
        {
            var healthDifference = newValue - max;
            max.Value = newValue;
            current.Value += healthDifference;
        }

        // If the player is losing health, decrease the max health & health
        else if (newValue < max)
            max.Value = newValue;

        // Clamp the health
        current.Value = Mathf.Clamp(current, 0, max);
    }

    public void SetPurchasedPower(PowerScriptableObject power)
    {
        var powerResult = power.NullCheckToResult("Power Scriptable Object");

        // Set the purchased power if the power is not null
        powerResult.Match(n => _purchasedPower = n);

        // Set the tutorial to play if the power is a drug
        powerResult
            .Check(n => n.PowerType == PowerType.Drug, "Power is not a drug!")
            .Match(_ => _playTutorialAfterClose = true);

        // Write a tooltip telling the player to check the tutorials menu for more info
        powerResult.Match(p =>
            {
                JournalTooltipManager.Instance.AddTooltip(
                    $"Check the tutorials menu for more info about {p.PowerName}!");
            }
        );
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
}