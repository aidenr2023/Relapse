using System;
using UnityEngine;

[Serializable]
public class DynamicOffsetModule : DynamicVCamModule
{
    #region Serialized Fields

    [SerializeField] private Vector3 defaultOffset = Vector3.zero;

    #endregion

    #region Private Fields

    private CinemachineCameraOffset _cameraOffset;

    private TokenManager<Vector3> _offsetTokens;

    #endregion

    #region Getters

    public TokenManager<Vector3> OffsetTokens => _offsetTokens;

    #endregion

    protected override void CustomInitialize(PlayerVirtualCameraController controller)
    {
        // Initialize the token manager
        _offsetTokens = new(false, null, Vector3.zero);
    }

    public override void Start()
    {
        // Get the camera offset component
        _cameraOffset = playerVCamController.VirtualCamera.GetComponent<CinemachineCameraOffset>();
    }

    public override void Update()
    {
        // Update the token manager
        _offsetTokens.Update(Time.deltaTime);

        // Calculate the new offset
        var newOffset = defaultOffset + CurrentTokenValue();

        // Set the new offset
        _cameraOffset.m_Offset = newOffset;
    }

    private Vector3 CurrentTokenValue()
    {
        var value = Vector3.zero;

        foreach (var token in _offsetTokens.Tokens)
            value += token.Value;

        return value;
    }
}