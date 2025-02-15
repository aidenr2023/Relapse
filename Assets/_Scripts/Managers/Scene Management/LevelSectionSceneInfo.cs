using UnityEngine;

[CreateAssetMenu(fileName = "LevelSectionSceneInfo", menuName = "Scene Management/Level Section Scene Info")]
public class LevelSectionSceneInfo : ScriptableObject
{
    [SerializeField] private SceneField sectionPersistentData;
    [SerializeField] private SceneField sectionScene;

    public SceneField SectionPersistentData => sectionPersistentData;

    public SceneField SectionScene => sectionScene;
    
    public static LevelSectionSceneInfo Create(SceneField sectionPersistentData, SceneField sectionScene)
    {
        var info = new LevelSectionSceneInfo();
        info.sectionPersistentData = sectionPersistentData;
        info.sectionScene = sectionScene;
        
        return info;
    }
}