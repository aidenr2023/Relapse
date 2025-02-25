using System;
using UnityEngine;
using UnityEngine.UI;

public class PowerIconController : MonoBehaviour
{
    #region Serialized Fields

    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private GameObject parentObject;
    [SerializeField] private Image bgImage;
    [SerializeField] private Image fgImage;

    [SerializeField, Min(0)] private float visibilityLerpAmount = .1f;
    [SerializeField, Min(0)] private float colorLerpAmount = .1f;

    #endregion

    #region Private Fields

    private bool _isVisible = true;

    private Color _targetColor;

    #endregion

    private void Start()
    {
        _targetColor = bgImage.color;
    }

    private void Update()
    {
        var desiredAlpha = _isVisible ? 1 : 0;

        // Lerp the alpha
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, desiredAlpha,
            CustomFunctions.FrameAmount(visibilityLerpAmount, false, true));

        // Lerp the color
        bgImage.color = Color.Lerp(bgImage.color, _targetColor,
            CustomFunctions.FrameAmount(colorLerpAmount, false, true));
    }

    public void SetFill(float amount)
    {
        amount = Mathf.Clamp01(amount);

        // Set the fill amount
        bgImage.fillAmount = amount;
    }

    public void SetForeground(Sprite sprite)
    {
        fgImage.sprite = sprite;
    }

    public void SetVisible(bool visible)
    {
        _isVisible = visible;
    }

    public void SetColor(Color color)
    {
        // Set the target color
        _targetColor = new Color(color.r, color.g, color.b, bgImage.color.a);
    }

    public void LerpScale(float scale, float lerpSpeed)
    {
        // Lerp the scale
        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.unscaledDeltaTime / defaultFrameTime;

        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * scale, lerpSpeed * frameAmount);

        // If the scale is within a certain range, set scale to the scale
        if (Mathf.Abs(transform.localScale.x - scale) < .01f)
            transform.localScale = Vector3.one * scale;
    }

    public void SetRotation(float rotation)
    {
        // Set the rotation
        fgImage.transform.localEulerAngles = new Vector3(0, 0, rotation);
    }
}