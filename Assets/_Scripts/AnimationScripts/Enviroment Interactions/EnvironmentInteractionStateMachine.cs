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
   public EnvironmentInteractionContext _context;
   
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

   private void OnDrawGizmosSelected()
   {
      Gizmos.color = Color.red;
      if(_context != null && _context.ClosestPointOnColliderFromShoulder != null)
      {
         Gizmos.DrawSphere(_context.ClosestPointOnColliderFromShoulder, 0.03f);
      }
   }

   void Awake()
   {
      //initialize the context
      _context = new EnvironmentInteractionContext(_leftIKConstraint, _rightIKConstraint, 
         _headIKConstraint, _leftMultiRotationConstraint, _rightMultiRotationConstraint, _rigidbody, _rootCollider ,transform.root);
         InitializeState();
         ConstructorEnvironmentDetectionCollider();
         
   }
   
   public void InitializeState()
   {
      //add states to the state machine dictionary

      States.Add(EEnvironmentInteractionState.Reset, new ResetState(_context, EEnvironmentInteractionState.Reset));
      States.Add(EEnvironmentInteractionState.Search, new SearchState(_context, EEnvironmentInteractionState.Search));
      // States.Add(EEnvironmentInteractionState.Approach, new ApproachState(_context, EEnvironmentInteractionState.Approach));
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
   }


}


