using UnityEngine;

public class TutorialCollider : MonoBehaviour
{
    [SerializeField] private TutorialScreen.Tutorial tutorial;

    [SerializeField] private bool activateOnce = true;

    private bool _hasAlreadyActivated;

    public void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Return if the tutorial has already been activated
        if (_hasAlreadyActivated && activateOnce)
            return;

        // Add the tutorial to the tutorial screen
        TutorialScreen.Instance.ChangeTutorial(tutorial);
        TutorialScreen.Instance.Activate();

        // Set the has already activated flag to true
        if (activateOnce)
            _hasAlreadyActivated = true;
    }
}