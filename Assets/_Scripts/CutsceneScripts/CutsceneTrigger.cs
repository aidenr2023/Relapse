using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// When the player enters the trigger, this component tells the CutsceneManager to play a cutscene by name.
/// </summary>
public class CutsceneTrigger : MonoBehaviour
{
    // Name of the cutscene to trigger (must match a mapping in the CutsceneManager).
    public string cutsceneName;

    private CutsceneHandler cutsceneHandler;
    
    [SerializeField] private bool isCamChangeNeeded = false;
    
    [SerializeField] private bool isPlayerMovementNeeded = false;
    
    [SerializeField] private bool isCutsceneFirstPerson = false;
    
    
    
    private CutsceneHandler.CutsceneType _cutscenePerspective;
    private BoxCollider _boxCollider;
    
    
    public bool IsCamChangeNeeded => isCamChangeNeeded;

    private bool cutscenePlayed = false;
    
    private void Start()
    {
        cutsceneHandler = CutsceneManager.Instance.CutsceneHandler;
        cutsceneHandler.IsPlayerMovementNeeded = isPlayerMovementNeeded;
        cutsceneHandler.IsCutsceneFirstPerson = isCutsceneFirstPerson;
        if (isCutsceneFirstPerson)
        {
            _cutscenePerspective = CutsceneHandler.CutsceneType.FirstPerson;
        }
        else
        {
            _cutscenePerspective = CutsceneHandler.CutsceneType.ThirdPerson;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !cutscenePlayed)
        {
            // Access the singleton instance 
            StartCoroutine(TriggerCutsceneDelayed());
        }
        else if (other.CompareTag("Player") && cutscenePlayed)
        {
            StartCoroutine(DestroyTrigger());
        }
    }
    
    private IEnumerator TriggerCutsceneDelayed()
    {
        // Wait one frame after the player enters the trigger
        yield return null;
        //plays the cutscene by name, checks if player movement is needed,
        //and if the cutscene is first person
        CutsceneManager.Instance.PlayCutsceneByName(cutsceneName, isPlayerMovementNeeded, _cutscenePerspective);
        cutscenePlayed = true;
    }
   
    
    
    //destroy the trigger after seconds 
    private IEnumerator DestroyTrigger()
    {
        yield return new WaitForSeconds(3f);
        _boxCollider.enabled = false;
    }
}