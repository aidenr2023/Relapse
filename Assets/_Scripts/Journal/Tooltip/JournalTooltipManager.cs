﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

[Serializable]
public class JournalTooltipManager
{
    public static JournalTooltipManager Instance => Journal.Instance?.JournalTooltipManager;

    #region Serialized Fields

    [SerializeField] private Transform tooltipParent;

    [SerializeField] private JournalTooltip tooltipPrefab;

    [SerializeField] private float tooltipSpacing = -100;

    [Header("Durations")] [SerializeField, Min(0)]
    private float generalTooltipDuration = 3;

    [SerializeField, Min(0)] private float objectiveTooltipDuration = 10;
    [SerializeField, Min(0)] private float tutorialTooltipDuration = 3;
    [SerializeField, Min(0)] private float moneyTooltipDuration = 3;
    [SerializeField, Min(0)] private float debugTooltipDuration = 3;

    #endregion

    #region Private Fields

    private Journal _journal;

    private readonly List<JournalTooltip> _tooltips = new();
    private readonly Dictionary<JournalTooltip, float> _tooltipYAdjustments = new();

    #endregion

    #region Getters

    public float IntroDuration => tooltipPrefab.IntroTimer.MaxTime;

    public float OutroDuration => tooltipPrefab.OutroTimer.MaxTime;

    #endregion

    public void Initialize(Journal journal)
    {
        // Set the journal
        _journal = journal;
    }

    public void AddTooltip(string text, float duration)
    {
        AddTooltip(() => text, duration);
    }

    public void AddTooltip(string text)
    {
        // Create basic tooltip info
        var tooltipInfo = new BasicJournalTooltipInfo(text, JournalTooltipType.General);

        // Add the tooltip
        AddTooltip(tooltipInfo);

        // AddTooltip(text, generalTooltipDuration);
    }

    public void AddTooltip(
        Func<string> text, float duration, bool isIndefinite = false,
        Func<bool> completionCondition = null)
    {
        // Instantiate a new tooltip
        var tooltip = Object.Instantiate(tooltipPrefab, tooltipParent);

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
        tooltip.Initialize(text, duration, isIndefinite, completionCondition);
    }

    public void AddTooltip(Func<string> text)
    {
        AddTooltip(text, generalTooltipDuration);
    }

    public void AddTooltip(IJournalTooltipInfo tooltipInfo)
    {
        // Determine the duration of the tooltip based on the tooltip type
        var duration = tooltipInfo.TooltipType switch
        {
            JournalTooltipType.General => generalTooltipDuration,
            JournalTooltipType.Objective => objectiveTooltipDuration,
            JournalTooltipType.Tutorial => tutorialTooltipDuration,
            JournalTooltipType.Money => moneyTooltipDuration,
            JournalTooltipType.Debug => debugTooltipDuration,
            _ => throw new ArgumentOutOfRangeException()
        };

        AddTooltip(tooltipInfo.Text, duration);
    }

    public void Update()
    {
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

            const float timerAcceleration = 1f;

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