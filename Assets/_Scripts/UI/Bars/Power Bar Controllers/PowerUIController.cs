﻿using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUIController : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private PowerListReference currentPowers;
    [SerializeField] private IntReference currentPowerIndex;
    
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image currentPowerImage;
    [SerializeField] private Image currentPowerImageBg;
    [SerializeField] private UIJitter[] currentPowerJitters;

    [SerializeField] private PowerIconGroup powerIconGroup;
    [SerializeField] private PowerIconGroup powerIconGroupMiddle;

    [SerializeField, Range(0, 1)] private float powerIconsMinOpacity = .75f;
    [SerializeField] private float powerIconsOpacityLerpAmount = .1f;
    [SerializeField] private float powerIconsStayOnScreenTime = .5f;
    [SerializeField] private TMP_Text powerNameText;

    [SerializeField] private float drugRotation = -45;
    [SerializeField] private float medRotation = 45;

    [Space, SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = Color.black;

    [SerializeField] private Color disabledColor = Color.HSVToRGB(0, 0, .25f);

    #endregion

    #region Private Fields

    private int _previousPowerIndex;

    private Vector3 _currentPowerIconsOffset;

    #endregion

    #region Getters

    public float PowerIconsOpacityLerpAmount => powerIconsOpacityLerpAmount;

    public Color SelectedColor => selectedColor;

    public Color UnselectedColor => unselectedColor;

    #endregion

    private void Update()
    {
        // If the player has no powers, turn the canvas group's opacity to 0
        if (currentPowers.Value.Count == 0)
            canvasGroup.alpha = 0;
        else
            canvasGroup.alpha = 1;

        PowerScriptableObject currentPower = null;
        
        if (currentPowers.Value.Count > 0)
            currentPower = currentPowers.Value[currentPowerIndex.Value];
        
        // If the current power is not null
        if (currentPower != null)
        {
            currentPowerImage.sprite = currentPower.Icon;
            currentPowerImageBg.sprite = currentPower.Icon;

            // TODO: Remove the dependency on the player power manager
            var pToken = Player.Instance.PlayerPowerManager.CurrentPowerToken;

            foreach (var jitter in currentPowerJitters)
                jitter.enabled = !pToken.IsCoolingDown && !pToken.IsActiveEffectOn && !pToken.IsPassiveEffectOn;

            if (pToken.IsCoolingDown)
                currentPowerImage.color = new Color(disabledColor.r, disabledColor.g, disabledColor.b,
                    currentPowerImage.color.a);
            else
                currentPowerImage.color = new Color(1, 1, 1, currentPowerImage.color.a);
        }

        powerIconGroup?.UpdateIcons(this);
        powerIconGroupMiddle?.UpdateIcons(this);
    }
}