using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrightnessSlider : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ChangeGamma(float value)
    {
        UserSettings.Instance.SetGamma(value);
        Debug.Log("Gamma Value: " + value);
    }
    
    
}
