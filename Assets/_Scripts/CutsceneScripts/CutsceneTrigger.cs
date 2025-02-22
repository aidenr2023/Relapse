using UnityEngine;

/// <summary>
/// When the player enters the trigger, this component tells the CutsceneManager to play a cutscene by name.
/// </summary>
public class CutsceneTrigger : MonoBehaviour
{
    // Name of the cutscene to trigger (must match a mapping in the CutsceneManager).
    public string cutsceneName;

    [SerializeField] private bool isCamChangeNeeded = true;
    public bool IsCamChangeNeeded => isCamChangeNeeded;

    private bool cutscenePlayed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !cutscenePlayed)
        {
            cutscenePlayed = true;
            // Access the singleton instance directly
            CutsceneManager.Instance.PlayCutsceneByName(cutsceneName);
        }
        else if (other.CompareTag("Player") && cutscenePlayed)
        {
            Destroy(gameObject);
        }
    }
}