using UnityEngine;

public class UserSettings
{
    #region Constants

    public const float MIN_SENSITIVITY = 0.01f;
    public const float MAX_SENSITIVITY = 1;

    public const float MIN_GAMMA = -1;
    public const float MAX_GAMMA = 1;
    public const float MIN_VOLUME = -80;
    public const float MAX_VOLUME = 20;

    #endregion

    #region Singleton Pattern

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

    #endregion

    #region Getters for Settings

    public Vector2 MouseSens { get; private set; } = new(.5f, .5f);
    public Vector2 ControllerSens { get; private set; } = new(.5f, .5f);

    public float Gamma { get; private set; }

    #endregion

    #region Public Methods

    public void SetMouseSensitivity(float x, float y)
    {
        x = Mathf.Clamp(x, MIN_SENSITIVITY, MAX_SENSITIVITY);
        y = Mathf.Clamp(y, MIN_SENSITIVITY, MAX_SENSITIVITY);

        MouseSens = new Vector2(
            Mathf.Clamp(x, MIN_SENSITIVITY, MAX_SENSITIVITY),
            Mathf.Clamp(y, MIN_SENSITIVITY, MAX_SENSITIVITY)
        );
    }

    public void SetControllerSensitivity(float x, float y)
    {
        x = Mathf.Clamp(x, MIN_SENSITIVITY, MAX_SENSITIVITY);
        y = Mathf.Clamp(y, MIN_SENSITIVITY, MAX_SENSITIVITY);

        ControllerSens = new Vector2(
            Mathf.Clamp(x, MIN_SENSITIVITY, MAX_SENSITIVITY),
            Mathf.Clamp(y, MIN_SENSITIVITY, MAX_SENSITIVITY)
        );
    }

    public void SetGamma(float value)
    {
        value = Mathf.Clamp(value, MIN_GAMMA, MAX_GAMMA);

        Gamma = value;
    }
    public void SetSoundVolume(float value)
    {
        value = Mathf.Clamp(value, 0, 1);

        // Set sound volume
    }

    #endregion
}