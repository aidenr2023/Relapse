using System;
using System.Linq;
using UnityEngine;

public class SceneSaveLoader : MonoBehaviour
{
    private const string FILE_NAME = "SceneSave.json";

    public static string SceneInfoFilePath => GetSceneInfoFilePath(SaveFile.CurrentSaveFile.SaveFileDirectory);

    public static SceneSaveLoader Instance { get; private set; }

    public SceneResumeData SceneResumeData { get; private set; }

    [SerializeField] private LevelSectionSceneInfoList allSceneInfoList;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;

            // Set the parent of this object to null
            transform.SetParent(null);

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Load the settings from disk
        LoadSettingsFromDisk();

        if (SceneResumeData == null)
        {
            Debug.LogWarning("SceneResumeData is null. Cannot load settings.");
            return;
        }
    }

    public void SaveSettingsToDisk(Vector3 position, Quaternion rotation)
    {
        var currentSceneInfo = AsyncSceneManager.Instance.CurrentSceneInfo;

        // Return if the current scene info is null
        if (currentSceneInfo == null)
        {
            Debug.LogWarning("Current scene info is null. Cannot save to disk.");
            return;
        }

        // Return if the player instance is null
        if (Player.Instance == null)
        {
            Debug.LogWarning("Player instance is null. Cannot save to disk.");
            return;
        }

        // Create a new instance of the SceneResumeData class
        var newSceneData = new SceneResumeData
        {
            PersistentDataSceneName = currentSceneInfo.SectionPersistentData.SceneName,
            SectionSceneName = currentSceneInfo.SectionScene.SceneName,
            PlayerPosition = position,
            PlayerRotation = rotation
        };

        // Convert the current scene info to json
        SceneResumeData = newSceneData;

        // Convert the settings to json
        var jsonString = SceneResumeData.ToJson();
        System.IO.File.WriteAllText(SceneInfoFilePath, jsonString);

        Debug.Log($"Saved the scene data to {SceneInfoFilePath}");
    }

    public void LoadSettingsFromDisk()
    {
        // Check if the settings file path is valid
        if (!System.IO.File.Exists(SceneInfoFilePath))
        {
            Debug.LogWarning($"Scene Info file not found at {SceneInfoFilePath}. Using default settings.");
            return;
        }

        var jsonString = System.IO.File.ReadAllText(SceneInfoFilePath);

        // // Create a new instance of the LevelSectionSceneInfo class
        // // and deserialize the json string into it
        // var newSceneInfo = ScriptableObject.CreateInstance<LevelSectionSceneInfo>();
        // JsonUtility.FromJsonOverwrite(jsonString, newSceneInfo);
        //
        // // Set the loaded scene info to the current scene info
        // SceneResumeData = newSceneInfo;

        // Create a new sceneResumeData object
        var newSceneResumeData = new SceneResumeData()
        {
            PersistentDataSceneName = string.Empty,
            SectionSceneName = string.Empty,
            PlayerPosition = Vector3.zero,
            PlayerRotation = Quaternion.identity
        };

        // Deserialize the json string into the new sceneResumeData object
        JsonUtility.FromJsonOverwrite(jsonString, newSceneResumeData);
        
        var sceneInfo = allSceneInfoList.value.FirstOrDefault(info =>
            string.Equals(info.SectionPersistentData, newSceneResumeData.PersistentDataSceneName) &&
            string.Equals(info.SectionScene, newSceneResumeData.SectionSceneName)
        );

        // If the scene info of the new scene resume data is null, log an error and return
        if (sceneInfo == null)
        {
            Debug.LogError($"Scene info is null in {SceneInfoFilePath}. Cannot load settings.");
            return;
        }
        
        // Set the scene info of the new scene resume data
        newSceneResumeData.SceneInfo = sceneInfo;

        // Set the loaded scene info to the current scene info
        SceneResumeData = newSceneResumeData;

        Debug.Log($"Loaded the data from {SceneInfoFilePath}");
    }

    public void ClearData()
    {
        // Set the scene resume data to null
        SceneResumeData = null;

        // Delete the settings file
        if (System.IO.File.Exists(SceneInfoFilePath))
        {
            System.IO.File.Delete(SceneInfoFilePath);
            Debug.Log($"Deleted the scene data file at {SceneInfoFilePath}");
        }
    }

    public static string GetSceneInfoFilePath(string saveFileDirectory)
    {
        return System.IO.Path.Join(saveFileDirectory, FILE_NAME);
    }
}