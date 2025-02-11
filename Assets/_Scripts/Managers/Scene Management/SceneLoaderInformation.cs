﻿using System;
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
            // var scenes = new HashSet<SceneField>();
            //
            // foreach (var scene in scenesToLoad)
            //     scenes.Add(scene);
            //
            // // Remove null elements
            // scenes.Remove(null);
            //
            // return scenes;

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
            // var scenes = scenesToUnload
            //     .Where(n => !scenesToLoad.Contains(n))
            //     .ToArray();
            //
            // return scenes;

            // Create a hash set of scenes
            var scenes = new HashSet<LevelSectionSceneInfo>();

            // Add all the scenes to unload
            foreach (var scene in sectionsToUnload)
                scenes.Add(scene);

            // // Remove all the null scenes
            // scenes.Remove(null);
            //

            // // Remove all the scenes to load
            // foreach (var scene in sectionsToLoad)
            //     scenes.Remove(scene);

            return scenes;
        }
    }
}