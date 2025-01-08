using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerTutorialManager : ComponentScript<Player>, IPlayerLoaderInfo
{
    #region Serialized Fields

    [SerializeField, Readonly] private Tutorial[] completedTutorials = Array.Empty<Tutorial>();

    #endregion

    #region Private Fields

    private readonly HashSet<Tutorial> _completedTutorials = new();

    #endregion

    #region Getters

    public GameObject GameObject => gameObject;

    #endregion

    private void Update()
    {
        // Refresh the completed tutorials
        completedTutorials = _completedTutorials.ToArray();
    }

    public void CompleteTutorial(Tutorial tutorial)
    {
        // Add the tutorial to the completed tutorials
        _completedTutorials.Add(tutorial);
    }

    #region Saving and Loading

    public string Id => "PlayerTutorialManager";

    public void LoadData(PlayerLoader playerLoader, bool restore)
    {
        // Clear the completed tutorials
        _completedTutorials.Clear();

        // For each tutorial in the save data, add it to the completed tutorials
        foreach (var tutorial in Tutorial.Tutorials)
        {
            // If the tutorial is not in the save data, skip it
            if (!playerLoader.TryGetDataFromMemory(Id, tutorial.UniqueId, out bool isComplete))
                continue;

            // Add the tutorial to the completed tutorials
            if (isComplete)
                _completedTutorials.Add(tutorial);

            Debug.Log($"Reloaded & added {tutorial.TutorialName} to the completed tutorials!");
        }
    }

    public void SaveData(PlayerLoader playerLoader)
    {
        // For each completed tutorial, save the data
        foreach (var tutorial in _completedTutorials)
        {
            var itemData = new DataInfo(tutorial.UniqueId, _completedTutorials.Contains(tutorial));
            playerLoader.AddDataToMemory(Id, itemData);
        }
    }

    #endregion
}