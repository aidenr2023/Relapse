using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JournalUIObjective : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private JournalObjective objective;

    [SerializeField] private Color incompleteColor;
    [SerializeField] private Color completeColor;
    [SerializeField] private Image colorImage;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private Button button;
    [SerializeField] private EventTrigger eventTrigger;

    #endregion

    #region Getters

    public JournalObjective Objective => objective;

    public Button Button => button;

    public EventTrigger EventTrigger => eventTrigger;

    #endregion

    private void Update()
    {
        if (objective == null)
            throw new Exception("Objective not set");

        // Update the objective data
        UpdateObjectiveData();
    }

    private void UpdateObjectiveData()
    {
        objectiveText.text = objective.ShortDescription;

        // Set the color of the objective based on its completion status
        if (JournalObjectiveManager.Instance == null)
        {
            // Set the color to the incomplete color
            colorImage.color = incompleteColor;
            return;
        }

        // Set the color
        var color = incompleteColor;

        if (JournalObjectiveManager.Instance.IsObjectiveComplete(objective))
            color = completeColor;

        colorImage.color = color;
    }

    public void SetObjective(JournalObjective obj)
    {
        objective = obj;
    }
}