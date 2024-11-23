using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ButtonManager : MonoBehaviour
{
    // Colors for normal, hover, and click states
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color clickColor = Color.red;

    /// <summary>
    /// Changes the text color of the TextMeshPro to the hover color.
    /// Attach this to the PointerEnter event.
    /// </summary>
    public void OnPointerEnter(GameObject textObject)
    {
        TMP_Text tmpText = textObject.GetComponent<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.color = hoverColor;
        }
    }

    /// <summary>
    /// Changes the text color of the TextMeshPro back to the normal color.
    /// Attach this to the PointerExit event.
    /// </summary>
    public void OnPointerExit(GameObject textObject)
    {
        TMP_Text tmpText = textObject.GetComponent<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.color = normalColor;
        }
    }

    /// <summary>
    /// Called when the Resume button is clicked.
    /// </summary>
    public void Resume(GameObject textObject)
    {
        ChangeClickColor(textObject);
        // Add your Resume logic here
        Debug.Log("Resume game");
    }

    /// <summary>
    /// Called when the Journal button is clicked.
    /// </summary>
    public void Journal(GameObject textObject)
    {
        ChangeClickColor(textObject);
        // Add your Journal logic here
        Debug.Log("Open Journal");
    }

    /// <summary>
    /// Called when the Settings button is clicked.
    /// </summary>
    public void Settings(GameObject textObject)
    {
        ChangeClickColor(textObject);
        // Add your Settings logic here
        Debug.Log("Open Settings");
    }

    /// <summary>
    /// Called when the Exit button is clicked.
    /// </summary>
    public void Exit(GameObject textObject)
    {
        ChangeClickColor(textObject);
        // Add your Exit logic here
        Debug.Log("Exit the game");
    }

    /// <summary>
    /// Changes the color of the TextMeshPro text to the click color.
    /// </summary>
    /// <param name="textObject">The GameObject associated with the button.</param>
    private void ChangeClickColor(GameObject textObject)
    {
        TMP_Text tmpText = textObject.GetComponent<TMP_Text>();
        if (tmpText != null)
        {
            tmpText.color = clickColor;
        }
    }
}
