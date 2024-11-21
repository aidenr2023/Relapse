using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchState : EnvironmentInteractionState
{
    public float _approachDistanceThreshold = 9.0f;
    public SearchState(EnvironmentInteractionContext context,
        EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate) : base(context, estate)
    {
        EnvironmentInteractionContext Context = context;
    }

    public override void EnterState()
    {
        Debug.Log("Entering the search State");
        
    }

    public override void ExitState()
    {
        //Debug.Log("Exiting the Rest State");
    }

    public override void UpdateState()
    {
       // Debug.Log("Updating the Rest State");
    }

    public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
    {
        bool isCloseToTarget = Vector3.Distance(Context.ClosestPointOnColliderFromShoulder, Context.RootTransform.position) <= _approachDistanceThreshold;
        Debug.Log($"isCloseToTarget: {isCloseToTarget}, Distance: {Vector3.Distance(Context.ClosestPointOnColliderFromShoulder, Context.RootTransform.position)}, Threshold: {_approachDistanceThreshold}");

        bool isClosestPointColliderValid = Context.ClosestPointOnColliderFromShoulder != Vector3.positiveInfinity;
        Debug.Log($"isClosestPointColliderValid: {isClosestPointColliderValid}, ClosestPointOnColliderFromShoulder: {Context.ClosestPointOnColliderFromShoulder}");

        if (isCloseToTarget && isClosestPointColliderValid)
        {
            Debug.Log("Transitioning to the Approach State");
            return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Approach;
        }

        Debug.Log("Condition failed. Returning current state.");
        return StateKey;

    }

    public override void OnTriggerEnter(Collider other)
    {
        StartIKTrackingPositionTracking(other);
    }

    public override void OnTriggerStay(Collider other)
    {
        UpdateIKTargetPostionTracking(other);
    }

    public override void OnTriggerExit(Collider other)
    {
        ResetIKTracking(other);
    }
}
