using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class BossEnemy : MonoBehaviour
{
    [SerializeField,Range(0,100)] private int[] healthPercents;
    float bossHealthPercent;
    int bossPhase;

    UnityEvent PhaseOneEvent;
    UnityEvent PhaseTwoEvent;
    UnityEvent PhaseThreeEvent;

    public EnemyInfo enemyInfo;

    // Start is called before the first frame update
    void Start()
    {
        //Subscribe to the OnDamaged event
        enemyInfo.OnDamaged +=ActivatePhaseChange;

        //Initialize the boss
        InitializeBoss();

        //Initialize the UnityEvents
        if (PhaseOneEvent == null)
        {
            PhaseOneEvent = new UnityEvent();
        }
        if(PhaseTwoEvent == null)
        {
            PhaseTwoEvent = new UnityEvent();
        }
        if (PhaseThreeEvent == null)
        {
            PhaseThreeEvent = new UnityEvent();
        }

    }

    // Update is called once per frame
    void Update()
    {
    }
    void InitializeBoss()
    {
        CalculateHealthPercent();
    }
    void CalculateHealthPercent()
    {
        //Calculate the boss's health percentage
        bossHealthPercent = enemyInfo.CurrentHealth / enemyInfo.MaxHealth * 100;
    }
    void ActivatePhaseChange(object sender, HealthChangedEventArgs e)
    {
        //Debug.Log("Boss took damage");
        CalculateHealthPercent();
        if (bossHealthPercent <= healthPercents[0] && bossPhase==0)
        {
            bossPhase++;
            PhaseOneEvent.Invoke();
            Debug.Log("Phase 1 activated");
            PhaseOne();
        }
        if (bossHealthPercent <= healthPercents[1] && bossPhase == 1)
        {
            bossPhase++;
            PhaseTwoEvent.Invoke();
            Debug.Log("Phase 2 activated");
            PhaseTwo();
        }
        if (bossHealthPercent <= healthPercents[2] && bossPhase == 2)
        {
            bossPhase++;
            PhaseThreeEvent.Invoke();
            Debug.Log("Phase 3 activated");
            PhaseThree();
        }
    }
    void PhaseOne()
    {
        //Do code for phase one
    }
    void PhaseTwo()
    {
        //Do code for phase two
    }
    void PhaseThree()
    {
        //Do code for phase three
    }
}
