using System;
using System.Collections.Generic;
using UnityEngine;

public class ScriptExtender : MonoBehaviour
{
    public event Action<ScriptExtender> OnObjectUpdate;
    public event Action<ScriptExtender> OnObjectFixedUpdate;
    
    public event Action<ScriptExtender> OnObjectEnabled;
    public event Action<ScriptExtender> OnObjectDisabled;
    public event Action<ScriptExtender> OnObjectDestroyed;

    public event Action<ScriptExtender, Collider> TriggerEnter;
    public event Action<ScriptExtender, Collider> TriggerExit;
    public event Action<ScriptExtender, Collider> TriggerStay;
    
    public event Action<ScriptExtender, Collision> ColliderEnter;
    public event Action<ScriptExtender, Collision> ColliderExit;
    public event Action<ScriptExtender, Collision> ColliderStay;

    private Dictionary<Type, Component> _components;

    private void Awake()
    {
        // Create a new dictionary to store the components
        _components = new Dictionary<Type, Component>();

        // Loop through all the components & add them to the dictionary
        foreach (var component in GetComponents<Component>())
            _components.Add(component.GetType(), component);
    }

    #region Unity Event Functions

    private void OnEnable()
    {
        OnObjectEnabled?.Invoke(this);
    }

    private void OnDisable()
    {
        OnObjectDisabled?.Invoke(this);
    }

    private void Update()
    {
        OnObjectUpdate?.Invoke(this);
    }
    
    private void FixedUpdate()
    {
        OnObjectFixedUpdate?.Invoke(this);
    }

    private void OnDestroy()
    {
        OnObjectDestroyed?.Invoke(this);
    }

    private void OnTriggerEnter(Collider other) => TriggerEnter?.Invoke(this, other);

    private void OnTriggerExit(Collider other) => TriggerExit?.Invoke(this, other);

    private void OnTriggerStay(Collider other) => TriggerStay?.Invoke(this, other);

    private void OnCollisionEnter(Collision other) => ColliderEnter?.Invoke(this, other);

    private void OnCollisionExit(Collision other) => ColliderExit?.Invoke(this, other);

    private void OnCollisionStay(Collision other) => ColliderStay?.Invoke(this, other);

    #endregion

    public T ExtenderAddComponent<T>() where T : Component
    {
        // Check if a component of type T already exists
        // If it does, return that component
        if (_components.ContainsKey(typeof(T)))
            return (T)_components[typeof(T)];

        // Create a new component of type T and add it to the game object
        var component = gameObject.AddComponent<T>();

        // Add that component to the dictionary
        _components.Add(typeof(T), component);

        // Return the component
        return component;
    }

    public T ExtenderGetComponent<T>() where T : Component
    {
        return (T)_components[typeof(T)];
    }
}