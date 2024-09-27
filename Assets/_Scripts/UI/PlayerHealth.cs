using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public Slider healthSlider;
    public Slider easeHealthSlider;
    public Playerinfo Playerinfo;
    private float lerpSpeed = 0.02f;

    // Start is called before the first frame update
    void Start()
    {
        healthSlider.maxValue = Playerinfo.MaxHealth;
        easeHealthSlider.maxValue = Playerinfo.MaxHealth;

        healthSlider.value = Playerinfo.CurrentHealth;
        easeHealthSlider.value = Playerinfo.CurrentHealth;
    }

    // Update is called once per frame
    void Update()
    {
        healthSlider.value = Playerinfo.CurrentHealth;

        if (healthSlider.value != easeHealthSlider.value)
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, Playerinfo.CurrentHealth, lerpSpeed);
    }
}