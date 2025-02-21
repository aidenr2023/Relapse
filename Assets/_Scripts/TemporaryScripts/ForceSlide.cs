using UnityEngine;
using System.Collections;

public class ForcedSlideTrigger : MonoBehaviour
{
    [Tooltip("Duration (in seconds) to force the slide if no exit trigger is encountered.")]
    public float forcedSlideDuration = 5f;

    [Tooltip("Optional: Assign a collider from an exit trigger that, when the player enters it, ends the forced slide early.")]
    public Collider exitTriggerCollider;

    private bool isForcedSlideActive = false;
    private GameObject player;
    private PlayerMovementV2 playerMovement;
    private Coroutine forcedSlideCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        if (isForcedSlideActive)
            return;

        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            playerMovement = player.GetComponent<PlayerMovementV2>();
            if (playerMovement == null)
            {
                Debug.LogWarning("PlayerMovementV2 component not found on the player!");
                return;
            }
            StartForcedSlide();
        }
    }

    private void StartForcedSlide()
    {
        isForcedSlideActive = true;

        // Disable the main player movement component to block normal input.
        playerMovement.enabled = false;

        // Force the slide by setting the Animator parameter.
        if (playerMovement.PlayerAnimator != null)
        {
            playerMovement.PlayerAnimator.SetBool("IsSliding", true);
        }
        else
        {
            Debug.LogWarning("PlayerAnimator not found on the player!");
        }

        // Start monitoring for exit or timeout.
        if (forcedSlideCoroutine != null)
            StopCoroutine(forcedSlideCoroutine);
        forcedSlideCoroutine = StartCoroutine(ForcedSlideRoutine());
    }

    private IEnumerator ForcedSlideRoutine()
    {
        float elapsed = 0f;
        while (elapsed < forcedSlideDuration)
        {
            // If an exit trigger collider is set, check if the player is within its bounds.
            if (exitTriggerCollider != null && exitTriggerCollider.bounds.Contains(player.transform.position))
            {
                break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        EndForcedSlide();
    }

    private void EndForcedSlide()
    {
        if (playerMovement != null)
        {
            // Re-enable player movement to restore normal input.
            playerMovement.enabled = true;

            // Reset the slide animation parameter.
            if (playerMovement.PlayerAnimator != null)
            {
                playerMovement.PlayerAnimator.SetBool("IsSliding", false);
            }
        }

        isForcedSlideActive = false;
        player = null;
        playerMovement = null;
    }
}
