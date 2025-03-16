using System;

[Serializable]
public class IntReference : GenericReference<int, IntVariable>
{
    
    // Operators
    public static implicit operator int(IntReference variable)
    {
        return variable.Value;
    }

    public static int operator +(IntReference a, int b)
    {
        return a.Value + b;
    }

    public static int operator -(IntReference a, int b)
    {
        return a.Value - b;
    }

    public static int operator *(IntReference a, int b)
    {
        return a.Value * b;
    }

    public static int operator /(IntReference a, int b)
    {
        return a.Value / b;
    }
    
    public static bool operator ==(IntReference a, int b)
    {
        return a.Value == b;
    }
    
    public static bool operator !=(IntReference a, int b)
    {
        return a.Value != b;
    }
    
    public static bool operator >(IntReference a, int b)
    {
        return a.Value > b;
    }
    
    public static bool operator <(IntReference a, int b)
    {
        return a.Value < b;
    }
    
    public static bool operator >=(IntReference a, int b)
    {
        return a.Value >= b;
    }
    
    public static bool operator <=(IntReference a, int b)
    {
        return a.Value <= b;
    }
    
}