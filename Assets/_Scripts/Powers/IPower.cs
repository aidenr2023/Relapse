public interface IPower
{
    /// <summary>
    /// Used to increment or decrement the tolerance meter.
    /// This value should always be positive.
    /// Whether this value is added or subtracted is determined elsewhere.
    /// </summary>
    public float ToleranceMeterImpact { get; }

    /// <summary>
    /// Used to determine if a power is done charging.
    /// For powers that do not need to charge, this should always return true. 
    /// </summary>
    public bool IsCharged { get; }

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