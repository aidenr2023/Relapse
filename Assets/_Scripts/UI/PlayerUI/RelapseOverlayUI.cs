using System;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class RelapseOverlayUI : MonoBehaviour
{
    [SerializeField] private BoolReference isRelapsing;

    [SerializeField, Min(0)] private float duration = 1;
    [SerializeField, Min(0)] private float notRelapsingLerpAmount = .15f;
    [SerializeField] private AnimationCurve opacityCurve;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        // Get the CanvasGroup component
        _canvasGroup = GetComponent<CanvasGroup>();

        // Set the alpha to 0
        _canvasGroup.alpha = 0;
    }

    private void Update()
    {
        float opacity;

        if (!isRelapsing.Value)
            opacity = Mathf.Lerp(_canvasGroup.alpha, 0, CustomFunctions.FrameAmount(notRelapsingLerpAmount));

        else
        {
            var timeValue = Time.time % duration;
            opacity = opacityCurve.Evaluate(timeValue);
        }

        _canvasGroup.alpha = opacity;
    }
}