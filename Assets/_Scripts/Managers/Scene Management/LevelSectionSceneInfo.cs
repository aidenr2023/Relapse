using UnityEngine;

[CreateAssetMenu(fileName = "LevelSectionSceneInfo", menuName = "Scene Management/Level Section Scene Info")]
public class LevelSectionSceneInfo : ScriptableObject
{
    [SerializeField] private SceneField sectionPersistentData;
    [SerializeField] private SceneField sectionScene;
    [SerializeField] private bool setActiveSceneToSectionScene = false;

    public SceneField SectionPersistentData => sectionPersistentData;

    public SceneField SectionScene => sectionScene;
    
    public bool SetActiveSceneToSectionScene => setActiveSceneToSectionScene;
    
    public static LevelSectionSceneInfo Create(SceneField sectionPersistentData, SceneField sectionScene)
    {
        var info = CreateInstance<LevelSectionSceneInfo>();
        
        info.sectionPersistentData = sectionPersistentData;
        info.sectionScene = sectionScene;
        
        return info;
    }
}