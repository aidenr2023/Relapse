/*
 * From https://discussions.unity.com/t/inspector-field-for-scene-asset/40763/5
 */

using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class SceneField
{
    [SerializeField] protected Object sceneAsset;

    [SerializeField] protected string sceneName = "";
    public string SceneName => sceneName;

    // makes it work with the existing Unity methods (LoadLevel/LoadScene)
    public static implicit operator string(SceneField sceneField)
    {
        return sceneField.SceneName;
    }

    public static bool operator ==(SceneField a, object b)
    {
        // If A is null, but not B, return if the scene asset is null or the scene name is null or empty
        if (!ReferenceEquals(a, null) && ReferenceEquals(b, null))
        {
            return a.sceneAsset == null || a.sceneAsset.Equals(null) || a.sceneName == null ||
                   a.sceneName.Equals(null) || a.sceneName.Equals("");
        }
        
        // If A is null and B is not null, call the function again with the parameters reversed
        if (ReferenceEquals(a, null) && !ReferenceEquals(b, null))
            return b == a;
        
        // If A is not null and B is not null, return if the scene asset is not null and the scene asset equals B
        return a.sceneAsset != null && a.Equals(b);
    }

    public static bool operator !=(SceneField a, object b)
    {
        return !(a == b);
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(SceneField))]
public class SceneFieldPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        // Begin drawing the property
        EditorGUI.BeginProperty(_position, GUIContent.none, _property);

        // Get the scene asset and scene name properties
        var sceneAsset = _property.FindPropertyRelative("sceneAsset");
        var sceneName = _property.FindPropertyRelative("sceneName");

        // Set the position to the prefix label
        _position = EditorGUI.PrefixLabel(_position, GUIUtility.GetControlID(FocusType.Passive), _label);

        // If the scene asset is not null
        if (sceneAsset != null)
        {
            // Set the scene asset to the object field
            sceneAsset.objectReferenceValue =
                EditorGUI.ObjectField(_position, sceneAsset.objectReferenceValue, typeof(SceneAsset), false);

            // If the scene asset is not null
            if (sceneAsset.objectReferenceValue != null)
                sceneName.stringValue = (sceneAsset.objectReferenceValue as SceneAsset).name;
            else
            {
                // If the scene asset is null, reset the scene asset and scene name
                sceneAsset.objectReferenceValue = null;
                sceneName.stringValue = "";
            }
        }
        else
        {
            sceneAsset.objectReferenceValue = null;
            sceneName.stringValue = "";
        }

        EditorGUI.EndProperty();
    }
}
#endif