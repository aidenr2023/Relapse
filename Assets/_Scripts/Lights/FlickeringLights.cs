using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlickeringLights : MonoBehaviour
{
    private Light light01;
    public float minTime;
    public float maxTime;
    public float timer;
    public float maxIntensity = 1;
    public float minIntensity = 0;
    private bool enabled;

    void Awake()
    {
        light01 = GetComponent<Light>();
    }

    
    void Start()
    {
        timer = Random.Range(minTime, maxTime);
    }

    // Update is called once per frame
    void Update()
    {
        LightsFlickering();
    }

    void LightsFlickering()
    {
        if (timer > 0)
        timer -= Time.deltaTime;

        if(timer <= 0)
        {
            enabled = !enabled;
        if (enabled)
        {
            light01.intensity = maxIntensity;
        }
        else
        {
            light01.intensity = minIntensity;
        }



            timer = Random.Range(minTime, maxTime);
        }
    }

}
