public abstract class ValueVariable<T> : ResetableScriptableObject
{
    public abstract T GetValue();
    
    public abstract void SetValue(T newValue);
}