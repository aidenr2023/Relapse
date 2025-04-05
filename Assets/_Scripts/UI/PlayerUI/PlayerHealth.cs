using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    private const float LERP_SPEED = 0.02f;

    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider easeHealthSlider;

    private PlayerInfo PlayerInfo => Player.Instance.PlayerInfo;

    // Start is called before the first frame update
    void Start()
    {
        healthSlider.minValue = 0;
        easeHealthSlider.minValue = 0;

        healthSlider.maxValue = PlayerInfo.MaxHealth;
        easeHealthSlider.maxValue = PlayerInfo.MaxHealth;

        healthSlider.value = PlayerInfo.CurrentHealth;
        easeHealthSlider.value = PlayerInfo.CurrentHealth;
    }

    // Update is called once per frame
    void Update()
    {
        healthSlider.value = PlayerInfo.CurrentHealth;

        if (!Mathf.Approximately(healthSlider.value, easeHealthSlider.value))
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, PlayerInfo.CurrentHealth, LERP_SPEED);
    }
}