using System.Collections.Generic;

public class GenericListVariable<T> : GenericVariable<List<T>>
{
    protected override void CustomReset()
    {
        // Create a new list for the value
        value = new List<T>();
        
        // Copy the default values to the value
        foreach (var item in defaultValue)
            value.Add(item);
    }
}