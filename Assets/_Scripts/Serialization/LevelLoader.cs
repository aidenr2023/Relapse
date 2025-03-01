using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    private const string FILE_NAME = "LevelData";

    public static LevelLoader Instance { get; private set; }

    /// <summary>
    /// [
    ///     SceneName: name,
    ///     data: [{
    ///         UniqueId: id,
    ///         data    : [{ variableName, dataType, value }]
    ///     }]
    /// ]
    ///
    /// {ID: { variableName: value }}
    ///
    /// </summary>
    private readonly Dictionary<string, Dictionary<string, object>> _data = new();

    private readonly Dictionary<string, string> _objectToScene = new();

    public IReadOnlyDictionary<string, Dictionary<string, object>> Data => _data;

    private static string LevelDataPath => $"{SaveFile.CurrentSaveFile.SaveFileDirectory}/{FILE_NAME}.json";

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

        // // Set the parent to null
        // transform.parent = null;
        //
        // // Set this to not be destroyed when reloading scene
        // DontDestroyOnLoad(gameObject);

        // // Whenever a new scene is loaded, call the LoadDataOnSceneLoaded function
        // SceneManager.sceneLoaded += LoadDataOnSceneLoaded;

        // Load ALL the data from the disk
        LoadDataDiskToMemory(null);
    }

    private void LoadDataOnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        // Load the data from the disk
        LoadDataDiskToMemory(scene);

        // Apply the loaded data to the game objects
        LoadDataMemoryToScene(scene);
    }

    private void Update()
    {
        // TODO: Remove these debug key presses
        
        // if (Input.GetKeyDown(KeyCode.F7))
        //     SaveDataSceneToMemory(null);

        // Reload the current scene to test the saving and loading
        if (Input.GetKeyDown(KeyCode.F8))
            StartCoroutine(ReloadScenes());

        // // Save the data to the disk
        // if (Input.GetKeyDown(KeyCode.F10))
        //     SaveDataMemoryToDisk();
        //
        // // Load the data from the disk
        // if (Input.GetKeyDown(KeyCode.F11))
        //     LoadDataDiskToMemory(null);
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

    #region Loading

    private void ParseDataWrapper(JsonDataWrapper dataWrapper, string id, string sceneName)
    {
        // Get the current dictionary / create a new one if it doesn't already exist
        if (!_data.TryGetValue(id, out var idData))
        {
            idData = new Dictionary<string, object>();
            _data.Add(id, idData);
        }

        // Then, add the id to the object to scene dictionary
        if (sceneName != null && !_objectToScene.TryAdd(id, sceneName))
            _objectToScene[id] = sceneName;

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

    /// <summary>
    /// Load the data from the disk and fill the dictionary with it.
    /// </summary>
    public void LoadDataDiskToMemory(params Scene[] scenes)
    {
        var saveFileName = LevelDataPath;

        if (!System.IO.File.Exists(saveFileName))
        {
            Debug.LogWarning($"The file {saveFileName} does not exist!");
            return;
        }

        // Read the JSON string from the disk
        var jsonDataObjectsString = System.IO.File.ReadAllText(saveFileName);

        // Convert the JSON string to an AllJsonData object
        var allJsonData = JsonUtility.FromJson<SceneJsonDataCollection>(jsonDataObjectsString);

        // Return if the allJsonData object is null
        if (allJsonData == null)
        {
            Debug.LogWarning($"The save file {saveFileName} is empty / is invalid and could not be loaded!");
            return;
        }

        foreach (var sceneJsonData in allJsonData.Data)
        {
            // Skip the scene if it is not the current scene AND the scene is not null
            if (scenes != null && scenes.Length > 0 &&
                !scenes.Contains(SceneManager.GetSceneByName(sceneJsonData.SceneName)))
                continue;

            var str = new StringBuilder();

            str.Append($"Loading data for {sceneJsonData.SceneName}");

            foreach (var jsonDataObjectWrapper in sceneJsonData.Data)
            {
                str.Append($"\n\tLoading data for {jsonDataObjectWrapper.UniqueId}");

                foreach (var dataWrapper in jsonDataObjectWrapper.Data)
                {
                    // str.Append(
                    //     $"\n\t\tLoading {dataWrapper.Key} ({dataWrapper.DataType}) with value {dataWrapper.Value}");

                    ParseDataWrapper(dataWrapper, jsonDataObjectWrapper.UniqueId, sceneJsonData.SceneName);
                }
            }

            Debug.Log(str);
        }
    }

    public void LoadDataMemoryToScene(Scene? scene)
    {
        GameObject[] rootGameObjects;

        // Get all the root game objects in the scene
        if (scene != null)
        {
            if (!scene.Value.IsValid() || !scene.Value.isLoaded)
            {
                Debug.LogError($"The scene {scene.Value.name} is not valid or not loaded! Cannot load data.");
                return;
            }

            rootGameObjects = scene.Value.GetRootGameObjects();
        }

        // Get all the root game objects in all the scenes
        else
        {
            var scenes = new List<Scene>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
                scenes.Add(SceneManager.GetSceneAt(i));

            rootGameObjects = scenes.SelectMany(n => n.GetRootGameObjects()).ToArray();

            Debug.Log(
                $"Loading data for all scenes because the scene is null. {scenes.Count} & {rootGameObjects.Length}");
        }

        // Get all the scripts in the scene that implement the ILevelLoaderInfo interface
        var levelLoaderInfos = rootGameObjects
            .SelectMany(rootGameObject => rootGameObject.GetComponentsInChildren<MonoBehaviour>())
            .OfType<ILevelLoaderInfo>();

        // For each script that implements the ILevelLoaderInfo interface
        // Load the data
        foreach (var levelLoaderInfo in levelLoaderInfos)
        {
            // Debug.Log($"Loading data for {levelLoaderInfo.GameObject.name} {levelLoaderInfo.UniqueId.UniqueIdValue}");
            levelLoaderInfo.LoadData(this);
        }
    }

    public bool TryGetDataFromMemory<T>(UniqueId id, string key, out T value)
    {
        // Add the unique id to the object to scene dictionary
        if (!_objectToScene.TryAdd(id.UniqueIdValue, id.OriginalScene))
            _objectToScene[id.UniqueIdValue] = id.OriginalScene;

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

    #endregion

    #region Saving

    public void SaveDataSceneToMemory(Scene? scene)
    {
        GameObject[] rootGameObjects;

        // Get all the root game objects in the scene
        if (scene != null && scene.Value.IsValid() && scene.Value.isLoaded)
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

    public void SaveDataMemoryToDisk()
    {
        // Create a dictionary of SceneName -> List of JsonDataObjectWrappers
        var sceneData = new Dictionary<string, List<JsonDataObjectWrapper>>();

        // // Create a list of JsonDataObjectWrappers
        // var jsonDataObjectWrappers = new List<JsonDataObjectWrapper>();

        // For each unique id in the data dictionary
        foreach (var (uniqueId, dataValue) in _data)
        {
            // Get the original scene of the object
            // If there is no valid original scene, log an error and continue
            if (!_objectToScene.TryGetValue(uniqueId, out var originalScene))
            {
                Debug.LogError($"The object with the unique id {uniqueId} does not have an original scene!");
                continue;
            }

            // If the sceneData dictionary doesn't contain the original scene
            if (!sceneData.TryGetValue(originalScene, out var jsonDataObjectWrappers))
            {
                jsonDataObjectWrappers = new List<JsonDataObjectWrapper>();
                sceneData.Add(originalScene, jsonDataObjectWrappers);
            }

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

        // Convert the dictionary of sceneData to a SceneJsonDataCollection
        var allJsonData = new SceneJsonDataCollection(
            sceneData.Select(n => new SceneJsonData(n.Key, n.Value))
        );

        // Convert the list of JsonDataObjectWrappers to a JSON string
        var jsonDataObjects = JsonUtility.ToJson(allJsonData);

        // // Save the JSON string to the disk
        // Debug.Log(jsonDataObjects);

        var dataFileName = LevelDataPath;

        System.IO.File.WriteAllText(dataFileName, jsonDataObjects);

        // Debug.Log($"Saved the data to {dataFileName}");
    }

    public void AddDataToMemory(UniqueId id, IDataInfo dataInfo)
    {
        // Return if the unique id was instantiated at runtime
        if (id.InstantiatedAtRuntime)
            return;
        
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

        // Then, add the id to the object to scene dictionary
        if (!_objectToScene.TryAdd(id.UniqueIdValue, id.OriginalScene))
            _objectToScene[id.UniqueIdValue] = id.OriginalScene;

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

    #endregion

    public void ClearData()
    {
        // Wipe the data dictionary
        _data.Clear();
        _objectToScene.Clear();

        // Save the data to the disk
        SaveDataMemoryToDisk();
    }
}