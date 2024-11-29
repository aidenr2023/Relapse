using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EnvironmentInteractionStateMachine : StateManager<EnvironmentInteractionStateMachine.EEnvironmentInteractionState>
{
   public enum EEnvironmentInteractionState
   {
      Search,
      Approach,
      Rise,
      Touch,
      Reset,
   }
   
   //context for the state machine
   protected EnvironmentInteractionContext Context;
   
   //referemces to the rigging constraints
   [SerializeField] private TwoBoneIKConstraint _leftIKConstraint;
   [SerializeField] private TwoBoneIKConstraint _rightIKConstraint;
   [SerializeField] private MultiAimConstraint _headIKConstraint;
   [SerializeField] private MultiRotationConstraint _leftMultiRotationConstraint;
   [SerializeField] private MultiRotationConstraint _rightMultiRotationConstraint;
   [SerializeField] private Rigidbody _rigidbody;
   [SerializeField] private CapsuleCollider _rootCollider;
   //may need to include the root transform
   [SerializeField] private Transform _rootTransform;

   private void OnDrawGizmos()
   {
      Debug.Log("Drawing Gizmos");
      Gizmos.color = Color.red;
      if(Context != null && Context.ClosestPointOnColliderFromShoulder != null)
      {
         Gizmos.DrawSphere(Context.ClosestPointOnColliderFromShoulder, 10f);
      }
   }

   void Awake()
   {
      //initialize the context
      Context = new EnvironmentInteractionContext(_leftIKConstraint, _rightIKConstraint, 
         _headIKConstraint, _leftMultiRotationConstraint, _rightMultiRotationConstraint, _rigidbody, _rootCollider ,transform.root);
         InitializeState();
         ConstructorEnvironmentDetectionCollider();
         
   }
   
   public void InitializeState()
   {
      //add states to the state machine dictionary

      //States.Add(EEnvironmentInteractionState.Reset, new ResetState(Context, EEnvironmentInteractionState.Reset));
      //States.Add(EEnvironmentInteractionState.Search, new SearchState(Context, EEnvironmentInteractionState.Search));
      //States.Add(EEnvironmentInteractionState.Approach, new ApproachState(Context, EEnvironmentInteractionState.Approach));
      // States.Add(EEnvironmentInteractionState.Rise, new RiseState(_context, EEnvironmentInteractionState.Rise));
      // States.Add(EEnvironmentInteractionState.Touch, new TouchState(_context, EEnvironmentInteractionState.Touch));
      CurrentState = States[EEnvironmentInteractionState.Reset];

   }
   
   //create collider for environment detection based on wingspan of the character
   private void ConstructorEnvironmentDetectionCollider()
   {
      float wingspan = _rootCollider.height;
      //environment detection collider
      BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
      boxCollider.size = new Vector3(wingspan, wingspan, wingspan);
      boxCollider.center = new Vector3(_rootCollider.center.x, _rootCollider.center.y + (0.25f * wingspan), _rootCollider.center.z + (.75f * wingspan)); 
      boxCollider.isTrigger = true;
      
      Context.ColliderCenterY = _rootCollider.center.y;
   }


}


