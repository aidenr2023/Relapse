using System;
using UnityEngine;

public class MedUsedUIGroup : MonoBehaviour
{
    private const float OPACITY_THRESHOLD = 0.001f;

    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField, Range(0, 1)] private float maxOpacity = 1f;
    [SerializeField, Range(0, 1)] private float opacityLerpAmount = .15f;

    private float _desiredOpacity;

    private void Awake()
    {
        canvasGroup.alpha = 0;
        _desiredOpacity = 0;
    }

    private void Update()
    {
        // Lerp the alpha of the canvas group
        canvasGroup.alpha =
            Mathf.Lerp(canvasGroup.alpha, _desiredOpacity, CustomFunctions.FrameAmount(opacityLerpAmount));

        // If the difference between the current alpha and the desired alpha is less than the threshold, set the current alpha to the desired alpha
        if (Mathf.Abs(canvasGroup.alpha - _desiredOpacity) < OPACITY_THRESHOLD)
            canvasGroup.alpha = _desiredOpacity;

        // If the alpha is 1, set the desired alpha to 0
        if (Mathf.Approximately(canvasGroup.alpha, maxOpacity))
            _desiredOpacity = 0;
    }

    private void OnPowerUsed(PlayerPowerManager arg1, PowerToken arg2)
    {
        // If the power used is not a medicine, return
        if (arg2.PowerScriptableObject.PowerType != PowerType.Medicine)
            return;

        // Set the desired opacity to the max opacity
        _desiredOpacity = maxOpacity;
    }
}