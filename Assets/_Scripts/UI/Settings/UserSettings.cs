using UnityEngine;

public class UserSettings
{
    public const float MIN_SENSITIVITY = 0.01f;
    public const float MAX_SENSITIVITY = 1;

    private static UserSettings _instance;

    public static UserSettings Instance
    {
        get
        {
            if (_instance == null)
                _instance = new UserSettings();

            return _instance;
        }
    }

    public Vector2 MouseSens { get; private set; } = new(.5f, .5f);
    public Vector2 ControllerSens { get; private set; } = new(.5f, .5f);

    #region Public Methods

    public void SetMouseSensitivity(float x, float y)
    {
        MouseSens = new Vector2(
            Mathf.Clamp(x, MIN_SENSITIVITY, MAX_SENSITIVITY),
            Mathf.Clamp(y, MIN_SENSITIVITY, MAX_SENSITIVITY)
        );
    }

    public void SetControllerSensitivity(float x, float y)
    {
        ControllerSens = new Vector2(
            Mathf.Clamp(x, MIN_SENSITIVITY, MAX_SENSITIVITY),
            Mathf.Clamp(y, MIN_SENSITIVITY, MAX_SENSITIVITY)
        );
    }

    #endregion
}