using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VendorShopButton : MonoBehaviour
{
    [SerializeField] private TMP_Text powerNameText;
    [SerializeField] private Image powerImage;

    [SerializeField] private PowerScriptableObject power;

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

}