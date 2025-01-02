using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance { get; private set; }

    private Scene _originalScene;

    private readonly Dictionary<string, Dictionary<string, object>> _data = new();

    public IReadOnlyDictionary<string, Dictionary<string, object>> Data => _data;

    private void Awake()
    {
        // If there is already an instance, destroy this object and return
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance to this object
        Instance = this;

        // Set this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

        // Whenever a new scene is loaded, call the LoadDataOnSceneLoaded function
        SceneManager.sceneLoaded += LoadDataOnSceneLoaded;
    }

    private void LoadDataOnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        // Set the original scene to the scene that was loaded
        _originalScene = scene;

        // TODO: Load the data from the disk
        LoadDataFromDisk();

        // Apply the loaded data to the game objects
        LoadDataObjects();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F7))
            SaveDataObjects();

        // Reload the current scene to test the saving and loading
        if (Input.GetKeyDown(KeyCode.F8))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    /// <summary>
    /// Load the data from the disk and fill the dictionary with it.
    /// </summary>
    private void LoadDataFromDisk()
    {
        // TODO: Populate the _data dictionary with the data from the disk
    }

    private void LoadDataObjects()
    {
        // Get all the root game objects in the scene
        var rootGameObjects = _originalScene.GetRootGameObjects();

        // Get all the scripts in the scene that implement the ILevelLoaderInfo interface
        var levelLoaderInfos = rootGameObjects
            .SelectMany(rootGameObject => rootGameObject.GetComponentsInChildren<MonoBehaviour>())
            .OfType<ILevelLoaderInfo>();

        // For each script that implements the ILevelLoaderInfo interface
        // Load the data
        foreach (var levelLoaderInfo in levelLoaderInfos)
            levelLoaderInfo.LoadData(this);
    }

    private void SaveDataObjects()
    {
        // Get all the root game objects in the scene
        var rootGameObjects = _originalScene.GetRootGameObjects();

        // Get all the scripts in the scene that implement the ILevelLoaderInfo interface
        var levelLoaderInfos = rootGameObjects
            .SelectMany(rootGameObject => rootGameObject.GetComponentsInChildren<MonoBehaviour>())
            .OfType<ILevelLoaderInfo>();

        // Debug.Log($"Saving data for {levelLoaderInfos.Count()} objects. ({rootGameObjects.Length} root objects)");

        // For each script that implements the ILevelLoaderInfo interface
        // Save the data
        foreach (var levelLoaderInfo in levelLoaderInfos)
            levelLoaderInfo.SaveData(this);
    }

    public bool GetData<T>(UniqueId id, string key, out T value)
    {
        // Check if the id exists in the data dictionary
        var hasIdValue = _data.TryGetValue(id.UniqueIdValue, out var idData);

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

    public void AddData(UniqueId id, IDataInfo dataInfo)
    {
        // If the unique id is empty, log an error and return
        if (id.UniqueIdValue == "")
        {
            Debug.LogError(
                $"The unique id for {id.gameObject.name} is empty! Click on the object in the inspector to generate a new unique id!");
            return;
        }

        // First, try to add the data to the dictionary
        if (!_data.TryGetValue(id.UniqueIdValue, out var idData))
        {
            idData = new Dictionary<string, object>();
            _data.Add(id.UniqueIdValue, idData);
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
}