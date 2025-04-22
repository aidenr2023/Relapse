using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewVendorShopButton : MonoBehaviour
{
    [SerializeField] private PowerListReference playerPowers;

    [SerializeField] private TMP_Text powerNameText;
    [SerializeField] private Image powerImage;
    [SerializeField] private Image selectImage;
    [SerializeField] private UIJitter uiJitter;

    [Space, SerializeField, Readonly] private PowerScriptableObject power;
    [SerializeField, Readonly] private bool isActive;
    [SerializeField, Readonly] private bool hasPower;

    [SerializeField] private Color normalColor;
    [SerializeField] private Color alreadyBoughtColor;
    [SerializeField] private Color unavailableColor;

    [Space, SerializeField] private Color normalImageColor = Color.white;
    [SerializeField] private Color greyedOutImageColor = Color.grey;

    [Space, SerializeField] private VendorMenu vendorMenu;

    public PowerScriptableObject Power => power;

    public void Initialize(PowerScriptableObject newPower, bool active)
    {
        power = newPower;
        hasPower = playerPowers.Value.Contains(power);
        isActive = active || hasPower;

        // Set the text and image
        powerImage.sprite = power.Icon;

        if (hasPower || !isActive)
        {
            if (hasPower)
                powerNameText.text = $"{power.PowerName}\n(Purchased!)";
            else
                powerNameText.text = $"{power.PowerName}\n(Unavailable!)";

            // Set the select image's color to unavailable
            selectImage.color = alreadyBoughtColor;

            // Disable the jitter
            uiJitter.SetLerpAmount(0);

            // Set the power image's color to greyed out
            powerImage.color = greyedOutImageColor;
        }
        else
        {
            powerNameText.text = power.PowerName;

            // Set the select image's color to normal
            selectImage.color = normalColor;

            // Enable the jitter
            uiJitter.SetLerpAmount(1);
            
            // Set the power image's color to normal
            powerImage.color = normalImageColor;
        }
    }

    public void BuyPower()
    {
        // Return if the power is null
        if (power == null)
            return;

        // Return if the player already has the power
        if (hasPower)
        {
            JournalTooltipManager.Instance.AddTooltip($"You already have {power.PowerName}!");
            return;
        }

        // Return if the power is not available
        if (!isActive)
        {
            JournalTooltipManager.Instance.AddTooltip($"{power.PowerName} is not available!");
            return;
        }

        // Return if the player cannot buy from the vendor
        if (!VendorMenu.Instance.CurrentVendor.CanBuyFromVendor)
        {
            JournalTooltipManager.Instance.AddTooltip(
                $"You can no longer buy from {vendorMenu.CurrentVendor.VendorName}.");
            return;
        }

        // The player can no longer buy from the vendor
        VendorMenu.Instance.CurrentVendor.CanBuyFromVendor = false;

        // Set the purchased power
        VendorMenu.Instance.SetPurchasedPower(power);

        // Add the power clicked
        Player.Instance.PlayerPowerManager.AddPower(power);

        // Go back to the initial menu
        vendorMenu.IsolateMenu(vendorMenu.InitialMenu);

        JournalTooltipManager.Instance.AddTooltip($"You can no longer buy from {vendorMenu.CurrentVendor.VendorName}.");
    }

    public void SetVendorMenuInfo() => vendorMenu.SetShopDescriptions(this);
}