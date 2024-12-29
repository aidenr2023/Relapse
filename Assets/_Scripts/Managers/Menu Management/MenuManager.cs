using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

public class MenuManager : IUsesInput
{
    #region Singleton Pattern

    private static MenuManager _instance;

    public static MenuManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new MenuManager();

            return _instance;
        }
    }

    #endregion

    #region Private Fields

    private readonly CustomStack<GameMenu> _activeMenus = new();

    private readonly HashSet<GameMenu> _managedMenus = new();

    #endregion

    #region Getters

    public HashSet<InputData> InputActions { get; } = new();

    public IReadOnlyCollection<GameMenu> ActiveMenus => _activeMenus.ToArray();

    public bool IsCursorActiveInMenus => _activeMenus.Any(menu => menu.IsCursorRequired);

    public bool IsControlsDisabledInMenus => _activeMenus.Any(menu => menu.DisablePlayerControls);

    public bool IsGamePausedInMenus => _activeMenus.Any(menu => menu.PausesGame);

    #endregion

    private MenuManager()
    {
        // Private constructor to prevent instantiation
        InitializeInput();

        // Register the MenuManager to use input
        InputManager.Instance.Register(this);
    }


    public void InitializeInput()
    {
        InputActions.Add(new InputData(
            InputManager.Instance.DefaultInputActions.UI.Cancel, InputType.Performed, ActiveMenuBack)
        );
    }

    private void ActiveMenuBack(InputAction.CallbackContext obj)
    {
        // Get the active menu
        var activeMenu = _activeMenus.Peek();

        // If the active menu is not null
        if (activeMenu != null)
            activeMenu.OnBackPressed();
    }

    public void AddActiveMenu(GameMenu menu)
    {
        _activeMenus.Push(menu);
    }

    public void RemoveActiveMenu(GameMenu menu)
    {
        _activeMenus.Remove(menu);
    }

    public void ManageMenu(GameMenu menu)
    {
        _managedMenus.Add(menu);
    }

    public void UnManageMenu(GameMenu menu)
    {
        _managedMenus.Remove(menu);
    }
}