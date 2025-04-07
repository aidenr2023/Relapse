using System;

public class Option<TSomeType>
{
    private readonly TSomeType _value;
    public bool HasValue { get; }
    
    public TSomeType Value
    {
        get
        {
            // If this has no value, throw an exception.
            if (!HasValue)
                throw new InvalidOperationException("No value present.");

            // Otherwise, return the value.
            return _value;
        }
    }

    private Option(TSomeType value, bool hasValue)
    {
        _value = value;
        HasValue = hasValue;
    }

    public static Option<TSomeType> Some(TSomeType value)
    {
        // Check if the value is null and throw an exception.
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        // Create an Option with a value.
        return new(value, true);
    }

    // Create an Option with no value.
    public static Option<TSomeType> None => new(default, false);

    // Map: Transform the value if it exists.
    public Option<TOtherType> Map<TOtherType>(Func<TSomeType, TOtherType> func)
    {
        // If this has a value, apply the function to it and return a new Option with the result.
        if (HasValue)
            return Option<TOtherType>.Some(func(_value));

        // If this has no value, return an Option with no value.
        return Option<TOtherType>.None;
    }

    // Bind: Chain operations that return an Option.
    public Option<TOtherType> Bind<TOtherType>(Func<TSomeType, Option<TOtherType>> func)
    {
        // If this has a value, apply the function to it and return the result.
        if (HasValue)
            return func(_value);
        
        // If this has no value, return an Option with no value.
        return Option<TOtherType>.None;
    }
    
    /// <summary>
    /// Match: Execute one of two actions based on whether this Option has a value.
    /// </summary>
    /// <param name="someAction"></param>
    /// <param name="noneAction"></param>
    public void Match(Action<TSomeType> someAction, Action noneAction)
    {
        // If this has a value, execute the someAction with the value.
        if (HasValue)
            someAction(_value);
        else
            noneAction();
    }
    public void Match(Action<TSomeType> someAction)
    {
        // If this has a value, execute the someAction with the value.
        if (HasValue)
            someAction(_value);
    }
    
    public TOtherType Switch<TOtherType>(Func<TSomeType, TOtherType> someFunc, Func<TOtherType> noneFunc)
    {
        // If this has a value, execute the someFunc with the value and return the result.
        if (HasValue)
            return someFunc(_value);
        
        // If this has no value, execute the noneFunc and return the result.
        return noneFunc();
    }
    
    public TSomeType Switch()
    {
        return Switch(some => some, () => default);
    }

    public static implicit operator Option<TSomeType>(TSomeType value)
    {
        // If the value is null, return an Option with no value.
        if (value == null)
            return Option<TSomeType>.None;

        // Otherwise, return an Option with the value.
        return Option<TSomeType>.Some(value);
    }
}