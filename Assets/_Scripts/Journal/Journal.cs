using System;
using UnityEngine;

public class Journal : MonoBehaviour
{
    public static Journal Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private JournalTooltipManager tooltipManager;
    [SerializeField] private JournalObjectiveManager objectiveManager;

    #endregion

    #region Getters

    public JournalTooltipManager JournalTooltipManager => tooltipManager;

    public JournalObjectiveManager JournalObjectiveManager => objectiveManager;

    #endregion

    private void Awake()
    {
        // Ensure only one instance of the Journal exists
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance to this object
        Instance = this;

        // Don't destroy the Journal when changing scenes
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Initialize the tooltip manager & objective manager
        tooltipManager.Initialize(this);
        objectiveManager.Initialize(this);
    }

    private void Update()
    {
        // Update the tooltip manager
        tooltipManager.Update();
    }
}