using UnityEngine;

public class InteractionHelper : MonoBehaviour
{
    public void PlayTutorial(Tutorial tutorial)
    {
        // TutorialScreen.Instance.PlayTutorial(tutorial, false);
        TutorialScreen.Play(this, tutorial, false);
    }
    
    public void PlayTutorialReplay(Tutorial tutorial)
    {
        // TutorialScreen.Instance.PlayTutorial(tutorial, true);
        TutorialScreen.Play(this, tutorial, true);
    }

}