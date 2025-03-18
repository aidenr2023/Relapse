using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : GameMenu
{
    [Header("Settings Menu"), SerializeField]
    private GameObject firstSelectedButton;

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
        StartCoroutine(EnsureSelectObject(firstSelectedButton));
    }

    private IEnumerator EnsureSelectObject(GameObject gameObject)
    {
        // If this is not the active menu, return
        if (!IsActive)
            yield break;

        while (!eventSystem.enabled || eventSystem.currentSelectedGameObject != gameObject)
        {
            eventSystem.SetSelectedGameObject(gameObject);
            yield return null;
        }
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
}