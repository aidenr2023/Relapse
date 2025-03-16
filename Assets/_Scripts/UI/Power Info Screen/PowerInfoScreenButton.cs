using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerInfoScreenButton : MonoBehaviour
{
    [SerializeField] private PowerScriptableObject power;

    [SerializeField] private Button button;
    [SerializeField] private Image powerImage;
    [SerializeField] private TMP_Text powerNameText;
    [SerializeField] private bool isEquipped;

    [SerializeField] private Color equippedColor = Color.white;
    [SerializeField] private Color unEquippedColor = new(.25f, .25f, .25f);

    private PowerInfoScreen _powerInfoScreen;

    public Button Button => button;

    public void Initialize(PowerInfoScreen powerInfoScreen, PowerScriptableObject currentPower, bool isPowerEquipped)
    {
        power = currentPower;
        _powerInfoScreen = powerInfoScreen;
        isEquipped = isPowerEquipped;

        SetImage(currentPower);
        SetText(currentPower);
        SetInteractable(currentPower);
    }

    private void SetImage(PowerScriptableObject currentPower)
    {
        // Return if the current power is null
        if (currentPower == null)
            return;

        powerImage.sprite = currentPower.Icon;

        if (isEquipped)
            powerImage.color = equippedColor;
        else
            powerImage.color = unEquippedColor;
    }

    private void SetText(PowerScriptableObject currentPower)
    {
        // Return if the current power is null
        if (currentPower == null)
            return;

        powerNameText.text = currentPower.PowerName;
    }

    private void SetInteractable(PowerScriptableObject currentPower)
    {
        // Return if the current power is null
        if (currentPower == null)
            return;

        button.interactable = isEquipped;
    }

    public void UpdateScreenInformation()
    {
        if (!isEquipped)
            return;

        _powerInfoScreen.SetInformation(power);
    }
}