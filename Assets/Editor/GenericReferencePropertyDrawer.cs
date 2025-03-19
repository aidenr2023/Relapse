using UnityEditor;
using UnityEngine;

/// <summary>
/// A class that draws the GenericReference in the inspector.
/// If the object is set to use a constant value, it will draw the constant value.
/// Otherwise, it will draw the slot for the scriptable object.
/// There is a dropdown to select if the object is using a constant value or a scriptable object.
/// </summary>
[CustomPropertyDrawer(typeof(GenericReference<,>), true)]
public class GenericReferencePropertyDrawer : PropertyDrawer
{
    // Code from: https://www.reddit.com/r/Unity3D/comments/iqo99y/how_can_i_achieve_this_dropdown_effect_in_a/?rdt=42967

    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, prop);
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calculate rects
        var dropdownRect = new Rect(position.x, position.y, 20, position.height);

        //get bool property
        var useConstantProp = prop.FindPropertyRelative("useConstant");

        // remove background
        GUI.backgroundColor = new Color(0, 0, 0, 0);
        GUI.contentColor = new Color(0, 0, 0, 0);

        // Draw Icon
        var iconRect = new Rect(position.x, position.y, 20, EditorGUIUtility.singleLineHeight);
        Texture icon = EditorGUIUtility.Load("icons/d_UnityEditor.SceneHierarchyWindow.png") as Texture2D;
        GUI.DrawTexture(iconRect, icon);

        // Create Popup and find bool value
        var popup = EditorGUI.Popup(
            dropdownRect, useConstantProp.boolValue ? 0 : 1,
            new[] { "Use Constant", "Use Variable" }
        );
        useConstantProp.boolValue = popup == 0;

        // Return colours
        GUI.backgroundColor = Color.white;
        GUI.contentColor = Color.white;

        // show appropriate input
        var displayProperty = useConstantProp.boolValue
            ? prop.FindPropertyRelative("constantValue")
            : prop.FindPropertyRelative("variable");

        // Get the height of the display property
        var cHeight = EditorGUI.GetPropertyHeight(displayProperty, GUIContent.none, true);

        var inputRect = new Rect(position.x + 20, position.y, position.width - 20, cHeight);

        EditorGUI.PropertyField(inputRect, displayProperty, GUIContent.none);

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty prop, GUIContent label)
    {
        var useConstantProp = prop.FindPropertyRelative("useConstant");

        // show appropriate input
        var displayProperty = useConstantProp.boolValue
            ? prop.FindPropertyRelative("constantValue")
            : prop.FindPropertyRelative("variable");
        
        return EditorGUI.GetPropertyHeight(displayProperty, GUIContent.none, true);
    }
}