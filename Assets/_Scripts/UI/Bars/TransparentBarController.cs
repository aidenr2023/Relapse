using UnityEngine;

[RequireComponent(typeof(TransparentBar))]
public abstract class TransparentBarController : MonoBehaviour
{
    private const float SNAPPING_THRESHOLD = 0.001f;

    #region Private Fields

    private TransparentBar _transparentBar;

    private float _desiredOpacity;

    private CountdownTimer _stayOnScreenTimer;

    private bool _hasFirstFrameRan;

    #endregion

    #region Getters

    protected abstract float CurrentValue { get; set; }
    protected abstract float PreviousValue { get; set; }

    #endregion

    protected void Awake()
    {
        // Get the transparent bar component
        _transparentBar = GetComponent<TransparentBar>();

        // Create the stay on screen timer
        _stayOnScreenTimer = new CountdownTimer(_transparentBar.StayOnScreenTime);
        _stayOnScreenTimer.Start();

        // Set the desired opacity to 0
        _desiredOpacity = _transparentBar.CanvasGroup.alpha = _transparentBar.MinOpacity;

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
        _stayOnScreenTimer.SetMaxTime(_transparentBar.StayOnScreenTime);
        _stayOnScreenTimer?.Update(Time.unscaledDeltaTime);

        // Custom update
        CustomUpdate();

        // Set the current value
        SetCurrentValue();

        // If the first frame has not run, set the previous value
        if (!_hasFirstFrameRan)
        {
            SetPreviousValue();
            _hasFirstFrameRan = true;

            // Set the values of the sliders
            _transparentBar.Slider.value = _transparentBar.BackgroundFillSlider.value = CalculatePercentage();
        }

        // Calculate the stamina percentage
        var percentage = CalculatePercentage();

        // Change the slider value
        _transparentBar.Slider.value = Mathf.Lerp(_transparentBar.Slider.value, percentage,
            _transparentBar.SliderLerpAmount * frameAmount);
        if (Mathf.Abs(_transparentBar.Slider.value - percentage) < SNAPPING_THRESHOLD)
            _transparentBar.Slider.value = percentage;

        // Change the background fill slider value
        _transparentBar.BackgroundFillSlider.value = Mathf.Lerp(
            _transparentBar.BackgroundFillSlider.value, percentage,
            _transparentBar.SliderLerpAmount * _transparentBar.BackgroundSliderLerpMultiplier * frameAmount
        );
        if (Mathf.Abs(_transparentBar.BackgroundFillSlider.value - percentage) < SNAPPING_THRESHOLD)
            _transparentBar.BackgroundFillSlider.value = percentage;

        // If the value of the foreground slider is greater than the background slider,
        // set the background slider to the foreground slider
        if (_transparentBar.Slider.value > _transparentBar.BackgroundFillSlider.value)
            _transparentBar.BackgroundFillSlider.value = _transparentBar.Slider.value;

        // If the previous stamina is greater than the current stamina, set the desired opacity to 1
        // If the current stamina is the max stamina, but the previous stamina was not, set the desired opacity to 0
        var isValueDecreasing = PreviousValue > CurrentValue;
        var isValueIncreasing = PreviousValue < CurrentValue;
        var isValueJustNowFull = percentage >= 1 && PreviousValue < CurrentValue;
        var isValueEmpty = percentage <= 0 && PreviousValue > CurrentValue;
        var isValueFull = percentage >= 1;

        if ((isValueIncreasing && _transparentBar.ShowWhenIncreasing) ||
            (isValueDecreasing && _transparentBar.ShowWhenDecreasing) |
            (isValueJustNowFull && _transparentBar.ShowWhenFull) ||
            (isValueEmpty && _transparentBar.ShowWhenEmpty) ||
            (isValueFull && _transparentBar.AlwaysShowWhileFull)
           )
        {
            // Set the desired opacity to 1
            _desiredOpacity = _transparentBar.MaxOpacity;

            // Reset the stay on screen timer
            _stayOnScreenTimer?.Reset();
        }

        // Set the desired opacity to 0
        else if (_stayOnScreenTimer?.IsComplete ?? false)
            _desiredOpacity = _transparentBar.MinOpacity;

        // Set the opacity of the images
        var newAlpha = Mathf.Lerp(_transparentBar.CanvasGroup.alpha, _desiredOpacity,
            _transparentBar.OpacityLerpAmount * frameAmount);

        if (Mathf.Abs(newAlpha - _transparentBar.CanvasGroup.alpha) < SNAPPING_THRESHOLD)
            newAlpha = _desiredOpacity;

        _transparentBar.CanvasGroup.alpha = newAlpha;
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