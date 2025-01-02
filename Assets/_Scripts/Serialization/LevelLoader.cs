using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader Instance { get; private set; }

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

        // Set the parent to null
        transform.parent = null;

        // Set this to not be destroyed when reloading scene
        DontDestroyOnLoad(gameObject);

        // Whenever a new scene is loaded, call the LoadDataOnSceneLoaded function
        SceneManager.sceneLoaded += LoadDataOnSceneLoaded;
    }

    private void LoadDataOnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        // TODO: Load the data from the disk
        LoadDataFromDisk();

        // Apply the loaded data to the game objects
        LoadDataObjects(scene);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F7))
            SaveDataObjects(null);

        // Reload the current scene to test the saving and loading
        if (Input.GetKeyDown(KeyCode.F8))
            StartCoroutine(ReloadScenes());

        // Save the data to the disk
        if (Input.GetKeyDown(KeyCode.F10))
            SaveDataToDisk();
    }

    private IEnumerator ReloadScenes()
    {
        // Get the build index of the active scene
        var activeSceneBuildIndex = SceneManager.GetActiveScene().buildIndex;

        // Get the build indices of the scenes that are loaded
        var loadedSceneBuildIndices = new List<int>();
        for (var i = 0; i < SceneManager.sceneCount; i++)
            loadedSceneBuildIndices.Add(SceneManager.GetSceneAt(i).buildIndex);

        // Create an empty scene
        // var emptyScene = SceneManager.CreateScene("EmptyScene");

        // Load the scene single
        SceneManager.LoadScene(0, LoadSceneMode.Single);

        // Wait for the end of the frame
        yield return new WaitForEndOfFrame();

        // // Unload all the scenes
        // foreach (var scene in loadedSceneBuildIndices)
        //     SceneManager.UnloadSceneAsync(scene);

        // // Wait for the end of the frame
        // yield return new WaitForEndOfFrame();

        // Load all the previously loaded scenes
        foreach (var scene in loadedSceneBuildIndices)
            SceneManager.LoadScene(scene, LoadSceneMode.Additive);

        // Wait for the end of the frame
        yield return new WaitForEndOfFrame();

        // Set the active scene to the previously active scene
        SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(activeSceneBuildIndex));

        // Wait for the end of the frame
        yield return new WaitForEndOfFrame();

        // Remove the empty scene
        SceneManager.UnloadSceneAsync(0);
    }

    /// <summary>
    /// Load the data from the disk and fill the dictionary with it.
    /// </summary>
    private void LoadDataFromDisk()
    {
        // TODO: Populate the _data dictionary with the data from the disk
    }

    public void LoadDataObjects(Scene? scene)
    {
        GameObject[] rootGameObjects;

        // Get all the root game objects in the scene
        if (scene != null)
            rootGameObjects = scene.Value.GetRootGameObjects();

        // Get all the root game objects in all the scenes
        else
        {
            var scenes = new List<Scene>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
                scenes.Add(SceneManager.GetSceneAt(i));

            rootGameObjects = scenes.SelectMany(n => n.GetRootGameObjects()).ToArray();
        }

        // Get all the scripts in the scene that implement the ILevelLoaderInfo interface
        var levelLoaderInfos = rootGameObjects
            .SelectMany(rootGameObject => rootGameObject.GetComponentsInChildren<MonoBehaviour>())
            .OfType<ILevelLoaderInfo>();

        // For each script that implements the ILevelLoaderInfo interface
        // Load the data
        foreach (var levelLoaderInfo in levelLoaderInfos)
            levelLoaderInfo.LoadData(this);
    }

    private void SaveDataObjects(Scene? scene)
    {
        GameObject[] rootGameObjects;

        // Get all the root game objects in the scene
        if (scene != null)
            rootGameObjects = scene.Value.GetRootGameObjects();

        // Get all the root game objects in all the scenes
        else
        {
            var scenes = new List<Scene>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
                scenes.Add(SceneManager.GetSceneAt(i));

            rootGameObjects = scenes.SelectMany(n => n.GetRootGameObjects()).ToArray();
        }

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

    public void SaveDataToDisk()
    {
        // Create a list of JsonDataObjectWrappers
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

        // Create a new AllJsonData object
        var allJsonData = new AllJsonData(jsonDataObjectWrappers);

        // Convert the list of JsonDataObjectWrappers to a JSON string
        var jsonDataObjects = JsonUtility.ToJson(allJsonData);

        // Save the JSON string to the disk
        Debug.Log(jsonDataObjects);
    }

    [Serializable]
    public class JsonDataWrapper
    {
        [SerializeField] public string key;
        [SerializeField] SerializationDataType dataType;
        [SerializeField] public string value;

        public JsonDataWrapper(string key, SerializationDataType dataType, string value)
        {
            this.key = key;
            this.dataType = dataType;
            this.value = value;
        }
    }

    [Serializable]
    private class JsonDataObjectWrapper
    {
        [SerializeField] protected string variableName;
        [SerializeField] protected JsonDataWrapper[] data;

        public JsonDataObjectWrapper(string variableName, IEnumerable<JsonDataWrapper> data)
        {
            this.variableName = variableName;

            // Convert the list of JsonDataWrappers to a JSON string
            this.data = data.ToArray();
        }

        // public void Add<TOtherType>(JsonDataWrapper<TOtherType> jsonDataWrapper) =>
        //     jsonDataWrappers.Add((JsonDataWrapper<object>)jsonDataWrapper);
    }

    [Serializable]
    private class AllJsonData
    {
        [SerializeField] private JsonDataObjectWrapper[] data;

        public AllJsonData(IEnumerable<JsonDataObjectWrapper> jsonDataObjectWrappers)
        {
            data = jsonDataObjectWrappers.ToArray();
        }
    }
}