using System;
using System.Collections.Generic;
using UnityEngine;

public class PickupIconManagerHelper : MonoBehaviour
{
    [SerializeField] private GameObject pickupIconPrefab;
    [SerializeField] private Vector3 offset;

    private readonly Dictionary<IInteractable, GameObject> _pickupIcons = new();

    private void OnEnable()
    {
        // Subscribe to events
        PickupIconManager.onInteractableAdded += OnInteractableAdded;
        PickupIconManager.onInteractableRemoved += OnInteractableRemoved;
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        PickupIconManager.onInteractableAdded -= OnInteractableAdded;
        PickupIconManager.onInteractableRemoved -= OnInteractableRemoved;
    }

    private void OnInteractableAdded(IInteractable obj)
    {
        // Return if the interactable is null
        if (obj == null)
            return;

        // Return if the interactable already has an icon
        if (_pickupIcons.ContainsKey(obj))
            return;

        // Instantiate the pickup icon prefab
        // Set the parent of the icon without changing the scale
        var icon = Instantiate(pickupIconPrefab);

        // Add the interactable to the dictionary
        _pickupIcons.Add(obj, icon);

        // Add a follow transform to the icon
        var followTransform = icon.AddComponent<FollowTransform>();

        // Set the transform and offset of the follow transform
        followTransform.SetTargetTransform(obj.GameObject.transform);
        followTransform.SetFollowOffset(offset);
    }

    private void OnInteractableRemoved(IInteractable obj)
    {
        // Return if the interactable is null
        if (obj == null)
            return;

        // Return if the interactable does not have an icon
        if (!_pickupIcons.TryGetValue(obj, out var icon))
            return;

        // Remove the interactable from the dictionary
        _pickupIcons.Remove(obj);

        // Destroy the icon
        Destroy(icon);
    }
}