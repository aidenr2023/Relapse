using UnityEngine;

public class LightHelper : MonoBehaviour
{
    [SerializeField] private Light[] lights;

    public void ChangeColor(string hex)
    {
        var trimmedHex = hex.Trim();

        if (trimmedHex[0] != '#')
            hex = "#" + trimmedHex;

        // Convert the hex string to a color
        ColorUtility.TryParseHtmlString(hex, out var color);

        foreach (var cLight in lights)
        {
            // Continue if the light is null
            if (cLight == null)
                continue;

            // Set the color of the light
            cLight.color = color;
        }
    }
}