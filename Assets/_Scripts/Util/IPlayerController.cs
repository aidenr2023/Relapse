using UnityEngine;

public interface IPlayerController
{
    /// <summary>
    /// The game object that is used to determine which way the camera is facing.
    /// </summary>
    public GameObject CameraPivot { get; }

    public bool IsGrounded { get; }
}