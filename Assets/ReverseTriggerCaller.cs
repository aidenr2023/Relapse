using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReverseTriggerCaller : MonoBehaviour
{
    [SerializeField] private ReverseTrigger reverseTrigger;
    

    public void CallTurnoffFunction()
    {
        reverseTrigger.Turnoff();
    }
    
    public void CallTurnonFunction()
    {
        reverseTrigger.Turnon();
    }
    
}
