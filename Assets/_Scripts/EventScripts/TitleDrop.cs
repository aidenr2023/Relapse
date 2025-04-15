using UnityEngine;
using System.Collections;

public class TitleLogoController : MonoBehaviour
{
    [Header("Music Settings")]
    public AudioSource mainMusic;      // Reference to the main music AudioSource.
    public float beatDropTime = 15.0f;   // Time (in seconds) when the beat drop occurs.

    [Header("Animation Options")]
    public bool useAnimator = false;     // Set to true if using an Animator instead of the simple coroutine.
    public Animator animator;            // Animator component (if using Animator).
    public float animationDuration = 1.0f; // Duration for scaling and fading in.

    [Header("Scale Settings")]
    public Vector3 startScale = new Vector3(10f, 10f, 10f); // Start big!
    public Vector3 targetScale = Vector3.one;               // Shrink down to normal.

    private bool animated = false;       // To prevent re-triggering.
    private CanvasGroup canvasGroup;     // For controlling opacity.

    void Start()
    {
        // Try to get a CanvasGroup, and add one if it doesn't exist.
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        if (!useAnimator)
        {
            // Initialize starting state: super-sized and completely transparent.
            transform.localScale = startScale;
            canvasGroup.alpha = 0f;
        }
    }

    void Update()
    {
        // Check if the beat drop has been reached.
        if (!animated && mainMusic != null && mainMusic.time >= beatDropTime)
        {
            animated = true;
            if (useAnimator && animator != null)
            {
                // If using an Animator, trigger the "BeatDrop" state.
                animator.SetTrigger("BeatDrop");
            }
            else
            {
                // Otherwise, animate via the coroutine.
                StartCoroutine(AnimateScaleAndFade());
            }
        }
    }

    IEnumerator AnimateScaleAndFade()
    {
        float elapsedTime = 0f;
        while (elapsedTime < animationDuration)
        {
            // Calculate the interpolation factor.
            float t = elapsedTime / animationDuration;

            // Lerp the scale from startScale to targetScale.
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);

            // Lerp the opacity from transparent (0) to opaque (1).
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final values are set.
        transform.localScale = targetScale;
        canvasGroup.alpha = 1f;
    }
}
