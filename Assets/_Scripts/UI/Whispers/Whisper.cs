using System;
using TMPro;
using UnityEngine;

public class Whisper : MonoBehaviour
{
    private const float OPACITY_THRESHOLD = 0.01f;

    #region Serialized Fields

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text text;
    [SerializeField, Range(0, 1)] private float fadeLerpAmount = 0.1f;

    [SerializeField] private Vector3 offset;
    [SerializeField, Min(0)] private float hoverFrequency = 1f;

    [SerializeField] private Vector3 offsetRotation;
    [SerializeField, Min(0)] private float rotationFrequency;

    #endregion

    #region Private Fields

    private CountdownTimer _timer;

    #endregion

    private void Start()
    {
        // Set the canvas group alpha to 0
        canvasGroup.alpha = 0;
    }

    private void Update()
    {
        // Update the timer
        _timer?.Update(Time.deltaTime);

        // Update the opacity
        UpdateOpacity();

        // Update the position
        UpdatePosition();
    }

    private void UpdateOpacity()
    {
        // Return if the timer is null
        if (_timer == null)
            return;

        var targetOpacity = 1f;

        // If the timer is done, set the target opacity to 0
        if (_timer.IsComplete)
            targetOpacity = 0;

        const float defaultDeltaTime = 1 / 60f;
        var frameTime = Time.deltaTime / defaultDeltaTime;

        // Lerp the alpha
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetOpacity, fadeLerpAmount * frameTime);

        // If the target Opacity is 0 and the alpha is less than the threshold, destroy the game object
        if (targetOpacity == 0 && canvasGroup.alpha < OPACITY_THRESHOLD)
            Destroy(gameObject);
    }

    private void UpdatePosition()
    {
        // Calculate the hover offset
        var hoverOffset = Mathf.Sin(Time.time * Mathf.PI / 2 * hoverFrequency);

        var calculatedOffset =
            transform.parent.up * (offset.y * hoverOffset) +
            transform.parent.right * (offset.x * hoverOffset) +
            transform.parent.forward * (offset.z * hoverOffset);

        // Set the local position to the offset
        transform.localPosition = calculatedOffset;

        var rotationOffset = Mathf.Sin(Time.time * Mathf.PI * 2 * rotationFrequency);

        transform.localRotation = Quaternion.Euler(
            offsetRotation.x * rotationOffset,
            offsetRotation.y * rotationOffset,
            offsetRotation.z * rotationOffset
        );
    }

    public void Initialize(string whisperText, float duration, Transform whisperPosition)
    {
        text.text = whisperText;

        // Set the parent to the whisper position
        // Set the local position to 0
        transform.SetParent(whisperPosition);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        // Create the timer
        _timer = new CountdownTimer(duration);
        _timer.Start();
    }
}