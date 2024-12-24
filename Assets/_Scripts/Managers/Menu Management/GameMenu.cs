using System;
using UnityEngine;

public abstract class GameMenu : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private bool isCursorRequired = true;

    [SerializeField] private bool disablePlayerControls = true;

    [SerializeField] private bool pausesGame = true;

    #endregion

    #region Getters

    public bool IsCursorRequired => isCursorRequired;

    public bool DisablePlayerControls => disablePlayerControls;

    public bool PausesGame => pausesGame;

    #endregion

    protected void Awake()
    {
        // Manage this menu
        MenuManager.Instance.ManageMenu(this);

        // Custom Awake
        CustomAwake();
    }

    protected abstract void CustomAwake();

    protected void OnEnable()
    {
        // Add this to the active menus
        MenuManager.Instance.AddMenu(this);

        CustomOnEnable();
    }

    protected void OnDisable()
    {
        // Remove this from the active menus
        MenuManager.Instance.RemoveMenu(this);

        CustomOnDisable();
    }

    private void OnDestroy()
    {
        // Un manage this menu
        MenuManager.Instance.UnManageMenu(this);
    }

    protected abstract void CustomOnEnable();
    protected abstract void CustomOnDisable();

    public virtual void MenuManagerUpdate()
    {
    }
}