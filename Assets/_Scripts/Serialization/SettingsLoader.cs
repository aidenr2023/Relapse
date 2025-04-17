using System;
using UnityEngine;

public class SettingsLoader : MonoBehaviour
{
    private static string SaveDirectory => $"{Application.persistentDataPath}";
    private const string FILE_NAME = "UserSettings.json";
    
    public static SettingsLoader Instance { get; private set; }
    
    [SerializeField] private UserSettingsVariable userSettings;

    public string SettingsFilePath => $"{SaveDirectory}/{FILE_NAME}";
    
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
    }

    public void SaveSettingsToDisk()
    {
        // Convert the settings to json
        var jsonString = userSettings.value.ToJson();
        System.IO.File.WriteAllText(SettingsFilePath, jsonString);
        
        Debug.Log($"Saved the data to {SettingsFilePath}");
    }

    public void LoadSettingsFromDisk()
    {
        // Bug Fix: This is to prevent NULL values from being assigned to the settings
        var previousSettings = userSettings.value;
        
        // Check if the settings file path is valid
        if (!System.IO.File.Exists(SettingsFilePath))
        {
            Debug.LogWarning($"Settings file not found at {SettingsFilePath}. Using default settings.");
            return;
        }
        
        var jsonString = System.IO.File.ReadAllText(SettingsFilePath);
        var loadedSettings = JsonUtility.FromJson<UserSettings>(jsonString);
        
        userSettings.value = loadedSettings;
        
        // Bug Fix: Prevent NULL values from being assigned to the settings
        if (userSettings.value.AudioMixer == null)
            userSettings.value.AudioMixer = previousSettings.AudioMixer;
        
        Debug.Log($"Loaded the data from {SettingsFilePath}");
    }
}