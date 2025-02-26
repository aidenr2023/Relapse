using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CameraShakeHelper : MonoBehaviour
{
    [SerializeField] private DynamicNoiseModule.NoiseTokenValue noiseToken;
    [SerializeField] private float shakeDuration = 0.5f;

    private readonly HashSet<TokenManager<DynamicNoiseModule.NoiseTokenValue>.ManagedToken> tokens =
        new();

    public void ShakeCamera()
    {
        // Add the token to the dynamic noise module
        var token = Player.Instance
            .PlayerVirtualCameraController
            .DynamicNoiseModule
            .NoiseTokens
            .AddToken(noiseToken, shakeDuration);

        // Add the token to the list of tokens
        tokens.Add(token);
    }

    private void Update()
    {
        // Convert the list of tokens to an array
        var tokensArray = tokens.ToArray();
        
        // Update each token
        foreach (var token in tokensArray)
        {
            UpdateToken(token);
            
            // If the token has expired, remove it from the list of tokens
            if (token.timer.IsComplete)
                tokens.Remove(token);
        }
    }

    private static void UpdateToken(TokenManager<DynamicNoiseModule.NoiseTokenValue>.ManagedToken token)
    {
        // Create a copy of the token, but with 0 amplitude
        var zeroToken = new DynamicNoiseModule.NoiseTokenValue(
            token.Value.PivotOffset, 0, token.Value.FrequencyGain
        );

        // Lerp the token value to 0
        token.Value = DynamicNoiseModule.NoiseTokenValue.Lerp(
            token.Value,
            zeroToken,
            token.timer.Percentage
        );
    }
}