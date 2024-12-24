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

    public event Action OnIntroBegin;

    public event Action OnOutroBegin;

    #region Private Fields

    private Animator _animator;

    // Timer that starts after the intro animation finishes
    private CountdownTimer _activeTimer;

    private CountdownTimer _introTimer;
    private CountdownTimer _outroTimer;

    private Func<string> _textFunction;

    private bool _isMarkedForDestruction;

    private Func<bool> _completionCondition;
    private bool _isIndefinite;

    #endregion

    #region Getters

    public CountdownTimer IntroTimer => _introTimer;

    public CountdownTimer OutroTimer => _outroTimer;

    public CountdownTimer ActiveTimer => _activeTimer;

    public bool IsIntro => _introTimer.IsActive;

    public RectTransform RectTransform => (RectTransform)transform;

    public bool IsMarkedForDestruction => _isMarkedForDestruction;

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

    public void Initialize(
        Func<string> func, float duration,
        bool isIndefinite = false, Func<bool> completionCondition = null
    )
    {
        // Set the text function
        _textFunction = func;

        // Set up the active timer
        _activeTimer = new CountdownTimer(duration, isActive: false);

        _isIndefinite = isIndefinite;
        _completionCondition = completionCondition;

        // Start the outro animation when the active timer finishes
        _activeTimer.OnTimerEnd += () =>
        {
            // Play the outro animation
            _animator.Play(outroAnimationName);

            // Get the length of the outro animation
            var outroDuration = _animator.GetCurrentAnimatorStateInfo(0).length;

            // Initialize the outro timer
            _outroTimer.SetMaxTimeAndReset(outroDuration);
            _outroTimer.Start();

            // Start the active timer when the outro animation finishes
            _outroTimer.OnTimerEnd += () => _isMarkedForDestruction = true;

            // Call the OnOutroBegin event
            OnOutroBegin?.Invoke();

            _activeTimer.Stop();

            Debug.Log($"IS IND: {_isIndefinite} - {_textFunction()}");
        };

        // Play the intro animation
        _animator.Play(introAnimationName);

        // Get the length of the intro animation
        var introDuration = _animator.GetCurrentAnimatorStateInfo(0).length;

        // Initialize the intro/outro timer
        _introTimer = new CountdownTimer(introDuration, isActive: true);
        _outroTimer = new CountdownTimer(introDuration, isActive: false);

        // Start the active timer when the intro animation finishes
        _introTimer.OnTimerEnd += () =>
        {
            _introTimer.Stop();
            _activeTimer.Start();
        };

        // Call the OnIntroBegin event
        OnIntroBegin?.Invoke();
    }

    public void Initialize(string text, float duration)
    {
        Initialize(() => text, duration);
    }

    private void Update()
    {
        if (_isMarkedForDestruction)
        {
            Destroy(gameObject);
            return;
        }

        // If the completion condition is met, force the completion of the tooltip
        if ((_completionCondition?.Invoke() ?? false) && _activeTimer.Percentage < 1)
            ForceCompletion();

        // Set the text to the result of the text function
        Text = _textFunction();

        // Update the timers
        _activeTimer.SetActive(!_isIndefinite);

        _activeTimer.Update(Time.unscaledDeltaTime);
        _introTimer.Update(Time.unscaledDeltaTime);
        _outroTimer.Update(Time.unscaledDeltaTime);
    }

    public void ForceCompletion()
    {
        _activeTimer.Stop();

        // Set the active timer to 100% completion
        _activeTimer.ForcePercent(1);
    }
}