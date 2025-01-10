using UnityEngine;

public class BlackScreenOverlayHandler : MonoBehaviour
{
    private const float THRESHOLD = 0.0001f;

    public static BlackScreenOverlayHandler Instance { get; private set; }

    #region Serialized Fields

    [SerializeField] private CanvasGroup overlayCanvasGroup;

    [SerializeField, Min(0)] private float darkenLerpAmount = 0.2f;
    [SerializeField, Min(0)] private float brightenLerpAmount = 0.5f;

    [SerializeField, Range(0, 1)] private float minAlpha = 0f;
    [SerializeField, Range(0, 1)] private float maxAlpha = 1f;

    #endregion

    #region Private Fields

    private float _desiredAlpha;

    #endregion

    private void Awake()
    {
        // Initialize the instance
        Instance = this;

        // Set the alpha of the overlay canvas group to 0
        overlayCanvasGroup.alpha = 0;
    }

    private void Update()
    {
        const float defaultFrameTime = 1 / 60f;
        var frameAmount = Time.unscaledDeltaTime / defaultFrameTime;

        // Calculate the lerp value
        var cLerpValue = _desiredAlpha > overlayCanvasGroup.alpha ? darkenLerpAmount : brightenLerpAmount;

        // Lerp the alpha of the overlay canvas group
        overlayCanvasGroup.alpha = Mathf.Lerp(overlayCanvasGroup.alpha, _desiredAlpha, cLerpValue * frameAmount);

        // If the value is close enough, set it to the desired value
        if (Mathf.Abs(overlayCanvasGroup.alpha - _desiredAlpha) < THRESHOLD)
            overlayCanvasGroup.alpha = _desiredAlpha;
    }

    private void SetOverlayAlpha(float alpha)
    {
        _desiredAlpha = alpha;
    }

    public void DarkenScreen()
    {
        SetOverlayAlpha(maxAlpha);
    }

    public void LightenScreen()
    {
        SetOverlayAlpha(minAlpha);
    }
}