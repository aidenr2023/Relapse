using System;
using UnityEngine;

[Serializable]
public class SceneResumeData
{
    [field: SerializeField] public LevelSectionSceneInfo SceneInfo { get; set; }
    [field: SerializeField] public Vector3 PlayerPosition { get; set; }
    [field: SerializeField] public Quaternion PlayerRotation { get; set; }
    
    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}