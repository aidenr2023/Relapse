using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class JournalUIPowerItem : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private PowerScriptableObject power;

    [SerializeField] private TMP_Text powerNameText;
    [SerializeField] private Image powerImage;

    [SerializeField] private Button button;

    [SerializeField] private EventTrigger eventTrigger;

    #endregion

    #region Getters

    public PowerScriptableObject Power => power;

    public Button Button => button;

    public EventTrigger EventTrigger => eventTrigger;

    #endregion

    private void Update()
    {
        if (power == null)
            throw new Exception("Power not set");

        // Update the power item data
        UpdatePowerItemData();
    }

    private void UpdatePowerItemData()
    {
        powerNameText.text = power.PowerName;
        powerImage.sprite = power.Icon;
    }

    public void SetPower(PowerScriptableObject power)
    {
        this.power = power;
    }
}