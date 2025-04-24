using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DelayUntilTransition : MonoBehaviour
{
    [SerializeField] public float delay;
    [SerializeField] private LevelTransitionCheckpoint LevelTransitionCheckpoint;
    [SerializeField] private UnityEvent onDelayComplete;

    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(Delay());
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(delay);
        
        // Invoke the event
        onDelayComplete?.Invoke();
        
        // Transition to the next scene
        if (LevelTransitionCheckpoint == null)
            yield break;
        
        LevelTransitionCheckpoint.ForceTransitionToNextScene();
    }
}