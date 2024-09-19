using System;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayerPowerManager : MonoBehaviour
{
    [SerializeField] private PowerScriptableObject[] powers;
    private HashSet<PowerScriptableObject> _drugsSet;
    private HashSet<PowerScriptableObject> _medsSet;

    [SerializeField] private PowerScriptableObject currentPower;

    #region Initialization Functions

    private void Awake()
    {
        // Initialize the power collections
        InitializePowerCollections();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    private void InitializePowerCollections()
    {
        _drugsSet = new HashSet<PowerScriptableObject>();
        _medsSet = new HashSet<PowerScriptableObject>();
        UpdatePowerCollections(powers);
    }

    #endregion


    // Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// A method to update the power collections properly
    /// </summary>
    private void UpdatePowerCollections(params PowerScriptableObject[] newPowers)
    {
        // Condense the powers array by trimming the null values at the end
        int removeAmount;
        for (removeAmount = 0; removeAmount < powers.Length; removeAmount++)
        {
            if (powers[powers.Length - 1 - removeAmount] != null)
                break;
        }

        // Remove the null values from the end of the array if removeAmount is greater than 0
        if (removeAmount > 0)
            Array.Resize(ref powers, powers.Length - removeAmount);

        // Loop through all the new powers
        foreach (var power in newPowers)
        {
            if (power == null)
                continue;
            
            // Determine which collection to add the power to
            var addSet = power.PowerType switch
            {
                PowerType.Drug => _drugsSet,
                PowerType.Medicine => _medsSet,
                _ => throw new ArgumentOutOfRangeException()
            };

            // Add the power to the correct collection
            addSet.Add(power);
        }

        // Skip if the current power is already set or if there are no powers
        if (currentPower != null || powers.Length == 0)
            return;

        // Set the current power to the first power in the array
        currentPower = powers[0];
    }

    public void AddPower(PowerScriptableObject powerScriptableObject)
    {
        // Check if the power is already in one of the hash sets
        var powerSet = powerScriptableObject.PowerType switch
        {
            PowerType.Drug => _drugsSet,
            PowerType.Medicine => _medsSet,
            _ => throw new ArgumentOutOfRangeException()
        };

        if (!powerSet.Add(powerScriptableObject))
            return;

        // Add the power to the end of the powers array
        Array.Resize(ref powers, powers.Length + 1);
        powers[^1] = powerScriptableObject;

        // Update the power collections
        UpdatePowerCollections(powerScriptableObject);
    }
}