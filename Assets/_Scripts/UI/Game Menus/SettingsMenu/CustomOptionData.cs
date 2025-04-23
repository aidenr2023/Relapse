using TMPro;
using UnityEngine.UI;

public class CustomOptionData : TMP_Dropdown.OptionData
{
    private object Value { get; }

    public CustomOptionData(string text, object value) : base(text)
    {
        Value = value;
    }

    public bool TryGetValue<T>(out T value)
    {
        value = default;

        if (Value is not T castedValue)
            return false;

        value = castedValue;
        return true;
    }

    public bool ValueEquals<T>(T value)
    {
        if (Value is not T castedValue)
            return false;

        return castedValue.Equals(value);
    }
}