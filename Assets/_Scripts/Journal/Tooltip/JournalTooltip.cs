using System;
using TMPro;
using UnityEngine;

/// <summary>
/// A tool tip that displays information at the top of the screen.
/// There can be multiple tooltips on the screen at once.
/// </summary>
public class JournalTooltip : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private TMP_Text textComponent;

    [SerializeField] private string introAnimationName;
    [SerializeField] private string outroAnimationName;

    #endregion

    public event Action OnTooltipEnd;

    #region Private Fields

    private Animator _animator;

    // Timer that starts after the intro animation finishes
    private CountdownTimer _activeTimer;

    private CountdownTimer _introOutroTimer;

    private Func<string> _textFunction;

    #endregion

    public string Text
    {
        get => textComponent.text;
        private set => textComponent.text = value;
    }

    private void Awake()
    {
        // Get the animator component
        _animator = GetComponent<Animator>();
    }

    public void Initialize(Func<string> func, float duration)
    {
        // Set the text function
        _textFunction = func;

        // Set up the active timer
        _activeTimer = new CountdownTimer(duration);

        // Start the outro animation when the active timer finishes
        _activeTimer.OnTimerEnd += () =>
        {
            // Play the outro animation
            _animator.Play(outroAnimationName);

            // Get the length of the outro animation
            var outroDuration = _animator.GetCurrentAnimatorStateInfo(0).length;

            // Initialize the intro/outro timer
            _introOutroTimer.SetMaxTimeAndReset(outroDuration);
            _introOutroTimer.Start();

            // Start the active timer when the outro animation finishes
            _introOutroTimer.OnTimerEnd += () => Destroy(gameObject);

            // Clear the intro/outro timer
            _introOutroTimer = null;
        };

        // Play the intro animation
        _animator.Play(introAnimationName);

        // Get the length of the intro animation
        var introDuration = _animator.GetCurrentAnimatorStateInfo(0).length;

        // Initialize the intro/outro timer
        _introOutroTimer = new CountdownTimer(introDuration, isActive: true);

        // Start the active timer when the intro animation finishes
        _introOutroTimer.OnTimerEnd += () =>
        {
            _introOutroTimer.Stop();
            _activeTimer.Start();
        };
    }

    public void Initialize(string text, float duration)
    {
        Initialize(() => text, duration);
    }

    private void Update()
    {
        // Set the text to the result of the text function
        Text = _textFunction();

        // Update the timers
        _activeTimer.Update(Time.deltaTime);
        _introOutroTimer.Update(Time.deltaTime);
    }
}