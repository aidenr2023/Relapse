using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class StateManager<EState> : MonoBehaviour where EState : System.Enum
{
    // The states that the state manager can be in
    protected Dictionary<EState, BaseState<EState>> States = new Dictionary<EState, BaseState<EState>>();
    
    // The current state of the state manager
    protected BaseState<EState> CurrentState;
    protected bool isTransitionToState = false;
    
    //private field holding queued state
    private EState _queuedState;
    
    private void Update()
    {
        EState nextStateKey = CurrentState.GetNextState();

        if (!isTransitionToState && nextStateKey.Equals(CurrentState.StateKey))
        {
            CurrentState.UpdateState();
        }
        else if(!isTransitionToState)
        {
            TransitionToState(nextStateKey);
        }
        
       
        
        
    }
    public void TransitionToState(EState stateKey)
    {
        isTransitionToState = true;
        CurrentState.ExitState();
        CurrentState = States[stateKey];
        CurrentState.EnterState();
        isTransitionToState = false;
    }
    void OnTriggerEnter(Collider other)
    {
        CurrentState.OnTriggerEnter(other);
    }
    void OnTriggerStay(Collider other)
    {
        CurrentState.OnTriggerStay(other);
    }
    void OnTriggerExit(Collider other)
    {
        CurrentState.OnTriggerExit(other);
    }
}
