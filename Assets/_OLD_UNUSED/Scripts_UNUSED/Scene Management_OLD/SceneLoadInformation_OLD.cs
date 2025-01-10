using System;
using System.Collections.Generic;
using UnityEngine;

// [CreateAssetMenu(fileName = "SceneLoadInformation", menuName = "Scene Load Information")]
public class SceneLoadInformation : ScriptableObject
{
    [SerializeField] private SceneLoadInfoPriority[] scenes;

    public IReadOnlyCollection<SceneLoadInfoPriority> Scenes => scenes;

    [Serializable]
    public class SceneLoadInfoPriority
    {
        [SerializeField] private string sceneName;
        [SerializeField] private int priority;

        public string SceneName => sceneName;
        public int Priority => priority;
    }
}