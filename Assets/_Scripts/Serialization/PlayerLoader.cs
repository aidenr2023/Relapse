using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlayerLoader : MonoBehaviour
{
    private const string FILE_NAME = "PlayerData";

    public static PlayerLoader Instance { get; private set; }

    /// <summary>
    /// [
    ///     UniqueId: id,
    ///     data    : [{ variableName, dataType, value }]
    /// ]
    ///
    /// {ID: { variableName: value }}
    ///
    /// </summary>
    private readonly Dictionary<string, Dictionary<string, object>> _data = new();

    private static string PlayerDataPath => $"{SaveFile.CurrentSaveFile.SaveFileDirectory}/{FILE_NAME}.json";

    protected void Awake()
    {
        // If there is already an instance, destroy this object and return
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance to this object
        Instance = this;

        // // Set the parent to null
        // transform.parent = null;
        //
        // // Set this to not be destroyed when reloading scene
        // DontDestroyOnLoad(gameObject);

        // Load ALL the data from the disk
        LoadDataDiskToMemory();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F7))
            SaveDataSceneToMemory();

        // Save the data to the disk
        if (Input.GetKeyDown(KeyCode.F10))
            SaveDataMemoryToDisk();

        // Load the data from the disk
        if (Input.GetKeyDown(KeyCode.F11))
        {
            LoadDataDiskToMemory();
            LoadDataMemoryToScene();
        }
    }

    #region Loading

    public void LoadDataDiskToMemory()
    {
        var saveFileName = PlayerDataPath;

        if (!System.IO.File.Exists(saveFileName))
        {
            Debug.LogWarning($"The file {saveFileName} does not exist!");
            return;
        }

        // Read the JSON string from the disk
        var jsonDataObjectsString = System.IO.File.ReadAllText(saveFileName);

        // Convert the JSON string to an AllJsonData object
        var allJsonData = JsonUtility.FromJson<SceneJsonData>(jsonDataObjectsString);

        // Return if the allJsonData object is null
        if (allJsonData == null)
        {
            Debug.LogWarning($"The save file {saveFileName} is empty / is invalid and could not be loaded!");
            return;
        }

        foreach (var dataObjectWrapper in allJsonData.Data)
        {
            var str = new StringBuilder();

            str.Append($"Loading data for {dataObjectWrapper.UniqueId}");

            foreach (var dataWrapper in dataObjectWrapper.Data)
                ParseDataWrapper(dataWrapper, dataObjectWrapper.UniqueId);

            Debug.Log(str);
        }
    }

    public void LoadDataMemoryToScene(bool restore = false)
    {
        // Get all the scripts in the scene that implement the ILevelLoaderInfo interface
        var playerLoaderInfos = FindObjectsOfType<MonoBehaviour>().OfType<IPlayerLoaderInfo>().ToArray();

        // For each script that implements the ILevelLoaderInfo interface
        // Load the data
        foreach (var levelLoaderInfo in playerLoaderInfos)
        {
            Debug.Log($"Loading data for {levelLoaderInfo.GameObject.name} {levelLoaderInfo.Id}");
            levelLoaderInfo.LoadData(this, restore);
        }
    }

    public bool TryGetDataFromMemory<T>(string id, string key, out T value)
    {
        // Check if the id exists in the data dictionary
        var hasIdValue = _data.TryGetValue(id, out var idData);

        // If the key exists in the data dictionary
        if (hasIdValue && idData.TryGetValue(key, out var dataValue))
        {
            if (dataValue is T castedValue)
            {
                value = castedValue;
                return true;
            }

            var tType = typeof(T);

            if (tType == typeof(float) || tType == typeof(int))
            {
                var doubleValue = (double)dataValue;
                value = (T)Convert.ChangeType(doubleValue, typeof(T));
                return true;
            }

            throw new InvalidCastException($"The value for the key {key} is not of type {typeof(T)}!");
        }

        value = default;
        return false;
    }

    private void ParseDataWrapper(JsonDataWrapper dataWrapper, string id)
    {
        // Get the current dictionary / create a new one if it doesn't already exist
        if (!_data.TryGetValue(id, out var idData))
        {
            idData = new Dictionary<string, object>();
            _data.Add(id, idData);
        }

        // Parse the data wrapper
        switch (dataWrapper.DataType)
        {
            case SerializationDataType.Boolean:
                if (!idData.TryAdd(dataWrapper.Key, bool.Parse(dataWrapper.Value)))
                    idData[dataWrapper.Key] = bool.Parse(dataWrapper.Value);
                break;

            case SerializationDataType.Number:
                if (!idData.TryAdd(dataWrapper.Key, double.Parse(dataWrapper.Value)))
                    idData[dataWrapper.Key] = double.Parse(dataWrapper.Value);
                break;

            case SerializationDataType.String:
                if (!idData.TryAdd(dataWrapper.Key, dataWrapper.Value))
                    idData[dataWrapper.Key] = dataWrapper.Value;
                break;

            case SerializationDataType.Vector3:
                if (!idData.TryAdd(dataWrapper.Key, JsonUtility.FromJson<Vector3>(dataWrapper.Value)))
                    idData[dataWrapper.Key] = JsonUtility.FromJson<Vector3>(dataWrapper.Value);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        // Debug.Log(
        //     $"Added {dataWrapper.Key} ({dataWrapper.DataType}) with value {dataWrapper.Value} to {id}: {idData[dataWrapper.Key]}");
    }

    #endregion

    #region Saving

    public void AddDataToMemory(string id, IDataInfo dataInfo)
    {
        // If the script name is empty, return
        if (string.IsNullOrEmpty(id))
            return;

        // First, try to add the data to the dictionary
        if (!_data.TryGetValue(id, out var idData))
        {
            idData = new Dictionary<string, object>();
            _data.Add(id, idData);
        }

        switch (dataInfo.DataType)
        {
            case SerializationDataType.Boolean:
                if (!idData.TryAdd(dataInfo.VariableName, dataInfo.GetBoolValue()))
                    idData[dataInfo.VariableName] = dataInfo.GetBoolValue();
                break;

            case SerializationDataType.Number:
                if (!idData.TryAdd(dataInfo.VariableName, dataInfo.GetNumberValue()))
                    idData[dataInfo.VariableName] = dataInfo.GetNumberValue();
                break;

            case SerializationDataType.String:
                if (!idData.TryAdd(dataInfo.VariableName, dataInfo.GetStringValue()))
                    idData[dataInfo.VariableName] = dataInfo.GetStringValue();
                break;

            case SerializationDataType.Vector3:
                if (!idData.TryAdd(dataInfo.VariableName, dataInfo.GetVector3Value()))
                    idData[dataInfo.VariableName] = dataInfo.GetVector3Value();
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void SaveDataSceneToMemory()
    {
        // Get all the player loader infos in the scene
        var playerLoaderInfos = FindObjectsOfType<MonoBehaviour>().OfType<IPlayerLoaderInfo>().ToArray();

        // Loop through all the player loader infos & save the data to memory
        foreach (var playerLoaderInfo in playerLoaderInfos)
            playerLoaderInfo.SaveData(this);
    }

    public void SaveDataMemoryToDisk()
    {
        // Create a list of json data object wrappers
        var jsonDataObjectWrappers = new List<JsonDataObjectWrapper>();

        // For each unique id in the data dictionary
        foreach (var (uniqueId, dataValue) in _data)
        {
            // Create a list of JsonDataWrappers
            var jsonDataWrappers = new List<JsonDataWrapper>();

            // For each key-value pair in the data dictionary
            foreach (var (key, value) in dataValue)
            {
                // Debug.Log($"Saving {key} with value {value} for {uniqueId} to the disk.");

                // Determine the type of the value
                var valueType = value.GetType();

                JsonDataWrapper wrapper;

                // Based on the type of the value, create the appropriate JsonDataWrapper
                if (valueType == typeof(bool))
                    wrapper = new JsonDataWrapper(key, SerializationDataType.Boolean, value.ToString());

                else if (valueType == typeof(double))
                    wrapper = new JsonDataWrapper(key, SerializationDataType.Number, value.ToString());

                else if (valueType == typeof(string))
                    wrapper = new JsonDataWrapper(key, SerializationDataType.String, value.ToString());

                else if (valueType == typeof(Vector3))
                    wrapper = new JsonDataWrapper(key, SerializationDataType.Vector3, JsonUtility.ToJson(value));

                else
                {
                    Debug.LogError($"The value type {valueType} is not supported!");
                    throw new ArgumentOutOfRangeException();
                }

                // Add the JsonDataWrapper to the list
                jsonDataWrappers.Add(wrapper);
            }

            // Create a JsonDataObjectWrapper
            var jsonDataObjectWrapper = new JsonDataObjectWrapper(uniqueId, jsonDataWrappers);

            // Add the JsonDataObjectWrapper to the list
            jsonDataObjectWrappers.Add(jsonDataObjectWrapper);
        }

        // Convert the list of JsonDataObjectWrappers a scene json data object
        var allJsonData = new SceneJsonData("PlayerData", jsonDataObjectWrappers);

        // Convert the list of JsonDataObjectWrappers to a JSON string
        var jsonDataObjects = JsonUtility.ToJson(allJsonData);

        // // Save the JSON string to the disk
        // Debug.Log(jsonDataObjects);

        var dataFileName = PlayerDataPath;

        System.IO.File.WriteAllText(dataFileName, jsonDataObjects);

        Debug.Log($"Saved the data to {dataFileName}");
    }

    #endregion

    public void ClearData()
    {
        // Wipe the data dictionary
        _data.Clear();
        
        // Save the data to the disk
        SaveDataMemoryToDisk();
    }
}