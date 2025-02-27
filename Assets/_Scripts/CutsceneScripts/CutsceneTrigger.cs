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
    
    [SerializeField] private bool isCamChangeNeeded = true;
    [SerializeField] private bool isPlayerMovementNeeded = true;
    public bool IsCamChangeNeeded => isCamChangeNeeded;

    private bool cutscenePlayed = false;
    
    private void Start()
    {
        cutsceneHandler = CutsceneManager.Instance.CutsceneHandler;
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
        yield return null;
        CutsceneManager.Instance.PlayCutsceneByName(cutsceneName, cutsceneHandler.IsPlayerMovementNeeded);
        cutscenePlayed = true;
    }
    
    //destroy the trigger after seconds 
    private IEnumerator DestroyTrigger()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}