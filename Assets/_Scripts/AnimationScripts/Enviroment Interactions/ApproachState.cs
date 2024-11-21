using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApproachState : EnvironmentInteractionState
{
    public ApproachState(EnvironmentInteractionContext context,
        EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate) : base(context, estate)
    {
        EnvironmentInteractionContext Context = context;
    } 
    [SerializeField] private float _approachWeight = .5f;
    [SerializeField] private float lerpDuration = 5f;
    [SerializeField] private float elapsedTime = 0f; 
    [SerializeField] private float _rotationspeed = 500f; 
    [SerializeField] private float _approachRotationWeight = .75f;
    

    public override void EnterState()
    {
        Debug.Log("Entering the Approach State");
        Context._currentIKConstraint.weight = .4f;
        elapsedTime = 0f;
        
    }

    public override void ExitState()
    {
        //Debug.Log("Exiting the Rest State");
    }

    public override void UpdateState()
    { 
        Debug.Log("Updating the Approach State");
        Quaternion expectedGroundRotation = Quaternion.LookRotation(-Vector3.up, Context.RootTransform.forward);
        elapsedTime += Time.deltaTime;

        Context._currentIKTarget.rotation = Quaternion.RotateTowards(Context._currentIKTarget.rotation, 
        expectedGroundRotation, _rotationspeed * Time.deltaTime);
        
        //lerp the rotation weight of the ik constraint 
        Context._currentMultiRotationConstraint.weight = Mathf.Lerp(Context._currentMultiRotationConstraint.weight,_approachRotationWeight,
            elapsedTime/ lerpDuration );        
        
        //lerp the weight of the ik constraint to approach weight
        Context._currentIKConstraint.weight = Mathf.Lerp(Context._currentIKConstraint.weight,_approachWeight,
         elapsedTime/ lerpDuration );
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
