using UnityEngine;
using UnityEngine.Serialization;

public class TutorialButtonManager : MonoBehaviour
{
    [SerializeField] private GameObject commonText;
    [SerializeField] private GameObject keyboardButton;
    [SerializeField] private GameObject gamepadButton;

    public GameObject KeyboardButton => keyboardButton;
    public GameObject GamepadButton => gamepadButton;
    
    public void SetActiveButton(bool isKeyboard)
    {
        commonText.SetActive(true);
        keyboardButton.SetActive(isKeyboard);
        gamepadButton.SetActive(!isKeyboard);
    }
    
    public void HideImages()
    {
        commonText.SetActive(false);
        keyboardButton.SetActive(false);
        gamepadButton.SetActive(false);
    }
}