using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetState : EnvironmentInteractionState
{
   float _elapsedTime = 0f;
   float resetDuration = 2f;
   float lerpDuration = 10f;
   [SerializeField] private float _resetDuration = 5f;
   public ResetState(EnvironmentInteractionContext context,
      EnvironmentInteractionStateMachine.EEnvironmentInteractionState estate) : base(context, estate)
   {
      EnvironmentInteractionContext Context = context;
   }

   public override void EnterState()
   {
      Debug.Log("Entering the Rest State");
      _elapsedTime = 0f;
      Context.ClosestPointOnColliderFromShoulder = Vector3.positiveInfinity;
      Context._currentIntersectingCollider = null;
   }

   public override void ExitState()
   {
      //Debug.Log("Exiting the Rest State");
   }

   public override void UpdateState()
   {
     //increment the time  in the rest state
     _elapsedTime += Time.deltaTime;
     Context.InteractionPointOffsetY = Mathf.Lerp(Context.InteractionPointOffsetY, 
     Context.ColliderCenterY, _elapsedTime / lerpDuration);
   }

   public override EnvironmentInteractionStateMachine.EEnvironmentInteractionState GetNextState()
   {
  // bool isMoving = Context.Rb.velocity != Vector3.zero;
   //if the time in the rest state is greater than 5 seconds, transition to the search state otherwise return statekey
   if (_elapsedTime >= _resetDuration ) //&& isMoving for when the character movement is implemented
   {
      return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Search;
   }
      return StateKey;
   }

   public override void OnTriggerEnter(Collider other)
   {
      //Debug.Log("Triggered the Rest State");
   }

   public override void OnTriggerStay(Collider other)
   {
     // Debug.Log("Stayed in the Rest State");
   }

   public override void OnTriggerExit(Collider other)
   {
     // Debug.Log("Exited the Rest State");
   }



}
