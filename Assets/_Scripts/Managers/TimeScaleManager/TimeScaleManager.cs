using System.Linq;
using System.Text;
using UnityEngine;

public class TimeScaleManager : IDebugged
{
    #region Singleton Pattern

    private static TimeScaleManager _instance;

    public static TimeScaleManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new TimeScaleManager();

            return _instance;
        }
    }

    #endregion

    #region Private Fields

    private readonly TokenManager<float> _timeScaleTokenManager;

    private readonly TokenManager<float>.ManagedToken _menuPauseToken;

    #endregion

    #region Getters

    public TokenManager<float> TimeScaleTokenManager => _timeScaleTokenManager;

    #endregion

    private TimeScaleManager()
    {
        _timeScaleTokenManager = new TokenManager<float>(false, null, 1);

        _menuPauseToken = _timeScaleTokenManager.AddToken(1, -1, true);
    }

    public void Update()
    {
        // Update the Menu Pause Token
        UpdateMenuPauseToken();

        Time.timeScale = 1 * GetCalculatedValue();
    }

    private void UpdateMenuPauseToken()
    {
        // If there are any menus active, pause the game
        // If there are no menus active, resume the game
        _menuPauseToken.Value = MenuManager.Instance.IsGamePausedInMenus ? 0 : 1;
    }

    private float GetCalculatedValue()
    {
        var value = 1f;

        foreach (var token in _timeScaleTokenManager.Tokens)
            value *= token.Value;

        return value;
    }


    public string GetDebugText()
    {
        StringBuilder sb = new();

        sb.Append($"Time Scale Manager: {Time.timeScale:0.0000}\n");

        var tokenString = string.Join(
            " * ",
            _timeScaleTokenManager.Tokens.Select(n => $"{n.Value:0.00}")
        );

        sb.Append($"\tTokens: [{tokenString}]\n");

        return sb.ToString();
    }
}