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

    #endregion

    #region Getters

    public TokenManager<float> TimeScaleTokenManager => _timeScaleTokenManager;

    #endregion

    private TimeScaleManager()
    {
        _timeScaleTokenManager = new TokenManager<float>(false, null, 1);
    }

    public void Update()
    {
        Time.timeScale = 1 * GetCalculatedValue();
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