using System;
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

    private readonly CustomStack<IGameMenu> _activeMenus = new();

    #endregion

    #region Getters

    public HashSet<InputData> InputActions { get; } = new();

    public IReadOnlyCollection<IGameMenu> ActiveMenus => _activeMenus.ToArray();

    public bool IsCursorActiveInMenus => _activeMenus.Any(menu => menu.IsCursorRequired);

    public bool IsControlsDisabledInMenus => _activeMenus.Any(menu => menu.DisablePlayerControls);

    public bool IsGamePausedInMenus => _activeMenus.Any(menu => menu.PausesGame);
    
    public bool IsGameMusicPausedInMenus => _activeMenus.Any(menu => menu.PausesGameMusic);

    public IGameMenu ActiveMenu => _activeMenus.Peek();

    #endregion
    
    public event Action OnActiveMenuChanged; 

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
            InputManager.Instance.UIControls.UI.Cancel, InputType.Performed, ActiveMenuBack)
        );
    }

    private void ActiveMenuBack(InputAction.CallbackContext obj)
    {
        // Get the active menu
        var activeMenu = _activeMenus.Peek();

        // If the active menu is not null
        if (activeMenu is not null && (activeMenu as UnityEngine.Object) != null)
            activeMenu.OnBackPressed();
    }

    public void AddActiveMenu(IGameMenu menu)
    {
        // Return if the menu is already active
        if (_activeMenus.Contains(menu))
            return;

        _activeMenus.Push(menu);
        
        // Invoke the OnActiveMenuChanged event
        OnActiveMenuChanged?.Invoke();
    }

    public void RemoveActiveMenu(IGameMenu menu)
    {
        _activeMenus.Remove(menu);
        
        // Invoke the OnActiveMenuChanged event
        OnActiveMenuChanged?.Invoke();
    }
}