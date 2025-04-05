using UnityEngine;

public static class EnemyRelapseOutlineManager
{
    private const string LERP_AMOUNT_KEY = "_EnemyAlternateLerp";
    private static readonly int EnemyAlternateLerpID = Shader.PropertyToID(LERP_AMOUNT_KEY);

    public static void SetOutlineLerpAmount(float amount)
    {
        // Clamp the amount to the range of 0 to 1
        amount = Mathf.Clamp01(amount);

        // Set the global shader property for the outline lerp amount
        Shader.SetGlobalFloat(EnemyAlternateLerpID, amount);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void ResetLerpAmount()
    {
        Debug.Log("Resetting enemy outline lerp amount to 0");
        
        // Reset the lerp amount to 0
        SetOutlineLerpAmount(0f);
    }
}