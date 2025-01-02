using System;
using UnityEngine;

[RequireComponent(typeof(UniqueId))]
public class DataSerializer : MonoBehaviour, ILevelLoaderInfo
{
    [SerializeField] private SerializationDataEvent[] dataLoadedEvents;

    public GameObject GameObject => gameObject;

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
                            (float)dataLoadedEvent.NumberConditionalValue,
                            (float)dataLoadedEvent.DataInfo.GetNumberValue())
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

                case SerializationDataType.Vector3:
                    if (Mathf.Approximately(dataLoadedEvent.Vector3ConditionalValue.x,
                            dataLoadedEvent.DataInfo.GetVector3Value().x) &&
                        Mathf.Approximately(dataLoadedEvent.Vector3ConditionalValue.y,
                            dataLoadedEvent.DataInfo.GetVector3Value().y) &&
                        Mathf.Approximately(dataLoadedEvent.Vector3ConditionalValue.z,
                            dataLoadedEvent.DataInfo.GetVector3Value().z)
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
    }

    public void PrintMessage(string text)
    {
        Debug.Log(text);
    }

    #region ILevelLoaderInfo

    private UniqueId _uniqueId;

    public UniqueId UniqueId
    {
        get
        {
            if (_uniqueId == null)
                _uniqueId = GetComponent<UniqueId>();

            return _uniqueId;
        }
    }


    public void LoadData(LevelLoader levelLoader)
    {
        // Load the data
        RunEvents();
    }

    public void SaveData(LevelLoader levelLoader)
    {
        // For each event in the data loaded events
        // Add the data to the level loader
        foreach (var dataLoadedEvent in dataLoadedEvents)
            levelLoader.AddData(UniqueId, dataLoadedEvent.DataInfo);
    }

    #endregion
}