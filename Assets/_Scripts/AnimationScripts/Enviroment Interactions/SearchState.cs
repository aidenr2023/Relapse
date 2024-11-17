using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SearchState : EnvironmentInteractionState
{
    public SearchState(EnvironmentInteractionContext context,
        EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate) : base(context, estate)
    {
        EnvironmentInteractionContext Context = context;
    }

    public override void EnterState()
    {
        //Debug.Log("Entering the Rest State");
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
