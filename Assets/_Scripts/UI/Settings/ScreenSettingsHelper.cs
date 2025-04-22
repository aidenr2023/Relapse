using UnityEngine;

public class ScreenSettingsHelper : MonoBehaviour
{
    public void SetFullscreenMode(int mode)
    {
        // If the fullscreen mode is already set to the desired value, do nothing
        if (Screen.fullScreenMode == (FullScreenMode)mode)
            return;

        // Set the fullscreen mode
        Screen.fullScreenMode = (FullScreenMode)mode;

        Debug.Log($"Set fullscreen mode to {(FullScreenMode)mode}");
    }

    public void SetResolution(int width, int height, FullScreenMode fullScreenMode)
    {
        // If the resolution is already set to the desired values, do nothing
        if (Screen.width == width && Screen.height == height && Screen.fullScreenMode == fullScreenMode)
            return;

        // Set the resolution and fullscreen mode
        Screen.SetResolution(width, height, fullScreenMode);
    }

    public void SetQualityLevel(int qualityLevel)
    {
        // If the quality level is already set to the desired value, do nothing
        if (QualitySettings.GetQualityLevel() == qualityLevel)
            return;

        // Set the quality level
        QualitySettings.SetQualityLevel(qualityLevel);
    }

    public void SetVSync(bool isOn)
    {
        // If VSync is already in the desired state, do nothing
        if (QualitySettings.vSyncCount == (isOn ? 1 : 0))
            return;

        // Set VSync
        QualitySettings.vSyncCount = isOn ? 1 : 0;
    }

    public void SetAntiAliasing(int level)
    {
        // If the anti-aliasing level is already set to the desired value, do nothing
        if (QualitySettings.antiAliasing == level)
            return;

        // Set the anti-aliasing level
        QualitySettings.antiAliasing = level;
    }

    public void SetRefreshRate(RefreshRate rate)
    {
        // Set the refresh rate
        Screen.SetResolution(Screen.width, Screen.height, Screen.fullScreenMode, rate);
    }
}