using System.Collections.Generic;

public class TutorialManager
{
    #region Singleton Pattern

    private static TutorialManager _instance;

    public static TutorialManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new TutorialManager();

            return _instance;
        }
    }

    #endregion

    private readonly HashSet<Tutorial> _completedTutorials = new();
    
    public IReadOnlyCollection<Tutorial> CompletedTutorials => _completedTutorials;

    public void CompleteTutorial(Tutorial tutorial)
    {
        _completedTutorials.Add(tutorial);
    }

    public bool IsTutorialCompleted(Tutorial tutorial)
    {
        return _completedTutorials.Contains(tutorial);
    }
    
    public void ClearCompletedTutorials()
    {
        _completedTutorials.Clear();
    }
}