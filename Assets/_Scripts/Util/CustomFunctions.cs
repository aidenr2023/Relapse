using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class CustomFunctions
{
    private const float DEFAULT_FRAME_AMOUNT = 1 / 60f;
    private const float FIXED_FRAME_AMOUNT = 1 / 50f;
    private const int MAX_COMPONENT_SEARCH_DEPTH = 20;

    public static TComponent GetComponentInParent<TComponent>(
        this Component mb,
        int maxDepth = MAX_COMPONENT_SEARCH_DEPTH
    )
    {
        var cItem = mb.transform;

        for (var i = 0; i < maxDepth && cItem != null; i++)
        {
            // Try to get the component
            var found = cItem.TryGetComponent(out TComponent component);

            // If there is a component, return it
            if (found)
                return component;

            // Move to the parent of the current item
            cItem = cItem.transform.parent;
        }

        // There is none of the component in any of the parents
        return default;
    }

    public static bool TryGetComponentInParent<TComponent>(
        this Component mb,
        out TComponent component,
        int maxDepth = MAX_COMPONENT_SEARCH_DEPTH
    )
    {
        var cItem = mb.transform;

        for (var i = 0; i < maxDepth && cItem != null; i++)
        {
            // Try to get the component
            var found = cItem.TryGetComponent(out component);

            // If there is a component, return it
            if (found)
                return true;

            // Move to the parent of the current item
            cItem = cItem.transform.parent;
        }

        // There is none of the component in any of the parents
        component = default;

        return false;
    }

    public static float FrameAmount(float lerpAmount, bool isFixed = false, bool isUnscaled = false)
    {
        var frameAmount = isFixed ? FIXED_FRAME_AMOUNT : DEFAULT_FRAME_AMOUNT;

        float deltaTime;

        if (isUnscaled)
            deltaTime = isFixed ? Time.fixedUnscaledDeltaTime : Time.unscaledDeltaTime;
        else
            deltaTime = isFixed ? Time.fixedDeltaTime : Time.deltaTime;

        return deltaTime / frameAmount * lerpAmount;
    }

    public static float FrameAmount(float lerpAmount, float deltaTime, bool isFixed)
    {
        var frameAmount = isFixed ? FIXED_FRAME_AMOUNT : DEFAULT_FRAME_AMOUNT;

        return deltaTime / frameAmount * lerpAmount;
    }

    public static void DrawArrow(
        Vector3 position, Vector3 forward,
        float arrowLength = 3, float arrowYOffset = 2, float arrowAngleSize = 30
    )
    {
        var arrowStart = position - forward * arrowLength / 2 + Vector3.up * arrowYOffset;
        var arrowEnd = position + forward * arrowLength + Vector3.up * arrowYOffset;

        // Draw the forward of the respawn point
        Gizmos.DrawLine(arrowStart, arrowEnd);

        // Draw the arrow head
        Gizmos.DrawLine(
            arrowEnd,
            arrowEnd + Quaternion.Euler(0, arrowAngleSize, 0) * -forward * arrowLength / 4
        );
        Gizmos.DrawLine(
            arrowEnd,
            arrowEnd + Quaternion.Euler(0, -arrowAngleSize, 0) * -forward * arrowLength / 4
        );
    }

    public static bool IsNotNull(this UnityEngine.Object obj) => obj != null;

    public static Result<T> NullCheckToResult<T>(this T obj, string valueName = "Value") where T : UnityEngine.Object
    {
        if (obj == null)
            return Result<T>.Error($"{valueName} is null!");

        return Result<T>.Ok(obj);
    }

    public static Result<T> BasicNullCheck<T>(this T obj, string valueName = "Value") 
    {
        if (obj is null)
            return Result<T>.Error($"{valueName} is null!");

        return Result<T>.Ok(obj);
    }

    public static Result<T> BoolToResult<T>(this bool condition, T value)
    {
        return Result<T>.BoolToResult(value, v => condition);
    }
    
    public static Option<T> ToSome<T>(this T obj)
    {
        return Option<T>.Some(obj);
    }

#if UNITY_EDITOR
    // Create a button for this in the top toolbar
    public static void FixNegativeBoxCollider(BoxCollider box, bool logHistory)
    {
        // If the box is null, return
        if (box == null)
            return;

        var lossy = box.transform.lossyScale;

        var flip = new Vector3(
            Mathf.Sign(lossy.x),
            Mathf.Sign(lossy.y),
            Mathf.Sign(lossy.z));

        var sign = new Vector3(
            Mathf.Sign(box.size.x),
            Mathf.Sign(box.size.y),
            Mathf.Sign(box.size.z));

        if (flip == sign)
            return;

        if (logHistory)
            Undo.RecordObject(box, "Fix Negative Box Collider");

        box.size = Vector3.Scale(box.size, flip);

        Debug.Log($"Fixed Box Collider: {box.name}", box);
    }

    // [MenuItem("Custom Tools/Collider/Fix Selected Negative Box Collider")]
    private static void FixSelectedNegativeBoxColliders()
    {
        var boxes = Selection
            .GetTransforms(SelectionMode.Editable)
            .Select(n => n.GetComponent<BoxCollider>())
            .Where(n => n != null)
            .ToArray();

        // If there are no boxes, return
        if (boxes.Length == 0)
        {
            Debug.LogWarning("No Box Colliders selected!");
            return;
        }

        // Record the undo action
        Undo.RecordObjects(boxes, "Fix Selected Negative Box Colliders");

        // Fix the boxes
        foreach (var box in boxes)
            FixNegativeBoxCollider(box, false);
    }

    [MenuItem("Custom Tools/Collider/Fix All Negative Box Collider")]
    private static void FixAllNegativeBoxColliders()
    {
        var boxes = Object.FindObjectsOfType<BoxCollider>(true);

        // If there are no boxes, return
        if (boxes.Length == 0)
        {
            Debug.LogWarning("No Box Colliders selected!");
            return;
        }

        // Record the undo action
        Undo.RecordObjects(boxes, "Fix ALL Negative Box Colliders");

        // Fix the boxes
        foreach (var box in boxes)
            FixNegativeBoxCollider(box, false);
    }

    [MenuItem("Custom Tools/Lights/Select All Realtime Lights")]
    private static void SelectAllRealtimeLights()
    {
        // Get all the lights in the scene
        var lights = Object.FindObjectsOfType<Light>(true);

        // If there are no lights, return
        if (lights.Length == 0)
        {
            Debug.LogWarning("No Lights in the scene!");
            return;
        }

        // get all the lights
        var realtimeLights = lights
            .Where(n => n.lightmapBakeType == LightmapBakeType.Realtime)
            .ToArray();
        
        // Select all the lights with context
        foreach (var light in realtimeLights)
        {
            // Select the light
            Selection.activeGameObject = light.gameObject;
        
            // Select the light
            EditorGUIUtility.PingObject(light);
        }
        
        // Then, select all the lights
        Selection.objects = realtimeLights
            .Select(n => n.gameObject)
            .ToArray();
        
        Debug.Log($"Selected {Selection.objects.Length} Directional Lights");
    }
    
#endif
}