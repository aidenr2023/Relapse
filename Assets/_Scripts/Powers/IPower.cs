using UnityEngine;

public interface IPower
{
    /// <summary>
    /// The scriptable object that contains this power.
    /// </summary>
    public PowerScriptableObject PowerScriptableObject { get; set; }

    /// <summary>
    /// Used to increment or decrement the tolerance meter.
    /// This value should always be positive.
    /// Whether this value is added or subtracted is determined elsewhere.
    /// </summary>
    public float ToleranceMeterImpact { get; }

    /// <summary>
    /// A float between 0 and 1.
    /// This is used to determine how much the power is charged.
    /// Powers that don't need to charge should NEVER just return 1.
    /// Instead, use normal logic to determine the charge percentage,
    /// but make the charge duration 0 in the power's scriptable object.
    /// </summary>
    public float ChargePercentage { get; }

    /// <summary>
    /// Logic for what happens when the player starts charging the power.
    /// </summary>
    /// <param name="startedChargingThisFrame">Whether the charge started this frame</param>
    public void StartCharge(bool startedChargingThisFrame);

    /// <summary>
    /// What happens when the player holds down the power button.
    /// Typically, this is used to charge the power.
    /// However, some powers may not need to charge, in which case this method should be empty.
    /// Some custom logic can be added to this method for cool effects.
    /// </summary>
    public void Charge();

    /// <summary>
    /// What happens when the player releases the power button.
    /// Typically, this is used to make a power stop charging.
    /// However, some powers may not need to charge, in which case this method should be empty.
    /// Some custom logic can be added to this method for cool effects.
    /// NOTE: This method SHOULD NOT call the Use method! 
    /// </summary>
    /// <param name="isCharged"></param>
    public void Release(bool isCharged);

    /// <summary>
    /// What happens when the player releases the power when they are done charging.
    /// This is where any projectiles may be fired or other effects may be triggered.
    /// </summary>
    public void Use();
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
    /// Common logic to charge a power.
    /// NOTE: This method should only be called in the UPDATE function of the class that implements IPower.
    /// </summary>
    /// <param name="power">The power that is being charged</param>
    /// <param name="currentChargeDuration">A reference to a float that represents the current duration that the spell has been charged.</param>
    public static void ChargePowerDuration(this IPower power, ref float currentChargeDuration)
    {
        // Get the maximum charge duration
        var maxChargeDuration = power.PowerScriptableObject.ChargeDuration;

        // Increment the charge duration by the time since the last frame
        currentChargeDuration += Time.deltaTime;

        // Clamp the charge duration to the maximum charge duration
        if (maxChargeDuration > 0)
            currentChargeDuration = Mathf.Clamp(currentChargeDuration, 0, maxChargeDuration);
        else
            currentChargeDuration = 1;
    }
}