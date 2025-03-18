using UnityEngine;

public class InGameUIObject : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private CanvasGroupListVariable inGameUi;

    private void OnEnable()
    {
        // Add the canvas group to the list of in-game UI objects
        inGameUi.value.Add(canvasGroup);
    }

    private void OnDisable()
    {
        // Remove the canvas group from the list of in-game UI objects
        inGameUi.value.Remove(canvasGroup);
    }
}