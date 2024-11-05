using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class CustomEventManager : MonoBehaviour
{
    [SerializeField] private UnityEvent OnAwakeEvent;
    [SerializeField] private UnityEvent OnStartEvent;
    [SerializeField] private UnityEvent OnEnableEvent;
    [SerializeField] private UnityEvent OnDisableEvent;
    [SerializeField] private UnityEvent OnDestroyEvent;

    private void Awake()
    {
        OnAwakeEvent.Invoke();
    }

    private void Start()
    {
        OnStartEvent.Invoke();
    }

    private void OnEnable()
    {
        OnEnableEvent.Invoke();
    }

    private void OnDisable()
    {
        OnDisableEvent.Invoke();
    }

    private void OnDestroy()
    {
        OnDestroyEvent.Invoke();
    }
}