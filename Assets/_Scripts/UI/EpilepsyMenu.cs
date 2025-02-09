using UnityEngine;
using UnityEngine.SceneManagement;

public class EpilepsyMenu : GameMenu
{
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    
    protected override void CustomAwake()
    {
        // Additively load the main menu scene
        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Additive);
    }

    protected override void CustomStart()
    {
    }

    protected override void CustomDestroy()
    {
    }

    protected override void CustomActivate()
    {
    }

    protected override void CustomDeactivate()
    {
    }

    protected override void CustomUpdate()
    {
        // If this is NOT the active menu and is currently active,
        // Remove it from the active menus stack and add it back to put it on top
        if (MenuManager.Instance.ActiveMenu != this && IsActive)
        {
            Deactivate();
            Activate();
        }
        
        // If the opacity is 0, unload the scene
        if (canvasGroup.alpha == 0)
            UnloadScene();
    }

    public override void OnBackPressed()
    {
    }

    public void ContinueButtonPressed()
    {
        // Deactivate the epilepsy menu
        Deactivate();
    }

    private void UnloadScene()
    {
        // Get the scene that this object is in
        var scene = gameObject.scene;
        
        // Unload the scene
        SceneManager.UnloadSceneAsync(scene);
    }
}