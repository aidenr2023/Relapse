using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SceneLoaderInformation
{
    [SerializeField] private LevelSectionSceneInfo[] sectionsToLoad;
    [SerializeField] private LevelSectionSceneInfo[] sectionsToUnload;

    public IReadOnlyCollection<LevelSectionSceneInfo> SectionsToLoad
    {
        get
        {
            // Create a hash set of scenes
            var scenes = new HashSet<LevelSectionSceneInfo>();

            // Add all the scenes to load
            foreach (var scene in sectionsToLoad)
                scenes.Add(scene);

            // Remove all the null scenes
            scenes.Remove(null);

            return scenes;
        }
    }

    public IReadOnlyCollection<LevelSectionSceneInfo> SectionsToUnload
    {
        get
        {
            // Create a hash set of scenes
            var scenes = new HashSet<LevelSectionSceneInfo>();

            // Add all the scenes to unload
            foreach (var scene in sectionsToUnload)
                scenes.Add(scene);

            return scenes;
        }
    }
    
    public static SceneLoaderInformation Create(LevelSectionSceneInfo[] sectionsToLoad, LevelSectionSceneInfo[] sectionsToUnload)
    {
        var information = new SceneLoaderInformation();
        
        information.sectionsToLoad = sectionsToLoad;
        information.sectionsToUnload = sectionsToUnload;
        
        return information;
    }
}