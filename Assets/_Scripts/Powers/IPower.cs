using UnityEngine;

public interface IPower
{
    /// <summary>
    /// The scriptable object that contains this power.
    /// </summary>
    public PowerScriptableObject PowerScriptableObject { get; set; }

    /// <summary>
    /// Logic for what happens when the player starts charging the power.
    /// </summary>
    /// <param name="startedChargingThisFrame">Whether the charge started this frame</param>
    public void StartCharge(TestPlayerPowerManager powerManager, PowerToken pToken, bool startedChargingThisFrame);

    /// <summary>
    /// What happens when the player holds down the power button.
    /// Typically, this is used to charge the power.
    /// However, some powers may not need to charge, in which case this method should be empty.
    /// Some custom logic can be added to this method for cool effects.
    /// </summary>
    public void Charge(TestPlayerPowerManager powerManager, PowerToken pToken);

    /// <summary>
    /// What happens when the player releases the power button.
    /// Typically, this is used to make a power stop charging.
    /// However, some powers may not need to charge, in which case this method should be empty.
    /// Some custom logic can be added to this method for cool effects.
    /// NOTE: This method SHOULD NOT call the Use method! 
    /// </summary>
    /// <param name="isCharged"></param>
    public void Release(TestPlayerPowerManager powerManager, PowerToken pToken, bool isCharged);

    /// <summary>
    /// What happens when the player releases the power when they are done charging.
    /// This is where any projectiles may be fired or other effects may be triggered.
    /// </summary>
    public void Use(TestPlayerPowerManager powerManager, PowerToken pToken);

    /// <summary>
    /// What happens while the power is active.
    /// This is where continuous effects are applied.
    /// </summary>
    /// <param name="powerManager"></param>
    /// <param name="pToken"></param>
    public void Active(TestPlayerPowerManager powerManager, PowerToken pToken);
}

public static class IPowerExtensions
{
    /// <summary>
    /// Connects the power to the scriptable object that contains it.
    /// Should ONLY be called via the PowerScriptableObject class.
    /// </summary>
    /// <param name="powerScriptableObject"></param>
    public static void AttachToScriptableObject(this IPower power, PowerScriptableObject powerScriptableObject)
    {
        power.PowerScriptableObject = powerScriptableObject;
    }

    /// <summary>
    /// Common logic to update clock-based durations.
    /// </summary>
    /// <param name="currentDuration">A reference to a float that represents the current duration</param>
    /// <param name="maxDuration">The maximum duration of the time span</param>
    /// <param name="amount">The amount that is added to the current duration</param>
    public static void UpdateDuration(ref float currentDuration, float maxDuration, float amount)
    {
        // Increment the charge duration by the amount
        currentDuration += amount;

        // Clamp the charge duration to the maximum charge duration
        if (maxDuration > 0)
            currentDuration = Mathf.Clamp(currentDuration, 0, maxDuration);
        else
            currentDuration = 1;
    }
    
    public static int ToleranceMultiplier(this PowerType powerType)
    {
        return powerType == PowerType.Drug ? 1 : -1;
    }
}