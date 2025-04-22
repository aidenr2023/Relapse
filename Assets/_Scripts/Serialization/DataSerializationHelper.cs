using UnityEngine;

public class DataSerializationHelper : MonoBehaviour
{
    public void ClearSaveData()
    {
        // Clear the data from the level loader
        LevelLoader.Instance?.ClearData();
        
        // Clear the data from the player
        PlayerLoader.Instance?.ClearData();
        
        // Clear the data from the scene save loader
        SceneSaveLoader.Instance?.ClearData();

        SaveFile.CurrentSaveFile.ClearSaveFile();
    }

    public void LoadSaveDataFromDisk()
    {
        // Load the data from the level loader
        LevelLoader.Instance?.LoadDataDiskToMemory();
        
        // Load the data from the player
        PlayerLoader.Instance?.LoadDataDiskToMemory();
    }
}