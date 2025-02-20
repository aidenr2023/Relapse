using System.Collections;
using UnityEngine;

public class LevelTransitionCheckpoint : LevelCheckpointReset
{
    private const float TRANSITION_TIME = .5f;
    private const float HOLD_TIME = 1f;
    
    protected override void CustomOnTriggerEnter(Collider other, Player player)
    {
        Debug.Log($"Entered the level transition checkpoint with player {player.name}");

        // Run the transition to the next scene coroutine
        DebugManagerHelper.Instance.StartCoroutine(TransitionToNextScene());
    }

    private static IEnumerator TransitionToNextScene()
    {
        // Disable the player's controls
        SetPlayerControls(Player.Instance, false);

        // Get the unscaled start time
        var startTime = Time.unscaledTime;

        // While transitioning, fade the screen to black
        while (Time.unscaledTime - startTime < TRANSITION_TIME)
        {
            var lerpValue = (Time.unscaledTime - startTime) / TRANSITION_TIME;
            TransitionOverlay.Instance.SetOpacity(lerpValue);

            yield return null;
        }

        TransitionOverlay.Instance.SetOpacity(1);

        // Move the player, reset the player to the checkpoint
        LevelCheckpointManager.Instance.ResetToCheckpoint(LevelCheckpointManager.Instance.CurrentCheckpoint);

        // Wait for the hold time
        yield return new WaitForSecondsRealtime(HOLD_TIME);

        // Enable the player's controls
        SetPlayerControls(Player.Instance, true);
        
        startTime = Time.unscaledTime;

        // While transitioning, fade the screen to black
        while (Time.unscaledTime - startTime < TRANSITION_TIME)
        {
            var lerpValue = (Time.unscaledTime - startTime) / TRANSITION_TIME;
            TransitionOverlay.Instance.SetOpacity(1 - lerpValue);

            yield return null;
        }
        
        TransitionOverlay.Instance.SetOpacity(0);
    }

    private static void SetPlayerControls(Player player, bool isOn)
    {
        var playerMovementV2 = player.PlayerController as PlayerMovementV2;
        
        // Return if the player movement is null
        if (playerMovementV2 == null)
            return;
        
        if (isOn)
            playerMovementV2.EnablePlayerControls();
        else
            playerMovementV2.DisablePlayerControls();
        
        player.WeaponManager.enabled = isOn;
    }
}