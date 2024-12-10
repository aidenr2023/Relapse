using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SceneLoaderInformation
{
    [SerializeField] private SceneField[] scenesToLoad;
    [SerializeField] private SceneUnloadField[] scenesToUnload;

    public IReadOnlyCollection<SceneField> ScenesToLoad
    {
        get
        {
            var scenes = new HashSet<SceneField>();

            foreach (var scene in scenesToLoad)
                scenes.Add(scene);

            // Remove null elements
            scenes.Remove(null);

            return scenes;
        }
    }

    public IReadOnlyCollection<SceneUnloadField> ScenesToUnload
    {
        get
        {
            var scenes = scenesToUnload
                .Where(n => !scenesToLoad.Contains(n))
                .ToArray();

            return scenes;
        }
    }
}