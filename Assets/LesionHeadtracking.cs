using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LesionHeadtracking : MonoBehaviour
{
    // Variables
    [SerializeField] private MultiAimConstraint headConstraint;
    [SerializeField] private MultiAimConstraint hipConstraint;
    [SerializeField] private StandardEnemyDetection enemyDetection;
    [Range(0, 1)] [SerializeField] private float weight = 1;

    private EnemyDetectionState lastState;

    private void Awake()
    {
        if (headConstraint == null || hipConstraint == null)
        {
            Debug.LogError("Constraints or EnemyDetection are not assigned!", this);
            enabled = false;
            return;
        }

        FindPlayer();
    }

    private void Update()
    {
        // If the detection state has changed, update the weight
        if (lastState != enemyDetection.CurrentDetectionState)
        {
            SetWeight();
            lastState = enemyDetection.CurrentDetectionState;
        }
    }

    private void FindPlayer()
    {
        var player = GameObject.FindWithTag("Player");
        if (player)
        {
            Debug.Log("Player found. Assigning constraints.");
            var sourceObjects = headConstraint.data.sourceObjects;
            sourceObjects.SetTransform(0, player.transform);
            headConstraint.data.sourceObjects = sourceObjects;

            sourceObjects = hipConstraint.data.sourceObjects;
            sourceObjects.SetTransform(0, player.transform);
            hipConstraint.data.sourceObjects = sourceObjects;
        }
        else
        {
            Debug.LogError("Player not found in the scene!");
        }
    }

    private void SetWeight()
    {
        if (enemyDetection.CurrentDetectionState == EnemyDetectionState.Aware)
        {
        //lerp the weight to Weight variable
            headConstraint.weight = Mathf.Lerp(headConstraint.weight, weight, Time.deltaTime);
            hipConstraint.weight = Mathf.Lerp(hipConstraint.weight, weight, Time.deltaTime);
        
        }
        // else if (enemyDetection.CurrentDetectionState == EnemyDetectionState.Curious)
        // {
        //     headConstraint.weight = Mathf.Lerp(headConstraint.weight, weight / 2, Time.deltaTime);
        //     hipConstraint.weight = Mathf.Lerp(hipConstraint.weight, weight / 2, Time.deltaTime);
       // }
        else
        {
            headConstraint.weight = Mathf.Lerp(headConstraint.weight, 0, Time.deltaTime);
            hipConstraint.weight = Mathf.Lerp(hipConstraint.weight, 0, Time.deltaTime);
            
        }
    }
}
