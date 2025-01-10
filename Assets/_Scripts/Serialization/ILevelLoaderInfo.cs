using UnityEngine;

public interface ILevelLoaderInfo
{
    public GameObject GameObject { get; }

    public UniqueId UniqueId { get; }

    public void LoadData(LevelLoader levelLoader);

    public void SaveData(LevelLoader levelLoader);
}