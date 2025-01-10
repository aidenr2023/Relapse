using UnityEngine;
using UnityEngine.UI;

public class TransparentBar : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] protected Slider slider;
    [SerializeField] protected Slider backgroundFillSlider;
    [SerializeField] private CanvasGroup canvasGroup;

    [Space, SerializeField] protected float sliderLerpAmount = .1f;
    [SerializeField, Range(0, 1)] protected float backgroundSliderLerpMultiplier = .5f;

    [SerializeField, Min(0)] protected float stayOnScreenTime = 1f;

    [Space, SerializeField, Range(0, 1)] protected float minOpacity = 0;
    [SerializeField, Range(0, 1)] protected float maxOpacity = 1;
    [SerializeField, Min(0)] protected float opacityLerpAmount = .1f;

    [Space, SerializeField] private bool showWhenDecreasing = true;
    [SerializeField] private bool showWhenIncreasing = true;
    [SerializeField] private bool showWhenFull = true;
    [SerializeField] private bool showWhenEmpty = true;
    [SerializeField] private bool alwaysShowWhileFull = false;

    #endregion

    #region Getters

    public Slider Slider => slider;

    public Slider BackgroundFillSlider => backgroundFillSlider;

    public CanvasGroup CanvasGroup => canvasGroup;

    public float SliderLerpAmount => sliderLerpAmount;

    public float BackgroundSliderLerpMultiplier => backgroundSliderLerpMultiplier;

    public float StayOnScreenTime => stayOnScreenTime;

    public float MinOpacity => minOpacity;

    public float MaxOpacity => maxOpacity;

    public float OpacityLerpAmount => opacityLerpAmount;

    public bool ShowWhenDecreasing => showWhenDecreasing;

    public bool ShowWhenIncreasing => showWhenIncreasing;

    public bool ShowWhenFull => showWhenFull;

    public bool ShowWhenEmpty => showWhenEmpty;

    public bool AlwaysShowWhileFull => alwaysShowWhileFull;

    #endregion
}