public interface IEnemyMovementBehavior : IEnemyBehavior
{
    public bool IsMovementEnabled { get; }

    public void SetMovementEnabled(bool on);
}