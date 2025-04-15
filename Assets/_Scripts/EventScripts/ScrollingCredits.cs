using UnityEngine;

public class CreditsScroll : MonoBehaviour
{
    [Header("Scrolling Settings")]
    public float scrollSpeed = 50f;   // Speed at which the credits scroll (adjust as needed).
    public float startDelay = 1.0f;   // Delay before the credits start scrolling.

    private RectTransform rectTransform;
    private bool scrolling = false;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogError("CreditsScroll must be attached to a UI element with a RectTransform!");
            return;
        }
        // Optionally, initialize the starting position here.
        // For example: rectTransform.anchoredPosition = new Vector2(0, -Screen.height);
        Invoke("StartScrolling", startDelay);
    }

    void Update()
    {
        if (scrolling)
        {
            // Move the RectTransform upward over time.
            rectTransform.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
        }
    }

    void StartScrolling()
    {
        scrolling = true;
    }
}
