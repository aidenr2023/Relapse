using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetState : EnvironmentInteractionState
{
   public ResetState(EnvironmentInteractionContext context,
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
      return EnvironmentInteractionStateMachine.EEnvironmentInteractionState.Search;
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
