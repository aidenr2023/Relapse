using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerInfoScreen : MonoBehaviour
{
    [SerializeField] private PowerArrayReference allPowers;
    [SerializeField] private PowerArrayReference equippedPowers;

    [SerializeField] private Transform neuroButtonParent;
    [SerializeField] private Transform vitalButtonParent;
    [SerializeField] private PowerInfoScreenButton powerButtonPrefab;

    [SerializeField] private Image powerImage;
    [SerializeField] private TMP_Text powerNameText;
    
    [SerializeField] private Button backButton;

    public GameObject FirstSelectedButton { get; private set; }
    
    public void SetUpScreen()
    {
        SetInformation(null);
        
        // Populate the power buttons
        PopulatePowerButtons();
    }

    private void PopulatePowerButtons()
    {
        // Clear the parents of any children
        ClearChildren(neuroButtonParent);
        ClearChildren(vitalButtonParent);

        // Reset the first selected button
        FirstSelectedButton = null;
        
        // Create a hash set of all the remaining powers
        var allPowersHashSet = new HashSet<PowerScriptableObject>(allPowers.Value);

        var anyNeuros = equippedPowers.Value.Any(n => n.PowerType == PowerType.Drug);
        
        // Go through each of the currently equipped powers and add them to the screen
        foreach (var power in equippedPowers.Value)
        {
            // If the power is null, continue to the next power
            if (power == null)
                continue;

            var powerButton = CreatePowerButton(power, true);
            
            switch (power.PowerType)
            {
                case PowerType.Drug:
                    powerButton.transform.SetParent(neuroButtonParent);

                    if (FirstSelectedButton == null)
                    {
                        FirstSelectedButton = powerButton.Button.gameObject;
                        Debug.Log($"First Selected Power: {FirstSelectedButton.name} - {power.PowerName}");
                    }
                    
                    break;
                
                case PowerType.Medicine:
                    powerButton.transform.SetParent(vitalButtonParent);
                    
                    if (FirstSelectedButton == null && !anyNeuros)
                        FirstSelectedButton = powerButton.Button.gameObject;
                    
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Remove the power from the hash set
            allPowersHashSet.Remove(power);
        }
        

        
        // Go through each of the remaining powers and add them to the screen
        while (allPowersHashSet.Count > 0)
        {
            var power = allPowersHashSet.First();
            var powerButton = CreatePowerButton(power, false);
            
            switch (power.PowerType)
            {
                case PowerType.Drug:
                    powerButton.transform.SetParent(neuroButtonParent);
                    break;
                
                case PowerType.Medicine:
                    powerButton.transform.SetParent(vitalButtonParent);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            // Remove the power from the hash set
            allPowersHashSet.Remove(power);
        }
        
        if (FirstSelectedButton == null)
            FirstSelectedButton = backButton.gameObject;
    }

    private void ClearChildren(Transform parent)
    {
        // Return if the parent is null
        if (parent == null)
            return;

        foreach (Transform child in parent)
            Destroy(child.gameObject);
    }

    private PowerInfoScreenButton CreatePowerButton(PowerScriptableObject power, bool isEquipped)
    {
        // Instantiate a new power button
        var newPowerButton = Instantiate(powerButtonPrefab, neuroButtonParent);

        // Initialize the power button
        newPowerButton.Initialize(this, power, isEquipped);

        return newPowerButton;
    }

    public void SetInformation(PowerScriptableObject power)
    {
        if (power == null)
        {
            powerNameText.text = string.Empty;
            
            powerImage.sprite = null;
            powerImage.color = new Color(0, 0, 0, 0);
            
            return;
        }
        
        powerNameText.text = power.PowerName;
        
        powerImage.sprite = power.Icon;
        powerImage.color = Color.white;
    }
}