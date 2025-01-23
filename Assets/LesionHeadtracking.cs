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
        // Wait one frame before retrying
        while (Player.Instance == null)
            yield return null;

        // Debug.Log("Player found. Assigning constraints.");

        // Assign the Player transform to the head constraint
        SetSourceWeight(headConstraint, Player.Instance.transform);

        // Assign the Player transform to the hip constraint
        SetSourceWeight(hipConstraint, Player.Instance.transform);

        // Debug.Log("Constraints assigned successfully.");
    }

    private void Update()
    {
        // Continuously update the constraint weights based on the enemy detection state
        SetWeight();
    }

    private void SetWeight()
    {
        // If there is no player instance, return
        if (Player.Instance == null)
            return;

        var targetWeight = 0f;

        // If the enemy is aware, smoothly transition constraints to 'weight' (e.g., 1.0)
        // Otherwise, smoothly transition constraints back to 0
        if (enemyDetection.CurrentDetectionState == EnemyDetectionState.Aware)
            targetWeight = weight;

        headConstraint.weight = Mathf.Lerp(headConstraint.weight, targetWeight, Time.deltaTime);
        hipConstraint.weight = Mathf.Lerp(hipConstraint.weight, targetWeight, Time.deltaTime);

        // Debug.Log($"Set to target: {Player.Instance}");
    }

    private void SetSourceWeight(MultiAimConstraint constraint, Transform targetTransform)
    {
        if (targetTransform == null)
            return;
        
        var sources = constraint.data.sourceObjects;
        sources.SetTransform(0, targetTransform);
        sources.SetWeight(0, 1f);
        constraint.data.sourceObjects = sources;
    }
}