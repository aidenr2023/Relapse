using System.Linq;
using UnityEngine;

public class SaveFile
{
    /// <summary>
    /// A temporary save file that is used when the current save file is null.
    /// This is used to prevent null reference exceptions when the game is first started.
    /// This is cleared when the game is opened.
    /// This is mainly used for when we play the game in the editor without choosing a save file.
    /// </summary>
    private const string TEMPORARY_SAVE_FILE_NAME = "TemporarySaveFile";

    private static string SaveDirectory => System.IO.Path.Join(Application.persistentDataPath, "SaveFiles");

    private static SaveFile _currentSaveFile;

    public static SaveFile CurrentSaveFile
    {
        get
        {
            // Create a temporary save file if the current save file is null
            if (_currentSaveFile == null)
                _currentSaveFile = new SaveFile(TEMPORARY_SAVE_FILE_NAME);

            return _currentSaveFile;
        }
    }

    public string Name { get; private set; }

    public string SaveFileDirectory => System.IO.Path.Join(SaveDirectory, Name); 

    public SaveFile(string name)
    {
        Name = name;

        // Create the save file directory if it doesn't exist
        if (!System.IO.Directory.Exists(SaveFileDirectory))
            System.IO.Directory.CreateDirectory(SaveFileDirectory);

        // // TODO: Turn this off / delete it if we want to keep the temporary save file between sessions.
        // // If this is the temporary save file, delete all files in the directory
        // if (name == TEMPORARY_SAVE_FILE_NAME && Application.isEditor)
        // {
        //     foreach (var file in System.IO.Directory.GetFiles(SaveFileDirectory))
        //         System.IO.File.Delete(file);
        // }
    }

    public static string GetMostRecentSaveFile()
    {
        // Get all save files in the save file directory
        var saveFiles = System.IO.Directory.GetDirectories(SaveDirectory);

        // If there are no save files, return null
        if (saveFiles.Length == 0)
            return null;
        
        // Get the directories in the save file directory.
        // Check if they actually have files in them
        // If they don't, return null
        var hasAnyFiles = false;
        
        foreach (var saveFile in saveFiles)
        {
            var files = System.IO.Directory.GetFiles(saveFile);
            
            if (files.Length == 0)
                continue;
            
            var sceneSaveLoaderPath = SceneSaveLoader.GetSceneInfoFilePath(saveFile);
            
            if (!files.Contains(sceneSaveLoaderPath))
            {
                Debug.LogError($"Save file {saveFile} does not contain a scene save loader file.");
                continue;
            }
            
            hasAnyFiles = true;
            break;
        }
        
        // If there are no save files, return null
        if (!hasAnyFiles)
            return null;

        // Get the most recent save file
        var mostRecentSaveFile = saveFiles[0];
        var mostRecentSaveFileTime = System.IO.Directory.GetLastWriteTime(mostRecentSaveFile);

        foreach (var saveFile in saveFiles)
        {
            var saveFileTime = System.IO.Directory.GetLastWriteTime(saveFile);

            if (saveFileTime <= mostRecentSaveFileTime)
                continue;

            mostRecentSaveFile = saveFile;
            mostRecentSaveFileTime = saveFileTime;
        }

        return mostRecentSaveFile;
    }

    public void ClearSaveFile()
    {
        // Delete all files in the save file directory
        foreach (var file in System.IO.Directory.GetFiles(SaveFileDirectory))
            System.IO.File.Delete(file);

        // Delete all directories in the save file directory
        foreach (var directory in System.IO.Directory.GetDirectories(SaveFileDirectory))
            System.IO.Directory.Delete(directory, true);
        
        // // Delete the save file directory
        // System.IO.Directory.Delete(SaveFileDirectory, true);
    }
}