using System;

[Serializable]
public class FloatReference : GenericReference<float, FloatVariable>
{
    // Operators
    public static implicit operator float(FloatReference variable)
    {
        return variable.Value;
    }

    public static float operator +(FloatReference a, float b)
    {
        return a.Value + b;
    }

    public static float operator -(FloatReference a, float b)
    {
        return a.Value - b;
    }

    public static float operator *(FloatReference a, float b)
    {
        return a.Value * b;
    }

    public static float operator /(FloatReference a, float b)
    {
        return a.Value / b;
    }
    
    public static bool operator ==(FloatReference a, float b)
    {
        return a.Value == b;
    }
    
    public static bool operator !=(FloatReference a, float b)
    {
        return a.Value != b;
    }
    
    public static bool operator >(FloatReference a, float b)
    {
        return a.Value > b;
    }
    
    public static bool operator <(FloatReference a, float b)
    {
        return a.Value < b;
    }
    
    public static bool operator >=(FloatReference a, float b)
    {
        return a.Value >= b;
    }
    
    public static bool operator <=(FloatReference a, float b)
    {
        return a.Value <= b;
    }
}