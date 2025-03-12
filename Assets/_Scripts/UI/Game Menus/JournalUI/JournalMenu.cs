using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JournalMenu : GameMenu
{
    protected override void CustomAwake()
    {
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
    }

    public override void OnBackPressed()
    {
    }
    
    public static IEnumerator LoadVendorMenu()
    {
        // // Load the vendor UI scene
        // SceneManager.LoadScene(VENDOR_SCENE_NAME, LoadSceneMode.Additive);
        //
        // // Wait while the instance is null
        // yield return new WaitUntil(() => Instance != null);

        yield return null;
    }
}