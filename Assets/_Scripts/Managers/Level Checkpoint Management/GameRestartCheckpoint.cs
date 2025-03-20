using System.Collections;
using UnityEngine;

public class GameRestartCheckpoint : LevelTransitionCheckpoint
{
    [SerializeField] private EventVariable onGameRestart;

    protected override IEnumerator DoSomethingWhileScreenIsWhite()
    {
        Debug.Log("Game Restart checkpoint");

        // Invoke the onGameRestartEvent
        onGameRestart?.Invoke();

        yield return null;
    }
}