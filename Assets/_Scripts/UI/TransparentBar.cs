using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class TransparentBar : MonoBehaviour
{
    private const float SNAPPING_THRESHOLD = 0.001f;

    #region Serialized Fields

    [SerializeField] protected Slider slider;
    [SerializeField] protected float sliderLerpAmount = .1f;
    [SerializeField] protected Slider backgroundFillSlider;
    [SerializeField, Range(0, 1)] protected float backgroundSliderLerpMultiplier = .5f;
    [SerializeField] protected CanvasGroup canvasGroup;

    [SerializeField, Min(0)] protected float stayOnScreenTime = 1f;

    [Space, SerializeField, Range(0, 1)] protected float minOpacity = 0;
    [SerializeField, Range(0, 1)] protected float maxOpacity = 1;
    [SerializeField, Min(0)] protected float opacityLerpAmount = .1f;

    [Space, SerializeField] private bool showWhenDecreasing = true;
    [SerializeField] private bool showWhenIncreasing = true;
    [SerializeField] private bool showWhenFull = true;
    [SerializeField] private bool showWhenEmpty = true;

    #endregion

    #region Private Fields

    protected float desiredOpacity;

    protected CountdownTimer stayOnScreenTimer;

    private bool _hasFirstFrameRan;

    #endregion

    #region Getters

    protected abstract float CurrentValue { get; set; }
    protected abstract float PreviousValue { get; set; }

    #endregion

    protected void Awake()
    {
        // Create the stay on screen timer
        stayOnScreenTimer = new CountdownTimer(stayOnScreenTime);
        stayOnScreenTimer.Start();

        // Set the desired opacity to 0
        desiredOpacity = canvasGroup.alpha = minOpacity;

        // Call the custom awake method
        CustomAwake();
    }

    protected virtual void CustomAwake()
    {
    }

    protected void Update()
    {
        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.unscaledDeltaTime / defaultFrameTime;

        // Update the stay on screen timer
        stayOnScreenTimer.SetMaxTime(stayOnScreenTime);
        stayOnScreenTimer?.Update(Time.unscaledDeltaTime);

        // Custom update
        CustomUpdate();

        // Set the current value
        SetCurrentValue();

        // If the first frame has not ran, set the previous value
        if (!_hasFirstFrameRan)
        {
            SetPreviousValue();
            _hasFirstFrameRan = true;

            // Set the values of the sliders
            slider.value = backgroundFillSlider.value = CalculatePercentage();
        }

        // Calculate the stamina percentage
        var percentage = CalculatePercentage();

        // Change the slider value
        slider.value = Mathf.Lerp(slider.value, percentage, sliderLerpAmount * frameAmount);
        if (Mathf.Abs(slider.value - percentage) < SNAPPING_THRESHOLD)
            slider.value = percentage;

        // Change the background fill slider value
        backgroundFillSlider.value = Mathf.Lerp(
            backgroundFillSlider.value, percentage,
            sliderLerpAmount * backgroundSliderLerpMultiplier * frameAmount
        );
        if (Mathf.Abs(backgroundFillSlider.value - percentage) < SNAPPING_THRESHOLD)
            backgroundFillSlider.value = percentage;

        // If the value of the foreground slider is greater than the background slider,
        // set the background slider to the foreground slider
        if (slider.value > backgroundFillSlider.value)
            backgroundFillSlider.value = slider.value;

        // If the previous stamina is greater than the current stamina, set the desired opacity to 1
        // If the current stamina is the max stamina, but the previous stamina was not, set the desired opacity to 0
        var isValueDecreasing = PreviousValue > CurrentValue;
        var isValueIncreasing = PreviousValue < CurrentValue;
        var isValueFull = percentage >= 1 && PreviousValue < CurrentValue;
        var isValueEmpty = percentage <= 0 && PreviousValue > CurrentValue;

        if ((isValueIncreasing && showWhenIncreasing) ||
            (isValueDecreasing && showWhenDecreasing) |
            (isValueFull && showWhenFull) ||
            (isValueEmpty && showWhenEmpty)
           )
        {
            // Set the desired opacity to 1
            desiredOpacity = maxOpacity;

            // Reset the stay on screen timer
            stayOnScreenTimer?.Reset();
        }

        // Set the desired opacity to 0
        else if (stayOnScreenTimer?.IsComplete ?? false)
            desiredOpacity = minOpacity;

        // Set the opacity of the images
        var newAlpha = Mathf.Lerp(canvasGroup.alpha, desiredOpacity, opacityLerpAmount * frameAmount);

        if (Mathf.Abs(newAlpha - canvasGroup.alpha) < SNAPPING_THRESHOLD)
            newAlpha = desiredOpacity;

        canvasGroup.alpha = newAlpha;
    }

    protected virtual void CustomUpdate()
    {
    }

    private void LateUpdate()
    {
        // Set the previous value
        SetPreviousValue();
    }

    protected abstract void SetCurrentValue();
    protected abstract void SetPreviousValue();

    protected abstract float CalculatePercentage();
}