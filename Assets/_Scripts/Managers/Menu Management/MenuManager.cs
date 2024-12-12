using System.Collections.Generic;
using System.Linq;

public class MenuManager
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

    private readonly HashSet<GameMenu> _activeMenus = new();

    #endregion

    #region Getters

    public IReadOnlyCollection<GameMenu> ActiveMenus => _activeMenus;

    public bool IsCursorActiveInMenus => _activeMenus.Any(menu => menu.IsCursorRequired);

    public bool IsControlsDisabledInMenus => _activeMenus.Any(menu => menu.DisablePlayerControls);

    #endregion

    private MenuManager()
    {
        // Private constructor to prevent instantiation
    }

    public void AddMenu(GameMenu menu)
    {
        _activeMenus.Add(menu);
    }

    public void RemoveMenu(GameMenu menu)
    {
        _activeMenus.Remove(menu);
    }
}