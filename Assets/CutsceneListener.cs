using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneListener : MonoBehaviour
{
    // hash to store the animation
    private static readonly int CinematicBarsHash = Animator.StringToHash("Cinematic");

    private static readonly int ShowBarsAnimationID = Animator.StringToHash("ShowBars");
    private static readonly int HideBarsAnimationID = Animator.StringToHash("HideBars");
    private static readonly int ShowTitleAnimationID = Animator.StringToHash("ShowTitle");
    private static readonly int HideTitleAnimationID = Animator.StringToHash("HideTitle");

    // create an instance of the listener
    public static CutsceneListener Instance { get; private set; }

    [SerializeField] private TMP_Text levelNameText;

    // get instance of cutscene manager
    private CutsceneManager _cutsceneManager;
    private CutsceneHandler _cutsceneHandler;
    private Animator _cinematicAnimator;

    public TMP_Text LevelNameText => levelNameText;

    private void Awake()
    {
        // Return if the instance already exists
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        // reset the instance if this is the instance
        if (Instance == this)
        {
            Debug.Log("CutsceneListener instance destroyed!");
            Instance = null;
        }
    }

    private void Start()
    {
        // get the animator component
        _cinematicAnimator = GetComponent<Animator>();

        // SceneManager.sceneLoaded += OnSceneLoaded;
        // cutsceneManager = CutsceneManager.Instance;
        // cutsceneHandler = cutsceneManager.CutsceneHandler;

        OnCutsceneStart();
        OnCutsceneEnd();
    }

    // start the animation when the cutscene starts
    private void OnCutsceneStart()
    {
        // cutsceneHandler.OnCutsceneStart.AddListener(PlayAnimation);
    }


    // stop the animation when the cutscene ends
    private void OnCutsceneEnd()
    {
        // cutsceneHandler.OnCutsceneEnd.AddListener(StopAnimation);
    }

    // on Scene load play animation
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // _cinematicAnimator.SetTrigger("SceneLoadLayer");
        _cinematicAnimator.SetTrigger(ShowBarsAnimationID);
    }

    private IEnumerator HideBarsAfterDelay(float delay, bool showTitle)
    {
        yield return new WaitForSeconds(delay);
        StopBarsAnimation(showTitle);
    }

    // play the animation
    public void PlayBarsAnimation(bool showTitle)
    {
        _cinematicAnimator.SetTrigger(ShowBarsAnimationID);
        
        if (showTitle)
            _cinematicAnimator.SetTrigger(ShowTitleAnimationID);

        StartCoroutine(HideBarsAfterDelay(3f, showTitle));
    }

    // stop the animation
    private void StopBarsAnimation(bool showTitle)
    {
        _cinematicAnimator.SetTrigger(HideBarsAnimationID);
        
        if (showTitle)
            _cinematicAnimator.SetTrigger(HideTitleAnimationID);
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