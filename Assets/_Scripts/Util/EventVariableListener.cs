using UnityEngine;
using UnityEngine.Events;

public class EventVariableListener : MonoBehaviour
{
    [SerializeField] private EventVariable eventVariable;
    [SerializeField] private UnityEvent onEventRaised;

    private void OnEnable()
    {
        // Bind the event to the listener
        eventVariable.Value.AddListener(OnEventRaised);
    }

    private void OnDisable()
    {
        // Unbind the event from the listener
        eventVariable.Value.RemoveListener(OnEventRaised);
    }

    private void OnEventRaised()
    {
        onEventRaised.Invoke();
    }
}