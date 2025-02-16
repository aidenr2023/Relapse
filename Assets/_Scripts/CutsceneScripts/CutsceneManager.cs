using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Organizes cutscenes using a dictionary (populated from a serialized array) and delegates playback to the CutsceneHandler.
/// </summary>
[System.Serializable]
public class CutsceneMapping
{
    // The unique name for the cutscene (used for lookup).
    public string cutsceneName;
    // The corresponding CutsceneData asset.
    public CutsceneData cutsceneData;
}

public class CutsceneManager : MonoBehaviour
{
    [Header("Cutscene Mappings (ordered for clarity)")]
    [Tooltip("Assign cutscene names and their corresponding CutsceneData asset.")]
    public CutsceneMapping[] cutsceneMappings;
    
    // Dictionary for fast runtime lookup of cutscene data by name.
    private Dictionary<string, CutsceneData> cutsceneDictionary;

    // Reference to the CutsceneHandler that actually plays the cutscene.
    public CutsceneHandler cutsceneHandler;
    
    private void Awake()
    {
        // Build the dictionary from the serialized mappings.
        cutsceneDictionary = new Dictionary<string, CutsceneData>();
        foreach(var mapping in cutsceneMappings)
        {
            if(!cutsceneDictionary.ContainsKey(mapping.cutsceneName))
            {
                cutsceneDictionary.Add(mapping.cutsceneName, mapping.cutsceneData);
            }
            else
            {
                Debug.LogWarning("Duplicate cutscene name found: " + mapping.cutsceneName);
            }
        }
    }
    
    /// <summary>
    /// Looks up and plays a cutscene by its name.
    /// </summary>
    /// <param name="cutsceneName">The name of the cutscene to play (must match one in the dictionary).</param>
    public void PlayCutsceneByName(string cutsceneName)
    {
        if(cutsceneDictionary.ContainsKey(cutsceneName))
        {
            CutsceneData data = cutsceneDictionary[cutsceneName];
            if(cutsceneHandler != null)
            {
                cutsceneHandler.PlayCutscene(data);
            }
            else
            {
                Debug.LogError("CutsceneHandler is not assigned in the CutsceneManager.");
            }
        }
        else
        {
            Debug.LogError("Cutscene not found: " + cutsceneName);
        }
    }
}
