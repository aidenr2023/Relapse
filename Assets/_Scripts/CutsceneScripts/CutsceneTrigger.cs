using UnityEngine;

/// <summary>
/// When the player enters the trigger, this component tells the CutsceneManager to play a cutscene by name.
/// </summary>
public class CutsceneTrigger : MonoBehaviour
{
    // Reference to the central CutsceneManager.
    public CutsceneManager cutsceneManager;
    
    // Name of the cutscene to trigger (must match a mapping in the CutsceneManager).
    public string cutsceneName;

    //bool to check if the cutscene has been played
    private bool cutscenePlayed = false;

    //if the player enters the trigger, play the cutscene
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !cutscenePlayed)
        {
            cutscenePlayed = true;
            // Use the manager to look up and play the cutscene.
            cutsceneManager.PlayCutsceneByName(cutsceneName);
        }
        //if the player has already played the cutscene, destroy the trigger
        else if (other.CompareTag("Player") && cutscenePlayed)
        {
            Destroy(gameObject);
        }
    }
    // //on trigger exit destroy the cutscene and gameobject trigger
    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.CompareTag("Player"))
    //     {
    //         Destroy(gameObject);
    //     }
    // }
}