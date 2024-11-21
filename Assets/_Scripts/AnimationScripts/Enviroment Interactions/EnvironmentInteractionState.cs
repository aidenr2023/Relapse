using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public abstract class EnvironmentInteractionState : BaseState<EnvironmentInteractionStateMachine.EEnvironmentInteractionState>
{
    protected EnvironmentInteractionContext Context;
    
    public EnvironmentInteractionState(EnvironmentInteractionContext context, EnvironmentInteractionStateMachine.EEnvironmentInteractionState key) : base(key)
    {
        Context = context;
    }
    
    //get closest point of collider and object
    private Vector3 GetClosestPointOnCollider(Collider intersectingCollider, Vector3 positionToCheck)
    {
        return intersectingCollider.ClosestPoint(positionToCheck);
    }
    
    //Start Ik Position tracking
    protected void StartIKTrackingPositionTracking(Collider intersectingCollider)
    {
        if(intersectingCollider.gameObject.layer == LayerMask.NameToLayer("Interactable") && Context._currentIntersectingCollider == null)
        {
            Context._currentIntersectingCollider = intersectingCollider;
            //Get the closest point on the collider
            Vector3 closestPointFromRoot = GetClosestPointOnCollider(intersectingCollider, Context.RootTransform.position);
            Context.SetCurrentSide(closestPointFromRoot);
            
            SetIKTargetPosition();
        }

       
        //
        // //Stop Ik Position tracking
        // protected void StopIKTracking(Collider intersectingCollider)
        // {
        //
        // }

    }
    
     protected void UpdateIKTargetPostionTracking(Collider intersectingCollider)
     {
            if (intersectingCollider == Context._currentIntersectingCollider)
            {
                SetIKTargetPosition();
            }
     }

    protected void ResetIKTracking(Collider intersectingCollider)
    {
        if (intersectingCollider == Context._currentIntersectingCollider)
        {
            Context._currentIntersectingCollider = null;
            Context.ClosestPointOnColliderFromShoulder = Vector3.positiveInfinity;
        }
    }
    
    private void SetIKTargetPosition()
    {
        Context.ClosestPointOnColliderFromShoulder = GetClosestPointOnCollider(Context._currentIntersectingCollider, 
        new Vector3(Context._currentShoulderTransform.position.x, Context.CharacterShoulderHeight, 
        Context._currentShoulderTransform.position.z));
        
        Vector3 rayDirection =  Context._currentShoulderTransform.position - Context.ClosestPointOnColliderFromShoulder;
        Vector3 normalizedRayDirection = rayDirection.normalized;
        float offsetDistance = 0.05f;
        Vector3 offset = normalizedRayDirection * offsetDistance;
        
        Vector3 offsetPosition = Context.ClosestPointOnColliderFromShoulder + offset;
Context._currentIKTarget.position = new Vector3(offsetPosition.x, Context.InteractionPointOffsetY, offsetPosition.z);        
        
    }
}
