using System;
using UnityEngine;

[Serializable]
public class SceneResumeData
{
    [field: SerializeField] public string PersistentDataSceneName { get; set; }
    [field: SerializeField] public string SectionSceneName { get; set; }
    [field: SerializeField] public Vector3 PlayerPosition { get; set; }
    [field: SerializeField] public Quaternion PlayerRotation { get; set; }
    [field: NonSerialized] public LevelSectionSceneInfo SceneInfo { get; set; }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}