using System;
using UnityEngine;

public abstract class ComponentScript<TComponent> : MonoBehaviour
{
    public TComponent ParentComponent { get; private set; }

    private void Awake()
    {
        // Get the parent component
        ParentComponent = GetComponent<TComponent>();

        // Assert that the parent component is not null
        Debug.Assert(ParentComponent != null, $"Add a {typeof(TComponent).Name} component to {name}! It needs it!");

        // Call the custom awake method
        CustomAwake();
    }

    /// <summary>
    /// A custom awake function that is called after the parent component is set.
    /// </summary>
    protected virtual void CustomAwake()
    {
    }
}