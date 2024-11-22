using System;
using System.Collections.Generic;
using UnityEngine;

public class JournalTooltipManager : MonoBehaviour
{
    public static JournalTooltipManager Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private JournalTooltip tooltipPrefab;

    #endregion

    #region Private Fields

    private readonly List<JournalTooltip> _tooltips = new();

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

    public void AddTooltip(Func<string> text, float duration)
    {
        // Instantiate a new tooltip
        var tooltip = Instantiate(tooltipPrefab, transform);

        // Initialize the tooltip
        tooltip.Initialize(text, duration);

        // Add the tooltip to the list of tooltips
        _tooltips.Add(tooltip);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            AddTooltip(() => $"Pos: {Player.Instance.gameObject.transform.position}", 3);
    }
}