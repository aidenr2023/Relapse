public interface IEnemyAttackBehavior : IEnemyBehavior, IDamager
{
    public bool IsAttackEnabled { get; }

    public void SetAttackEnabled(bool on);
}