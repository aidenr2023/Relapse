using System.Collections;
using UnityEngine;

public class CameraShakeHelper : MonoBehaviour
{
    [SerializeField] private DynamicNoiseModule.NoiseTokenValue noiseToken;
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeFadeDuration = 0.5f;

    public void ShakeCamera()
    {
        var cameraController = Player.Instance.PlayerVirtualCameraController;

        // Start the coroutine
        cameraController.StartCoroutine(
            CameraShakeCoroutine(cameraController.DynamicNoiseModule, noiseToken, shakeDuration, shakeFadeDuration)
        );
    }

    private static IEnumerator CameraShakeCoroutine(
        DynamicNoiseModule noiseModule, DynamicNoiseModule.NoiseTokenValue noiseToken,
        float duration, float shakeFadeDuration
    )
    {
        // Create a copy of the token
        var newTokenValue = new DynamicNoiseModule.NoiseTokenValue(
            noiseToken.PivotOffset, noiseToken.AmplitudeGain, noiseToken.FrequencyGain
        );

        // Add the token to the dynamic noise module (INFINITE DURATION)
        var token = noiseModule.NoiseTokens.AddToken(newTokenValue, -1, true);

        // Wait for the duration
        yield return new WaitForSeconds(duration);

        var startTime = Time.time;

        // Fade the token value to 0
        while (Time.time - startTime < shakeFadeDuration)
        {
            var lerpValue = Mathf.InverseLerp(startTime, startTime + shakeFadeDuration, Time.time);

            token.Value = DynamicNoiseModule.NoiseTokenValue.Lerp(
                newTokenValue, DynamicNoiseModule.NoiseTokenValue.Zero, lerpValue
            );

            yield return null;
        }

        // Remove the token from the dynamic noise module
        noiseModule.NoiseTokens.RemoveToken(token);
    }
}