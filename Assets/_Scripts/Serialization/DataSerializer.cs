using System;
using UnityEngine;

public class DataSerializer : MonoBehaviour
{
    [SerializeField] private SerializationDataEvent[] dataLoadedEvents;

    private bool _hasRunEvents;

    private void Update()
    {
        // If the events have not been run
        // Load the data
        if (!_hasRunEvents)
            RunEvents();
    }

    private void RunEvents()
    {
        // For each data loaded event
        foreach (var dataLoadedEvent in dataLoadedEvents)
        {
            var conditionMet = false;

            // Based on the type of the data, check if the condition is met
            switch (dataLoadedEvent.DataInfo.DataType)
            {
                case SerializationDataType.Boolean:
                    if (dataLoadedEvent.BoolConditionalValue == dataLoadedEvent.DataInfo.GetBoolValue())
                        conditionMet = true;

                    break;

                case SerializationDataType.Number:
                    if (Mathf.Approximately(
                            dataLoadedEvent.NumberConditionalValue,
                            dataLoadedEvent.DataInfo.GetNumberValue())
                       )
                        conditionMet = true;
                    break;

                case SerializationDataType.String:
                    if (string.Compare(
                            dataLoadedEvent.StringConditionalValue,
                            dataLoadedEvent.DataInfo.GetStringValue(),
                            StringComparison.Ordinal) == 0
                       )
                        conditionMet = true;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            // If the condition is met, invoke the onTrue event
            if (conditionMet)
            {
                dataLoadedEvent.OnTrue.Invoke();
                Debug.Log($"{dataLoadedEvent.DataInfo.VariableName} is true!");
            }
            else
            {
                dataLoadedEvent.OnFalse.Invoke();
                Debug.Log($"{dataLoadedEvent.DataInfo.VariableName} is false!");
            }
        }

        // Set the events as run
        _hasRunEvents = true;
    }

    public void PrintMessage(string text)
    {
        Debug.Log(text);
    }
}