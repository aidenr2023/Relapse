using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneListener : MonoBehaviour
{
    //get instance of cutscene manager
    private CutsceneManager cutsceneManager;
    private CutsceneHandler cutsceneHandler;
    private Animator _cinematicAnimator;
    
    //hash to store the animation
    private readonly int _cinematicBarsHash = Animator.StringToHash("Cinematic");
    
    private void Start()
    {
        //get the animator component
        _cinematicAnimator = GetComponent<Animator>();
        cutsceneManager = CutsceneManager.Instance;
        cutsceneHandler = cutsceneManager.CutsceneHandler;
        OnCutsceneStart();
        OnCutsceneEnd();
    }
    
    //start the animation when the cutscene starts
    private void OnCutsceneStart()
    {
        cutsceneHandler.OnCutsceneStart.AddListener(PlayAnimation);
    }
    
    
    //stop the animation when the cutscene ends
    private void OnCutsceneEnd()
    {
        cutsceneHandler.OnCutsceneEnd.AddListener(StopAnimation);
    }
    
    //play the animation
    private void PlayAnimation()
    {
        _cinematicAnimator.SetBool(_cinematicBarsHash, true);
    }
    //stop the animation
    private void StopAnimation()
    {
        _cinematicAnimator.SetBool(_cinematicBarsHash, false);
    }
    
    public void PauseAnimation()
    {
        _cinematicAnimator.speed = 0;
    }
    
    private void ReverseAnimation()
    {
        _cinematicAnimator.speed = -1;
    }
    
    
    
}
