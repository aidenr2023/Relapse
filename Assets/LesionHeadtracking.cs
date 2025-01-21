using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LesionHeadtracking : MonoBehaviour
{
    // Variables
    [SerializeField] private MultiAimConstraint headConstraint;
    [SerializeField] private MultiAimConstraint hipConstraint;
    [SerializeField] private StandardEnemyDetection enemyDetection;
    [Range(0, 1)] [SerializeField] private float weight = 1;

    private void Start()
    {
        // Start a coroutine to find the Player and assign constraints once it's ready
        StartCoroutine(WaitForPlayerAndAssignConstraints());
    }

    private IEnumerator WaitForPlayerAndAssignConstraints()
    {
        // Wait for the Player object to be present in the scene
        GameObject player = null;
        while (player == null)
        {
            player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("Player not found. Retrying...");
                yield return null; // Wait one frame before retrying
            }
        }

        Debug.Log("Player found. Assigning constraints.");

        // Assign the Player transform to the head constraint
        var headSources = headConstraint.data.sourceObjects;
        headSources.SetTransform(0, player.transform);
        headSources.SetWeight(0, 1f);
        headConstraint.data.sourceObjects = headSources;

        // Assign the Player transform to the hip constraint
        var hipSources = hipConstraint.data.sourceObjects;
        hipSources.SetTransform(0, player.transform);
        hipSources.SetWeight(0, 1f);
        hipConstraint.data.sourceObjects = hipSources;

        Debug.Log("Constraints assigned successfully.");
    }

    private void Update()
    {
        // Continuously update the constraint weights based on the enemy detection state
        SetWeight();
    }

    private void SetWeight()
    {
        // If the enemy is aware, smoothly transition constraints to 'weight' (e.g., 1.0)
        if (enemyDetection.CurrentDetectionState == EnemyDetectionState.Aware)
        {
            headConstraint.weight = Mathf.Lerp(headConstraint.weight, weight, Time.deltaTime);
            hipConstraint.weight = Mathf.Lerp(hipConstraint.weight, weight, Time.deltaTime);
        }
        // Otherwise, smoothly transition constraints back to 0
        else
        {
            headConstraint.weight = Mathf.Lerp(headConstraint.weight, 0f, Time.deltaTime);
            hipConstraint.weight = Mathf.Lerp(hipConstraint.weight, 0f, Time.deltaTime);
        }
    }
}
