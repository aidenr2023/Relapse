public interface IActor
{
    public float MaxHealth { get; }
    public float CurrentHealth { get; }
    
    public void ChangeHealth(float amount);
}