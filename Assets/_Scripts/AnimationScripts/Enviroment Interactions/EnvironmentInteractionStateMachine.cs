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
   
   [SerializeField] private TwoBoneIKConstraint _leftIKConstraint;
   [SerializeField] private TwoBoneIKConstraint _rightIKConstraint;
   [SerializeField] private TwoBoneIKConstraint _headIKConstraint;
   [SerializeField] private MultiRotationConstraint _leftMultiRotationConstraint;
   [SerializeField] private MultiRotationConstraint _rightMultiRotationConstraint;

}
