using UnityEngine;

public class InputManagerHelper : MonoBehaviour
{
    private void Update()
    {
        SetCursorState(InputManager.Instance.CursorActive);
    }

    /// <summary>
    /// Disable the player controls.
    /// Hide / show the cursor.
    ///
    /// True - Unlocks the cursor and shows it. Used for menus and pause states.
    /// False - Locks the cursor and hides it. Used for gameplay.
    /// </summary>
    /// <param name="state"></param>
    private void SetCursorState(bool state)
    {
        // Set the cursor state
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;

        // Show / hide the cursor
        Cursor.visible = state;
    }
}