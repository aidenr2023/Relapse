using UnityEngine;

public class DataSerializationHelper : MonoBehaviour
{
    public void ClearSaveData()
    {
        // Clear the data from the level loader
        LevelLoader.Instance?.ClearData();
        
        // Clear the data from the player
        PlayerLoader.Instance?.ClearData();
    }
}