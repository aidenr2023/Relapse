public interface INewEnemyMovementBehavior : IEnemyBehavior
{
    public NewEnemyBehaviorBrain Brain { get; }
    public NewEnemyMovement NewMovement { get; }

    /// <summary>
    /// The function that gets called when the behavior's state is set to "Movement Script"
    /// </summary>
    /// <param name="brain"></param>
    /// <param name="newMovement"></param>
    /// <param name="needsToUpdateDestination"></param>
    public void StateUpdateMovement(
        NewEnemyBehaviorBrain brain, NewEnemyMovement newMovement, bool needsToUpdateDestination
    );
}