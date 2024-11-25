using System;
using System.Collections.Generic;
using UnityEngine;

public class JournalTooltipManager : MonoBehaviour
{
    public static JournalTooltipManager Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private JournalTooltip tooltipPrefab;

    [SerializeField] private float tooltipSpacing = -100;

    [SerializeField] private float tooltipDuration = 3;

    #endregion

    #region Private Fields

    private readonly List<JournalTooltip> _tooltips = new();
    private readonly Dictionary<JournalTooltip, float> _tooltipYAdjustments = new();

    #endregion

    private void Awake()
    {
        // Set the instance to this object
        Instance = this;

        // Don't destroy the Journal when changing scenes
        DontDestroyOnLoad(gameObject);
    }

    public void AddTooltip(string text, float duration)
    {
        AddTooltip(() => text, duration);
    }

    public void AddTooltip(string text)
    {
        AddTooltip(text, tooltipDuration);
    }

    public void AddTooltip(Func<string> text, float duration)
    {
        // Instantiate a new tooltip
        var tooltip = Instantiate(tooltipPrefab, transform);

        // Add the tooltip to the list of tooltips
        _tooltips.Add(tooltip);
        _tooltipYAdjustments.Add(tooltip, 0);

        // Remove the tooltips from the list of tooltips when the outro animation finishes
        tooltip.OnTooltipEnd += () =>
        {
            _tooltips.Remove(tooltip);
            _tooltipYAdjustments.Remove(tooltip);
        };


        // Initialize the tooltip
        tooltip.Initialize(text, duration);
    }

    public void AddTooltip(Func<string> text)
    {
        AddTooltip(text, tooltipDuration);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            AddTooltip(() => $"Pos: {Player.Instance.gameObject.transform.position}", 3);

        // Update the Y values of the tooltips
        UpdateTooltipYAdjustment();
    }

    private void UpdateTooltipYAdjustment()
    {
        for (int i = 0; i < _tooltips.Count; i++)
        {
            var tooltip = _tooltips[i];

            if (tooltip.IsMarkedForDestruction)
                continue;

            const float timerAcceleration = 2f;

            if (tooltip.IntroTimer.IsActive)
                _tooltipYAdjustments[tooltip] =
                    Mathf.Clamp01(tooltip.IntroTimer.OutputValue * timerAcceleration) * tooltipSpacing;

            else if (tooltip.OutroTimer.IsActive)
                _tooltipYAdjustments[tooltip] =
                    (1 - Mathf.Clamp01(tooltip.OutroTimer.OutputValue * timerAcceleration)) * tooltipSpacing;

            else
                _tooltipYAdjustments[tooltip] = tooltipSpacing;
        }

        var currentY = 0f;

        foreach (var tooltip in _tooltips)
        {
            // Continue to the next tooltip if the tooltip is marked for destruction
            if (tooltip.IsMarkedForDestruction)
                continue;

            // Set the tooltip's position
            tooltip.RectTransform.anchoredPosition = new Vector2(tooltip.transform.position.x, currentY);

            // Get the corresponding Y adjustment
            var yAdjustment = _tooltipYAdjustments[tooltip];

            // Add the tooltip's height to the current Y value
            currentY += yAdjustment;
        }
    }
}