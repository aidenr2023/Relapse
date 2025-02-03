using UnityEngine;

public class InteractionHelper : MonoBehaviour
{
    public void PlayTutorial(Tutorial tutorial)
    {
        TutorialScreen.Instance.PlayTutorial(tutorial, false);
    }
    
    public void PlayTutorialReplay(Tutorial tutorial)
    {
        TutorialScreen.Instance.PlayTutorial(tutorial, true);
    }

}