using JetBrains.Annotations;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class VendorShopButton : MonoBehaviour
{
    [SerializeField] private TMP_Text powerNameText;
    [SerializeField] private Image powerImage;

    [SerializeField] private PowerScriptableObject power;

    [SerializeField] private GameObject vendorInteractable;

    public PowerScriptableObject Power => power;
    public bool canUseShop = true;

    public bool IsInteractable { get; set; } = true;

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
        Debug.Log("I was clicked!");
        Debug.Log("You purchased: " + power.PowerName);

        // Add the power clicked
        Player.Instance.PlayerPowerManager.AddPower(power);
        Debug.Log("Added a power: " + power);

        //NOT WORKING (make sure to set only that specific vendor and not all vendors. May require duping the buttons?)
        // Set IsInteractable to false
        //vendorInteractable.GetComponent<VendorInteractable>().IsInteractable = false;
        Debug.Log(vendorInteractable.GetComponent<VendorInteractable>());

        // Close the shop
        VendorMenu.Instance.EndVendor();
    }
}