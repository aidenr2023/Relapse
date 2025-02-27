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

    // Input settings

    public Vector2 MouseSens { get; private set; } = new(.5f, .5f);
    public Vector2 ControllerSens { get; private set; } = new(.5f, .5f);

    public float MinimumLookDeadzone { get; set; }

    public float MaximumLookDeadzone { get; set; } = 1;

    public float MinimumMoveDeadzone { get; set; }

    public float MaximumMoveDeadzone { get; set; } = 1;

    public float Gamma { get; private set; }

    // Volume settings
    public float MasterVolume { get; private set; }
    public float MusicVolume { get; private set; }
    public float GameSFXVolume { get; private set; }
    public float PlayerVolume { get; private set; }
    public float EnemiesVolume { get; private set; }
    public float OtherVolume { get; private set; }
    public float UISFXVolume { get; private set; }

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

    //Volume settings individual
    public void SetSoundMasterVolume(float value)
    {
        value = Mathf.Clamp(value, MIN_VOLUME, MAX_VOLUME);

        MasterVolume = value;
    }

    public void SetSoundMusicVolume(float value)
    {
        value = Mathf.Clamp(value, MIN_VOLUME, MAX_VOLUME);

        MusicVolume = value;
    }

    public void SetSoundGameSFXVolume(float value)
    {
        value = Mathf.Clamp(value, MIN_VOLUME, MAX_VOLUME);

        GameSFXVolume = value;
    }

    public void SetSoundPlayerVolume(float value)
    {
        value = Mathf.Clamp(value, MIN_VOLUME, MAX_VOLUME);

        PlayerVolume = value;
    }

    public void SetSoundEnemiesVolume(float value)
    {
        value = Mathf.Clamp(value, MIN_VOLUME, MAX_VOLUME);

        EnemiesVolume = value;
    }

    public void SetSoundOtherVolume(float value)
    {
        value = Mathf.Clamp(value, MIN_VOLUME, MAX_VOLUME);

        OtherVolume = value;
    }

    public void SetSoundUISFXVolume(float value)
    {
        value = Mathf.Clamp(value, MIN_VOLUME, MAX_VOLUME);

        UISFXVolume = value;
    }


    //This is probably better to use than the above methods
    public void SetAllVolumes(float Master, float Music, float GameSFX, float Player, float Enemies, float Other,
        float UISFX)
    {
        MasterVolume = Mathf.Clamp(Master, MIN_VOLUME, MAX_VOLUME);
        MusicVolume = Mathf.Clamp(Music, MIN_VOLUME, MAX_VOLUME);
        GameSFXVolume = Mathf.Clamp(GameSFX, MIN_VOLUME, MAX_VOLUME);
        PlayerVolume = Mathf.Clamp(Player, MIN_VOLUME, MAX_VOLUME);
        EnemiesVolume = Mathf.Clamp(Enemies, MIN_VOLUME, MAX_VOLUME);
        OtherVolume = Mathf.Clamp(Other, MIN_VOLUME, MAX_VOLUME);
        UISFXVolume = Mathf.Clamp(UISFX, MIN_VOLUME, MAX_VOLUME);
    }

    #endregion
}