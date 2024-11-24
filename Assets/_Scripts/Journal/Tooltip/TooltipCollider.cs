using System;
using UnityEngine;

public class TooltipCollider : MonoBehaviour
{
    [SerializeField] [TextArea(1, 8)] private string tooltipText;

    [SerializeField] private bool destroyOnEnter;

    private Collider _collider;

    private void Awake()
    {
        // Get the collider component
        _collider = GetComponent<Collider>();

        // Assert that the collider is not null
        if (_collider == null)
            throw new NullReferenceException("TooltipCollider: Collider is null.");
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the other collider is the player
        if (!other.CompareTag("Player"))
            return;

        // Show the tooltip
        JournalTooltipManager.Instance.AddTooltip(tooltipText);

        // Destroy the object if destroyOnEnter is true
        if (destroyOnEnter)
            Destroy(gameObject);
    }
}