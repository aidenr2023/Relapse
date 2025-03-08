using UnityEngine;
using System.Collections;

public class TutorialAnimation : MonoBehaviour
{
    public GameObject[] enemies;

    // Assign your Animator in the Inspector.
    public Animator Animator; 
    // Type the exact state name from the Animator’s state machine here.
    public string AnimationName;

    // This will ensure we only trigger the animation once.
    private bool hasTriggeredAnimation = false;

    void Update()
    {
        // If the animation hasn't been triggered yet and all enemies are destroyed
        if (!hasTriggeredAnimation && AllEnemiesDestroyed())
        {
            hasTriggeredAnimation = true;
            Debug.Log("All enemies have been destroyed!");
            StartCoroutine(PlayAnimation());
        }
    }

    private bool AllEnemiesDestroyed()
    {
        // If any enemy in the array is not null, the function returns false.
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator PlayAnimation()
    {
        // If we have a valid Animator and a non-empty animation name, play the animation
        if (Animator && !string.IsNullOrEmpty(AnimationName))
        {
            Animator.Play(AnimationName);
            Debug.Log("PlayAnimation");
        }

        // We can just yield return null since we only need to start the animation
        yield return null;
    }
}
