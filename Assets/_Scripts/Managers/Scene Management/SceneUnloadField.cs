using UnityEngine;
using UnityEngine.SceneManagement;


[System.Serializable]
public class SceneUnloadField
{
    [SerializeField] private SceneField sceneField;

    [SerializeField] private bool isDisableInstead;

    public SceneField SceneField => sceneField;

    public bool IsDisableInstead
    {
        get => isDisableInstead;

        // TODO: Possibly delete this line
        set => isDisableInstead = value;
    }

    public static implicit operator SceneField(SceneUnloadField sceneUnloadField) =>
        sceneUnloadField.sceneField;

    public static implicit operator string(SceneUnloadField sceneUnloadField) =>
        sceneUnloadField.sceneField;

    public static SceneUnloadField Create(SceneField sceneField, bool isDisableInstead) =>
        new SceneUnloadField
        {
            sceneField = sceneField,
            isDisableInstead = isDisableInstead
        };
}