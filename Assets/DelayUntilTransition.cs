using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayUntilTransition : MonoBehaviour
{
    [SerializeField] public float delay;
    [SerializeField] LevelTransitionCheckpoint LevelTransitionCheckpoint;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Delay());
    }

    // Update is called once per frame
    void Update()
    {
    }

    private IEnumerator Delay()
    {
        yield return new WaitForSeconds(delay);
        LevelTransitionCheckpoint.ForceTransitionToNextScene();
    }
}
