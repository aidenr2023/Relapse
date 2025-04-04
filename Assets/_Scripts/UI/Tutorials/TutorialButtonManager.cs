using UnityEngine;
using UnityEngine.Serialization;

public class TutorialButtonManager : MonoBehaviour
{
    [SerializeField] private GameObject commonText;
    [SerializeField] private GameObject keyboardButton;
    [SerializeField] private GameObject gamepadButton;

    private HiddenState _hiddenState = HiddenState.Uninitialized;

    public GameObject KeyboardButton => keyboardButton;
    public GameObject GamepadButton => gamepadButton;

    public void SetActiveButton(bool isKeyboard)
    {
        if (_hiddenState != HiddenState.Shown)
        {
            commonText.SetActive(true);
        }

        // Return if the button is already set to the desired state
        if (keyboardButton.activeSelf == isKeyboard && _hiddenState != HiddenState.Uninitialized)
            return;

        keyboardButton.SetActive(isKeyboard);
        gamepadButton.SetActive(!isKeyboard);
        
        _hiddenState = HiddenState.Shown;
    }

    public void HideImages()
    {
        if (_hiddenState == HiddenState.Hidden)
            return;

        _hiddenState = HiddenState.Hidden;

        commonText.SetActive(false);
        keyboardButton.SetActive(false);
        gamepadButton.SetActive(false);
    }

    private enum HiddenState
    {
        Uninitialized,
        Hidden,
        Shown
    }
}