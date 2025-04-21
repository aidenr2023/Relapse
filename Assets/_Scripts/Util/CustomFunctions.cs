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

    [MenuItem("Custom Tools/Lights/Find All Realtime Lights")]
    private static void FindAllRealtimeLights()
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

        Debug.Log($"Found {realtimeLights.Length} Realtime Lights in the scene");

        // Log all the lights
        foreach (var light in realtimeLights)
            Debug.Log($"Realtime Light: {light.name} - [{light.type}] - [{light.lightmapBakeType}]", light);
    }

    [MenuItem("Custom Tools/Collider/Find All Triggers Non-Non-Physical")]
    private static void FindAllTriggersNonNonPhysical()
    {
        var ignoreLayers =
            LayerMask.GetMask("NonPhysical", "Player", "Actor");

        // Get all the triggers in the scene
        var triggers = Object
            .FindObjectsOfType<Collider>(true)
            // Skip objects that are in any of the ignore layers
            .Where(n =>
                n.isTrigger &&
                ignoreLayers != (ignoreLayers | (1 << n.gameObject.layer))
            )
            .ToArray();

        Debug.Log($"Found {triggers.Length} Non-non-physical triggers in the scene");

        // Log all the triggers
        foreach (var trigger in triggers)
            Debug.Log($"Trigger: {trigger.name} - Layer: {LayerMask.LayerToName(trigger.gameObject.layer)}", trigger);
    }

    [MenuItem("Custom Tools/Audio/Find All Audio Sources")]
    private static void LogAllAudioSources()
    {
        // Get all the audio sources in the scene
        var audioSources = Object
            .FindObjectsOfType<AudioSource>(true)
            // Skip objects that are in any of the ignore layers
            .ToArray();

        Debug.Log($"Found {audioSources.Length} Audio Sources in the scene");

        // Log all the audio sources
        foreach (var audioSource in audioSources)
            Debug.Log($"Audio Source: {audioSource.name} - Mixer: {audioSource.outputAudioMixerGroup?.name ?? "No Mixer"}", audioSource);
    }
    
    [MenuItem("Custom Tools/Audio/Find All Audio Sources No Mixer")]
    private static void FindAllAudioSourcesNoMixer()
    {
        // Get all the audio sources in the scene
        var audioSources = Object
            .FindObjectsOfType<AudioSource>(true)
            // Skip objects that are in any of the ignore layers
            .Where(n => n.outputAudioMixerGroup == null)
            .ToArray();
        
        // Log all the audio sources
        foreach (var audioSource in audioSources)
            Debug.Log($"Audio Source: {audioSource.name} - No Mixer", audioSource);
    }

#endif
}