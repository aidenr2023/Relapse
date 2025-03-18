using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrightnessSlider : MonoBehaviour
{
    public void ChangeGamma(float value)
    {
        // UserSettings.Instance.SetGamma(value);
        Debug.Log("Gamma Value: " + value);
    }
    
    
}
