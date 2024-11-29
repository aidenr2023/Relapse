public enum EnemyDetectionState
{
    /// <summary>
    /// The enemy is unaware of the player's presence.
    /// They are doing their routine tasks.
    /// </summary>
    Unaware,

    /// <summary>
    /// The enemy is curious about the player's presence.
    /// They know that something is wrong, but they are not sure.
    /// </summary>
    Curious,

    /// <summary>
    /// The enemy is fully aware of the player's presence.
    /// They are actively looking at the player / chasing the player.
    /// </summary>
    Aware,
}