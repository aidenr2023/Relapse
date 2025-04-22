using UnityEngine;

[CreateAssetMenu(fileName = "LevelStartupSceneInfo", menuName = "Scene Management/Level Startup Scene Info")]
public class LevelStartupSceneInfo : ScriptableObject
{
    [SerializeField] private SceneField playerDataScene;
    [SerializeField] private SceneField activeScene;
    [SerializeField] private LevelSectionSceneInfo[] startupSections;

    public SceneField PlayerDataScene => playerDataScene;
    public SceneField ActiveScene => activeScene;
    public LevelSectionSceneInfo[] StartupSections => startupSections;
    
    public static LevelStartupSceneInfo Create(SceneField playerDataScene, SceneField activeScene, params LevelSectionSceneInfo[] startupSections)
    {
        var info = CreateInstance<LevelStartupSceneInfo>();

        info.playerDataScene = playerDataScene;
        info.activeScene = activeScene;
        info.startupSections = startupSections;

        return info;
    }
}