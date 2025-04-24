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

    //[SerializeField] private bool isCamChangeNeeded = false;

    [SerializeField] private CutsceneObjectList objectList;


    [SerializeField] private bool isPlayerMovementNeeded = false;

    [SerializeField] private bool isCutsceneFirstPerson = false;

    [SerializeField] private bool isCutsceneInteractable = false;


    private CutsceneHandler.CutsceneType _cutscenePerspective;
    private BoxCollider _boxCollider;

    private object _uiDisabler;

    //public bool IsCamChangeNeeded => isCamChangeNeeded;

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

    private void OnDestroy()
    {
        // Re-show the UI elements
        GameUIHelper.Instance?.RemoveUIHider(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCutsceneInteractable)
            return;

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
        yield return null; // give everything one frame to initialize

        objectList.EnableObjects(); // bring in just this list’s objects

        var handler = CutsceneManager.Instance.CutsceneHandler;
        UnityEngine.Events.UnityAction onEnd = null;
        onEnd = () =>
        {
            objectList.DisableObjects(); // tear down just this list
            handler.OnCutsceneEnd.RemoveListener(onEnd);

            // Re-show the UI elements
            GameUIHelper.Instance?.RemoveUIHider(this);
        };
        handler.OnCutsceneEnd.AddListener(onEnd);

        CutsceneManager.Instance.PlayCutsceneByName(
            cutsceneName,
            isPlayerMovementNeeded,
            _cutscenePerspective
        );
        cutscenePlayed = true;

        // If the player instance is not null, add a UI disabler to the player instance
        // Hide the UI elements
        GameUIHelper.Instance?.AddUIHider(this);
    }


    //destroy the trigger after seconds 
    private IEnumerator DestroyTrigger()
    {
        yield return new WaitForSeconds(3f);

        if (_boxCollider != null)
            _boxCollider.enabled = false;
    }

    /// <summary>
    /// Call this to play the named cutscene immediately.
    /// </summary>
    public void PlayCutscene()
    {
        // Play the cutscene
        StartCoroutine(PlayCutsceneNextFrame());
    }

    private IEnumerator PlayCutsceneNextFrame()
    {
        // Wait one frame so all Start() calls and bindings finish
        yield return null;
        //invoke the cutscene event

        CutsceneManager.Instance.PlayCutsceneByName(
            cutsceneName,
            isPlayerMovementNeeded,
            _cutscenePerspective
        );
    }
}