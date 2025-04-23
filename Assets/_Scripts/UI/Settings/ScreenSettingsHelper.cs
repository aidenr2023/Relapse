using UnityEngine;

public class ScreenSettingsHelper : MonoBehaviour
{
    private ScreenSettings _previousScreenSettings;
    private ScreenSettings _workingScreenSettings;

    public void SetFullscreenMode(FullScreenMode mode)
    {
        // If the fullscreen mode is already set to the desired value, do nothing
        if (Screen.fullScreenMode == mode)
            return;

        // Set the fullscreen mode of the working screen settings
        _workingScreenSettings.FullScreenMode = mode;

        Debug.Log($"Set fullscreen mode to {mode}");
    }

    private void SetResolution(int width, int height)
    {
        // If the resolution is already set to the desired values, do nothing
        if (_workingScreenSettings.Resolution.Width == width &&
            _workingScreenSettings.Resolution.Height == height)
            return;

        // Set the resolution of the working screen settings
        _workingScreenSettings.Resolution = new ScreenSettings.ResolutionSizeStruct
        {
            Width = width,
            Height = height
        };
    }

    public void SetResolution(Resolution resolution) => SetResolution(resolution.width, resolution.height);

    public void SetQualityLevel(int qualityLevel)
    {
        // Set the quality level of the working screen settings
        _workingScreenSettings.QualityLevel = qualityLevel;
    }

    public void SetVSync(bool isOn)
    {
        // Set the VSync state of the working screen settings
        _workingScreenSettings.IsVSync = isOn;
    }

    public void SetAntiAliasing(int level)
    {
        // Set the anti-aliasing level of the working screen settings
        _workingScreenSettings.AntiAliasing = level;
        Debug.Log($"Set anti-aliasing level to {level}");
    }

    public void SetRefreshRate(RefreshRate rate)
    {
        _workingScreenSettings.RefreshRate = new ScreenSettings.RefreshRateStruct
        {
            Numerator = rate.numerator,
            Denominator = rate.denominator
        };
    }

    private ScreenSettings GetCurrentScreenSettings()
    {
        var currentRefreshRate = Screen.currentResolution.refreshRateRatio;

        // Create a new ScreenSettings object and populate it with the current settings
        ScreenSettings settings = new()
        {
            FullScreenMode = Screen.fullScreenMode,
            Resolution = new ScreenSettings.ResolutionSizeStruct
            {
                Width = Screen.width,
                Height = Screen.height
            },
            QualityLevel = QualitySettings.GetQualityLevel(),
            IsVSync = QualitySettings.vSyncCount > 0,
            AntiAliasing = QualitySettings.antiAliasing,
            RefreshRate = new ScreenSettings.RefreshRateStruct
            {
                Numerator = currentRefreshRate.numerator,
                Denominator = currentRefreshRate.denominator
            }
        };

        // Return the settings
        return settings;
    }

    public void ResetWorkingScreenSettings()
    {
        // Reset the working screen settings to the current screen settings
        _workingScreenSettings = GetCurrentScreenSettings();

        // Reset the previous screen settings to the current screen settings
        _previousScreenSettings = _workingScreenSettings;
    }

    public void ApplyWorkingSettings()
    {
        // Create a refresh rate object from the working screen settings
        RefreshRate refreshRate = new()
        {
            numerator = _workingScreenSettings.RefreshRate.Numerator,
            denominator = _workingScreenSettings.RefreshRate.Denominator
        };

        // Apply the working screen settings to the actual screen settings
        Screen.SetResolution(
            _workingScreenSettings.Resolution.Width,
            _workingScreenSettings.Resolution.Height,
            _workingScreenSettings.FullScreenMode,
            refreshRate
        );

        QualitySettings.SetQualityLevel(_workingScreenSettings.QualityLevel);
        QualitySettings.vSyncCount = _workingScreenSettings.IsVSync ? 1 : 0;
        QualitySettings.antiAliasing = _workingScreenSettings.AntiAliasing;

        Debug.Log("Applied working screen settings");
    }
}