using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public Slider healthSlider;
    public Slider easeHealthSlider;
    public PlayerInfo playerInfo;
    private float lerpSpeed = 0.02f;

    // Start is called before the first frame update
    void Start()
    {
        healthSlider.maxValue = playerInfo.MaxHealth;
        easeHealthSlider.maxValue = playerInfo.MaxHealth;

        healthSlider.value = playerInfo.CurrentHealth;
        easeHealthSlider.value = playerInfo.CurrentHealth;
    }

    // Update is called once per frame
    void Update()
    {
        healthSlider.value = playerInfo.CurrentHealth;

        if (healthSlider.value != easeHealthSlider.value)
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, playerInfo.CurrentHealth, lerpSpeed);
    }
}