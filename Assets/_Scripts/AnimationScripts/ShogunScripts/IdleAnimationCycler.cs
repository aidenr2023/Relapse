using UnityEngine;
using System.Collections;

public class IdleAnimationCycler : MonoBehaviour
{
    private Animator animator;
    private int _idleHash;
    private Coroutine _cycleRoutine;

    private void Awake()
    {
        _idleHash = Animator.StringToHash("IdleNum");
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        // Start cycling when component is enabled
        _cycleRoutine = StartCoroutine(CycleIdles());
    }

    private void OnDisable()
    {
        // Stop cycling when component is disabled
        if (_cycleRoutine != null)
            StopCoroutine(_cycleRoutine);
    }

    private IEnumerator CycleIdles()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(10, 30));
            //log the seconds that past since the start of the game
            Debug.Log(Time.timeSinceLevelLoad);
            
            animator.SetInteger(_idleHash, Random.Range(1, 4));
        }
    }
}