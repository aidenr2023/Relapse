using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VendorShopButton : MonoBehaviour
{
    [SerializeField] private VendorMenu vendorMenu;

    [SerializeField] private Button button;

    [SerializeField] private TMP_Text powerNameText;
    [SerializeField] private Image powerImage;

    [SerializeField] private PowerScriptableObject power;

    public PowerScriptableObject Power => power;

    public Button Button => button;

    private void Start()
    {
        // Set the power info
        SetPowerInfo();
    }

    public void SetPower(PowerScriptableObject obj)
    {
        power = obj;

        // Set the power info
        SetPowerInfo();
    }

    public void SetPowerInfo()
    {
        // Return if the power is null
        if (power == null)
            return;

        powerNameText.text = power.PowerName;
        powerImage.sprite = power.Icon;
    }

    public void Reset()
    {
        // Reset the power
        power = null;

        // Set the power info
        SetPowerInfo();

        // Hide the button
        gameObject.SetActive(false);
    }

    public void Enable()
    {
        // Hide the button
        gameObject.SetActive(true);

        // Set the power info
        SetPowerInfo();
    }

    public void SetDescriptionText(TMP_Text text)
    {
        // Return if the power is null
        if (power == null)
            return;

        text.text = power.Description;
    }

    public void BuyPower()
    {
        // Return if the power is null
        if (power == null)
            return;

        // Return if the player already has the power
        if (Player.Instance.PlayerPowerManager.HasPower(power))
        {
            JournalTooltipManager.Instance.AddTooltip($"You already have {power.PowerName}.");
            return;
        }

        // Go back to the initial menu
        vendorMenu.IsolateMenu(vendorMenu.InitialMenu);

        // Return if the player cannot buy from the vendor
        if (!VendorMenu.Instance.CurrentVendor.CanBuyFromVendor)
        {
            JournalTooltipManager.Instance.AddTooltip($"You can no longer buy from {vendorMenu.CurrentVendor.VendorName}.");
            return;
        }

        // The player can no longer buy from the vendor
        VendorMenu.Instance.CurrentVendor.CanBuyFromVendor = false;

        JournalTooltipManager.Instance.AddTooltip($"You can no longer buy from {vendorMenu.CurrentVendor.VendorName}.");

        // Add the power clicked
        Player.Instance.PlayerPowerManager.AddPower(power);
    }

    public void SetNavigationUp(Selectable selectable)
    {
        var nav = button.navigation;

        nav.selectOnUp = selectable;

        button.navigation = nav;

        // Debug.Log($"Setting {gameObject.name}'s up navigation to {selectable.name}");
    }

    public void SetNavigationDown(Selectable selectable)
    {
        var nav = button.navigation;

        nav.selectOnDown = selectable;

        button.navigation = nav;

        // Debug.Log($"Setting {gameObject.name}'s down navigation to {selectable.name}");
    }

    public void SetNavigationLeft(Selectable selectable)
    {
        var nav = button.navigation;

        nav.selectOnLeft = selectable;

        button.navigation = nav;

        // Debug.Log($"Setting {gameObject.name}'s left navigation to {selectable.name}");
    }

    public void SetNavigationRight(Selectable selectable)
    {
        var nav = button.navigation;

        nav.selectOnRight = selectable;

        button.navigation = nav;

        // Debug.Log($"Setting {gameObject.name}'s right navigation to {selectable.name}");
    }

}