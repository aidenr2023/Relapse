using UnityEngine;

public interface IPlayerLoaderInfo
{
    public GameObject GameObject { get; }

    public string Id { get; }

    public void LoadData(PlayerLoader playerLoader, bool restore);

    public void SaveData(PlayerLoader playerLoader);
}